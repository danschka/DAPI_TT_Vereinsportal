using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace TT_Website.Services;

public class EmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendMemberApplicationEmailAsync(
        string firstName,
        string lastName,
        string email,
        string phone,
        DateTime birthDate,
        string address)
    {
        var fromEmail = GetRequiredSetting("EmailSettings:FromEmail");
        var toEmail = GetRequiredSetting("EmailSettings:ToEmail");
        var smtpServer = GetRequiredSetting("EmailSettings:SmtpServer");
        var smtpPort = int.Parse(GetRequiredSetting("EmailSettings:SmtpPort"));
        var username = GetRequiredSetting("EmailSettings:Username");
        var password = GetRequiredSetting("EmailSettings:Password");

        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(
            "TT Vereinsportal",
            fromEmail));

        message.To.Add(new MailboxAddress(
            "Verein",
            toEmail));

        message.Subject = "Neue Mitgliedsanfrage";

        message.Body = new TextPart("plain")
        {
            Text =
$@"Es wurde eine neue Mitgliedsanfrage eingereicht:

Name: {firstName} {lastName}
E-Mail: {email}
Telefon: {phone}
Geburtsdatum: {birthDate:dd.MM.yyyy}
Adresse: {address}

Eingereicht am: {DateTime.Now:dd.MM.yyyy HH:mm}"
        };

        using var client = new SmtpClient();

        await client.ConnectAsync(
            smtpServer,
            smtpPort,
            SecureSocketOptions.StartTls);

        await client.AuthenticateAsync(
            username,
            password);

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    private string GetRequiredSetting(string key)
    {
        var value = _configuration[key];

        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Die Einstellung '{key}' fehlt.");

        return value;
    }
}
