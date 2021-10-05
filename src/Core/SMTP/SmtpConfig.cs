using System;
using System.Net;
using System.Net.Mail;

namespace Gk.Core.SMTP
{
    public class SmtpConfig
    {
        public SmtpConfig()
        {
        }

        public SmtpConfig(string host, string port, string username, string password, string enableSsl)
        {
            Host = host;
            Port = port;
            Username = username;
            Password = password;
            EnableSsl = enableSsl;
        }

        public string Host { get; set; }
        public string Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string EnableSsl { get; set; }
        public string FromEmailAddress { get; set; }
        public string ToEmailAddress { get; set; }

        public SmtpClient CreateClient()
        {
            var client = new SmtpClient(Host, Convert.ToInt32(Port))
            {
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = Convert.ToBoolean(EnableSsl)
            };
            if (string.IsNullOrEmpty(Username))
            {
                client.UseDefaultCredentials = true;
            }
            else
            {
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(Username, Password);
            }
            return client;
        }
    }
}