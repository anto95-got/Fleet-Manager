using System;
using System.Net;
using System.Net.Mail;

namespace FleetManager.Services;

public static class EmailService
{
    // ============================================================
    // 👇 METS TES INFOS GMAIL ICI
    // ============================================================
    private const string MonEmail = "fleetmanager917@gmail.com";
    private const string MonMotDePasseApp = "swdtztlnrvdwhdql"; 
    // ============================================================

    public static void EnvoyerIdentifiants(string emailDestinataire, string nom, string motDePasseClair)
    {
        try
        {
            using var smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(MonEmail, MonMotDePasseApp),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            var message = new MailMessage
            {
                From = new MailAddress(MonEmail, "Admin Fleet Manager"),
                Subject = "Vos identifiants de connexion",
                Body = $@"
                    <h3>Bonjour {nom},</h3>
                    <p>Un administrateur vient de vous créer un compte sur Fleet Manager.</p>
                    <p>Voici vos identifiants :</p>
                    <ul>
                        <li><b>Email :</b> {emailDestinataire}</li>
                        <li><b>Mot de passe :</b> {motDePasseClair}</li>
                    </ul>
                    <p>Merci de changer ce mot de passe dès que possible.</p>",
                IsBodyHtml = true
            };

            message.To.Add(emailDestinataire);
            smtp.Send(message);
        }
        catch (Exception ex)
        {
            // On renvoie l'erreur pour qu'elle s'affiche dans l'interface si l'envoi échoue
            throw new Exception("Erreur d'envoi Email : " + ex.Message);
        }
    }
}