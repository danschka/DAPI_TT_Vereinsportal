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
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(
            "TT Vereinsportal",
            _configuration["EmailSettings:FromEmail"]));

        message.To.Add(new MailboxAddress(
            "Verein",
            _configuration["EmailSettings:ToEmail"]));

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
            _configuration["EmailSettings:SmtpServer"],
            int.Parse(_configuration["EmailSettings:SmtpPort"]!),
            SecureSocketOptions.StartTls);

        await client.AuthenticateAsync(
            _configuration["EmailSettings:Username"],
            _configuration["EmailSettings:Password"]);

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}