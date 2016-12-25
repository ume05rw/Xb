using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;

namespace Xb.Net
{
    public class MailClient
    {
        /// <summary>
        /// Smtp server
        /// SMTPサーバ
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// Smtp port
        /// 接続先ポート
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Do auth before sending
        /// 送信前に認証を行うか否か
        /// </summary>
        public bool IsAuth { get; set; }

        /// <summary>
        /// Id for authentication smtp
        /// 認証用ID
        /// </summary>
        public string AuthId { get; set; }

        /// <summary>
        /// Password for authentication smtp
        /// 認証用パスワード
        /// </summary>
        public string AuthPassword { get; set; }

        /// <summary>
        /// Use smtp on ssl
        /// SSL使用可否
        /// </summary>
        public bool IsUseSsl { get; set; }

        /// <summary>
        /// Text encoding
        /// エンコード
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Sender's mail address
        /// 送信元アドレス
        /// </summary>
        public string FromAddress { get; set; }

        /// <summary>
        /// Sender's name
        /// 送信元名
        /// </summary>
        public string FromName { get; set; }


        /// <summary>
        /// Constructor
        /// コンストラクタ
        /// </summary>
        public MailClient(string fromAddress)
        {
            this.Server = "localhost";
            this.Port = 25;
            this.IsAuth = false;
            this.AuthId = "";
            this.AuthPassword = "";
            this.Encoding = Encoding.UTF8;
            this.IsUseSsl = false;

            this.FromAddress = fromAddress;
            this.FromName = fromAddress;
        }


        /// <summary>
        /// Send text-only mail
        /// テキストのみのメールを送信する
        /// </summary>
        /// <param name="bodyText"></param>
        /// <param name="subject"></param>
        /// <param name="toAddress"></param>
        /// <param name="toName"></param>
        /// <returns></returns>
        public async Task SendAsync(string bodyText
                                  , string subject
                                  , string toAddress
                                  , string toName = null)
        {
            if (string.IsNullOrEmpty(bodyText))
                throw new ArgumentNullException(nameof(bodyText), "Xb.Net.Mail.Send: bodyText null");

            if (string.IsNullOrEmpty(subject))
                throw new ArgumentNullException(nameof(subject), "Xb.Net.Mail.Send: subject null");

            if (string.IsNullOrEmpty(toAddress))
                throw new ArgumentNullException(nameof(toAddress), "Xb.Net.Mail.Send: toAddress null");

            var message = new MimeMessage();
            message.From.Add(string.IsNullOrEmpty(this.FromName)
                                ? new MailboxAddress(this.FromAddress)
                                : new MailboxAddress(this.Encoding
                                                   , this.FromName
                                                   , this.FromAddress));

            message.To.Add(string.IsNullOrEmpty(toName)
                                ? new MailboxAddress(toAddress)
                                : new MailboxAddress(this.Encoding
                                                   , toName
                                                   , toAddress));

            message.Subject = subject;
            message.Body = new TextPart("plain")
            {
                Text = bodyText
            };

            await this.SendAsync(message);

            message = null;
        }


        /// <summary>
        /// Send mail by MimeMessage
        /// MimeMessageオブジェクトでメールを送信する
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendAsync(MimeMessage message)
        {
            await Task.Run(() =>
            {
                using (var client = new SmtpClient())
                {
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    client.Connect(this.Server, this.Port, this.IsUseSsl);

                    client.AuthenticationMechanisms.Remove("XOAUTH2");

                    if (this.IsAuth)
                        client.Authenticate(this.AuthId, this.AuthPassword);

                    client.Send(message);
                    client.Disconnect(true);
                }
            });
        }


        /// <summary>
        /// Get MimeMessage-object for send
        /// 送信メッセージオブジェクトを取得する。
        /// </summary>
        /// <returns></returns>
        public MimeMessage GetMessage()
        {
            var message = new MimeMessage();
            message.From.Add(string.IsNullOrEmpty(this.FromName)
                                ? new MailboxAddress(this.FromAddress)
                                : new MailboxAddress(this.Encoding
                                                   , this.FromName
                                                   , this.FromAddress));
            return message;
        }
    }
}
