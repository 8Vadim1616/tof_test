using System.ComponentModel;
using System.Net;
using System.Net.Mail;
using System.Text;
using UnityEditor.Build.Content;
using UnityEngine;
using BuildSettings = Assets.Scripts.BuildSettings.BuildSettings;

public class MailUtils
{
    public static string[] MAILS => new[]
    {
        "artemtukanow@gmail.com",
        "kalinairis800@gmail.com",
        "ws.dark666@gmail.com",
        "tatianaorlova0271@gmail.com",
        "volmemarus@gmail.com",
        "_fetus_@mail.ru",
    };
    
    public static string[] DEV_MAILS => new[]
    {
        "artemtukanow@gmail.com",
        "KolesovD98@gmail.com",
    };
    
    public static void SendMail(string title, string body, string[] toMail)
    {
        if (!BuildSettings.SendMailOnBuildSuccess)
        {
            BuildSettings.SendMailOnBuildSuccess = true;
            return;
        }
        
        SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
        client.Credentials = new NetworkCredential("playme8builds.sender@gmail.com",  "M6BzXTkt");
        client.EnableSsl = true;
        
        MailAddress from = new MailAddress("playme8builds.sender@gmail.com", "PlayMe8Sender", Encoding.UTF8);
        MailMessage message = new MailMessage();
        message.From = from;
        foreach (var to in toMail)
            message.To.Add(to);

        message.Subject = title;
        message.Body = body;
        message.BodyEncoding = Encoding.UTF8;
        message.SubjectEncoding = Encoding.UTF8;
        
        client.SendCompleted += SendCompletedCallback;
        client.SendAsync(message, "test message1");
        
        void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            string token = (string)e.UserState;

            if (e.Cancelled)
            {
                Debug.Log("Send canceled "+ token);
            }
            if (e.Error != null)
            {
                Debug.Log("[ "+token+" ] " + " " + e.Error.ToString());
            }
            else
            {
                Debug.Log("Message sent.");
            }
        }
    }

}
