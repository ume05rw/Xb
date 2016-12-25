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
    public class MailClient : IDisposable
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

            await this.SendMultiAsync(new MimeMessage[] { message });
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
            await this.SendMultiAsync(new MimeMessage[] { message });
        }


        /// <summary>
        /// Send mail by MimeMessage
        /// MimeMessageオブジェクトでメールを送信する
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        public async Task SendMultiAsync(IEnumerable<MimeMessage> messages)
        {
            await Task.Run(() =>
            {
                using (var client = new SmtpClient())
                {
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    client.Connect(this.Server, this.Port, this.IsUseSsl);

                    client.AuthenticationMechanisms.Remove("XOAUTH2");

                    if (this.IsAuth)
                    {
                        try
                        {
                            //var credential
                            //    = new System.Net.NetworkCredential(this.AuthId
                            //        , this.AuthPassword);
                            //client.Authenticate(credential);

                            client.Authenticate(this.AuthId, this.AuthPassword);
                        }
                        catch (Exception ex)
                        {
                            Xb.Util.Out(ex);

                            //SMTP Error 503 [but you already said HELO]
                            //BadCommandSequence,
                            //throw ex;
                        }
                    }

                    foreach (var message in messages)
                    {
                        try
                        {
                            client.Send(message);
                        }
                        catch (Exception ex)
                        {
                            Xb.Util.Out(ex);
                            throw ex;
                        }
                    }

                    client.Disconnect(true);
                }
            });
        }


        /// <summary>
        /// Get MimeMessage-object for send - see: http://www.mimekit.net/docs/html/CreatingMessages.htm
        /// 送信メッセージオブジェクトを取得する。
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// GitHub - jstedfast/MailKit: A cross-platform .NET library for IMAP, POP3, and SMTP.
        /// https://github.com/jstedfast/MailKit
        /// 
        /// Introduction(MailKit official document)
        /// http://www.mimekit.net/docs/html/Introduction.htm
        /// 
        /// c# - How to send email by using MailKit? - Stack Overflow
        /// http://stackoverflow.com/questions/33496290/how-to-send-email-by-using-mailkit
        /// </remarks>
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

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.Server = null;
                    this.AuthId = null;
                    this.AuthPassword = null;
                    this.Encoding = null;
                    this.FromAddress = null;
                    this.FromName = null;
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
