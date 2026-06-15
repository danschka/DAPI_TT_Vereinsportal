using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace TT_Website.Services;

public record EmailConfigurationStatus(
    bool IsConfigured,
    string Message,
    IReadOnlyList<string> Problems);

public class EmailService
{
    private readonly IConfiguration _configuration;
    private readonly SiteSettingsService _siteSettingsService;

    public EmailService(
        IConfiguration configuration,
        SiteSettingsService siteSettingsService)
    {
        _configuration = configuration;
        _siteSettingsService = siteSettingsService;
    }

    public async Task SendMemberApplicationEmailAsync(
        string firstName,
        string lastName,
        string email,
        string phone,
        DateTime birthDate,
        string address)
    {
        var settings = await GetValidatedSettingsAsync();

        var body =
$@"Es wurde eine neue Mitgliedsanfrage eingereicht:

Name: {firstName} {lastName}
E-Mail: {email}
Telefon: {phone}
Geburtsdatum: {birthDate:dd.MM.yyyy}
Adresse: {address}

Eingereicht am: {DateTime.Now:dd.MM.yyyy HH:mm}";

        await SendPlainTextEmailAsync(settings, "Neue Mitgliedsanfrage", body);
    }

    public async Task SendMasterDataChangeEmailAsync(
        string salutation,
        string firstName,
        string lastName,
        string email,
        string phone,
        string address,
        string changeDetails,
        string messageText)
    {
        var settings = await GetValidatedSettingsAsync();

        var body =
$@"Es wurde eine Stammdatenänderung eingereicht:

Anrede: {salutation}
Name: {firstName} {lastName}
E-Mail: {email}
Telefon: {phone}
Adresse: {address}
Änderung: {changeDetails}

Mitteilung:
{messageText}

Eingereicht am: {DateTime.Now:dd.MM.yyyy HH:mm}";

        await SendPlainTextEmailAsync(settings, "Stammdatenänderung", body);
    }

    public async Task<EmailConfigurationStatus> GetConfigurationStatusAsync()
    {
        var problems = new List<string>();

        var fromEmail = _configuration["EmailSettings:FromEmail"];
        var smtpServer = _configuration["EmailSettings:SmtpServer"];
        var username = _configuration["EmailSettings:Username"];

        AddIfInvalidEmail(problems, "Absender-E-Mail", fromEmail);

        var recipientEmail = await _siteSettingsService.GetValueAsync(SiteSettingsService.MemberApplicationRecipientEmail);
        if (!IsValidEmail(recipientEmail))
            AddIfInvalidEmail(problems, "Ziel-E-Mail", _configuration["EmailSettings:ToEmail"]);

        AddIfEmpty(problems, "SMTP-Server", smtpServer);
        AddIfInvalidPort(problems, "SMTP-Port", _configuration["EmailSettings:SmtpPort"]);
        AddIfInvalidEmail(problems, "SMTP-Benutzername als vollständige E-Mail-Adresse", username);
        AddIfSenderAndUsernameDoNotMatch(problems, fromEmail, username);
        AddIfGmailServerDoesNotUseGmailAccount(problems, smtpServer, username);
        AddIfEmpty(problems, "SMTP-Passwort", _configuration["EmailSettings:Password"]);

        return problems.Count == 0
            ? new EmailConfigurationStatus(true, "E-Mail-Versand ist vollständig konfiguriert.", [])
            : new EmailConfigurationStatus(false, "E-Mail-Versand ist noch nicht vollständig konfiguriert.", problems);
    }

    private async Task<EmailSettings> GetValidatedSettingsAsync()
    {
        var status = await GetConfigurationStatusAsync();

        if (!status.IsConfigured)
        {
            throw new InvalidOperationException(
                $"{status.Message} Fehlend oder ungültig: {string.Join(", ", status.Problems)}.");
        }

        return new EmailSettings(
            GetRequiredEmailSetting("EmailSettings:FromEmail"),
            await GetRecipientEmailAsync(),
            GetRequiredSetting("EmailSettings:SmtpServer"),
            GetRequiredPortSetting("EmailSettings:SmtpPort"),
            GetRequiredSetting("EmailSettings:Username"),
            GetRequiredSetting("EmailSettings:Password"));
    }

    private static async Task SendPlainTextEmailAsync(
        EmailSettings settings,
        string subject,
        string body)
    {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(
            "TT Vereinsportal",
            settings.FromEmail));

        message.To.Add(new MailboxAddress(
            "Verein",
            settings.ToEmail));

        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        using var client = new SmtpClient();

        try
        {
            await client.ConnectAsync(
                settings.SmtpServer,
                settings.SmtpPort,
                SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(
                settings.Username,
                settings.Password.Replace(" ", ""));

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"SMTP-Versand fehlgeschlagen: {ex.Message}", ex);
        }
    }

    private string GetRequiredSetting(string key)
    {
        var value = _configuration[key];

        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Die Einstellung '{key}' fehlt.");

        return value;
    }

    private string GetRequiredEmailSetting(string key)
    {
        var value = GetRequiredSetting(key);

        if (!IsValidEmail(value))
            throw new InvalidOperationException($"Die Einstellung '{key}' ist keine gültige E-Mail-Adresse.");

        return value;
    }

    private int GetRequiredPortSetting(string key)
    {
        var value = GetRequiredSetting(key);

        if (!int.TryParse(value, out var port) || port <= 0)
            throw new InvalidOperationException($"Die Einstellung '{key}' ist kein gültiger SMTP-Port.");

        return port;
    }

    private async Task<string> GetRecipientEmailAsync()
    {
        var databaseEmail = await _siteSettingsService.GetValueAsync(SiteSettingsService.MemberApplicationRecipientEmail);

        if (IsValidEmail(databaseEmail))
            return databaseEmail!;

        return GetRequiredEmailSetting("EmailSettings:ToEmail");
    }

    private static bool IsValidEmail(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        try
        {
            _ = new System.Net.Mail.MailAddress(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void AddIfEmpty(List<string> problems, string label, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            problems.Add(label);
    }

    private static void AddIfInvalidEmail(List<string> problems, string label, string? value)
    {
        if (!IsValidEmail(value))
            problems.Add(label);
    }

    private static void AddIfSenderAndUsernameDoNotMatch(List<string> problems, string? fromEmail, string? username)
    {
        if (!IsValidEmail(fromEmail) || !IsValidEmail(username))
            return;

        if (!string.Equals(fromEmail, username, StringComparison.OrdinalIgnoreCase))
            problems.Add("Absender-E-Mail und SMTP-Benutzername sollten identisch sein");
    }

    private static void AddIfGmailServerDoesNotUseGmailAccount(List<string> problems, string? smtpServer, string? username)
    {
        if (string.IsNullOrWhiteSpace(smtpServer) ||
            !smtpServer.Contains("gmail.com", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(username))
        {
            return;
        }

        if (!username.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase) &&
            !username.EndsWith("@googlemail.com", StringComparison.OrdinalIgnoreCase))
        {
            problems.Add("Bei smtp.gmail.com muss der SMTP-Benutzer normalerweise eine Gmail-Adresse sein");
        }
    }

    private static void AddIfInvalidPort(List<string> problems, string label, string? value)
    {
        if (!int.TryParse(value, out var port) || port <= 0)
            problems.Add(label);
    }

    private sealed record EmailSettings(
        string FromEmail,
        string ToEmail,
        string SmtpServer,
        int SmtpPort,
        string Username,
        string Password);
}
