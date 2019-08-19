using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;

namespace Quartz.MARC
{
    class MarcusEmail
    {
        

    public void email_send(string bodytext)
    {
        string filePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        filePath += @"\intruderpic.jpg";

            MailMessage mail = new MailMessage();
    SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
    mail.From = new MailAddress("shimarumaka@gmail.com");
    mail.To.Add("x7darkzero@gmail.com");
    mail.Subject = "OSPJ Quartz - Unauthorized login into your account";
    mail.Body = bodytext + "Attached is the image of the intruder captured from the webcam";

    System.Net.Mail.Attachment attachment;
    attachment = new System.Net.Mail.Attachment(filePath);
    mail.Attachments.Add(attachment);

    SmtpServer.Port = 587;
    SmtpServer.Credentials = new System.Net.NetworkCredential("shimarumaka@gmail.com", "l1pm3hb4nk$");
    SmtpServer.EnableSsl = true;

    SmtpServer.Send(mail);

    }
}
}
