using System;

namespace Xb.Net
{

    /// <summary>
    /// メール送信処理クラス
    /// </summary>
    /// <remarks>
    /// メールの受信処理はユーザーのメーラーに任せたい。
    /// 本クラスはメール送信処理に特化する。
    /// 
    /// 
    /// SmtpClientクラスを使ってメールを送信する
    /// http://dobon.net/vb/dotnet/internet/smtpclient.html
    /// 
    /// SSL/TLSを使用してSMTPでメールを送信する
    /// http://dobon.net/vb/dotnet/internet/smtpssltls.html
    /// 
    /// SMTP認証でメールを送信する
    /// http://dobon.net/vb/dotnet/internet/smtpauth.html
    /// 
    /// CC、BCC、添付ファイル、優先順位などを指定してメールを送信する
    /// http://dobon.net/vb/dotnet/internet/smtpmail2.html
    /// </remarks>
    public class Mail : IDisposable
    {
        /// <summary>
        /// 指定された文字列がメールアドレスとして正しい形式か検証する
        /// </summary>
        /// <param name="address">検証する文字列</param>
        /// <returns>正しい時はTrue。正しくない時はFalse。</returns>
        /// <remarks>
        /// まるっと以下のコピペ。
        /// メールアドレスが正式な規則にあっているか検証する
        /// http://dobon.net/vb/dotnet/internet/validatemailaddress.html
        /// </remarks>
        public static bool IsValidAddress(string address)
        {

            if (string.IsNullOrEmpty(address))
            {
                return false;
            }

            try
            {
                System.Net.Mail.MailAddress tmp = new System.Net.Mail.MailAddress(address);
            }
            catch (FormatException)
            {
                //FormatExceptionがスローされた時は、正しくない
                return false;
            }

            return true;
        }


        private System.Net.Mail.SmtpClient _smtpClient;
        private string _host;
        private int _port;

        private System.Net.NetworkCredential _credentials;

        /// <summary>
        /// SMTPサーバアドレス
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string Host
        {
            get { return this._host; }
        }


        /// <summary>
        /// SMTPポート
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public int Port
        {
            get { return this._port; }
        }


        /// <summary>
        /// メールテキストのエンコード
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public System.Text.Encoding Encoding { get; set; }


        /// <summary>
        /// 送信元として表示されるメールアドレス
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string FromAddress { get; set; }

        /// <summary>
        /// 送信元アドレスに併記される送信元名称
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string FromName { get; set; }


        /// <summary>
        /// コンストラクタ(認証無し)
        /// </summary>
        /// <param name="fromAddress"></param>
        /// <remarks>
        /// ローカルSMTPサーバを想定。
        /// </remarks>
        public Mail(string fromAddress)
        {
            this._smtpClient = new System.Net.Mail.SmtpClient();
            this._host = "localhost";
            this._port = 25;
            this._credentials = null;
            this.Encoding = System.Text.Encoding.UTF8;
            this.FromAddress = fromAddress;
            this.FromName = "";

            this._smtpClient.Host = this._host;
            this._smtpClient.Port = this._port;
            this._smtpClient.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
        }


        /// <summary>
        /// コンストラクタ(STARTTLS認証)
        /// </summary>
        /// <param name="host"></param>
        /// <param name="authId"></param>
        /// <param name="authPassword"></param>
        /// <param name="fromAddress"></param>
        /// <param name="fromName"></param>
        /// <param name="port"></param>
        /// <remarks>
        /// GMailを想定。
        /// .Netの仕様上、SMTP over SSL（SMTP/SSL、SMTPS）には対応していない。
        /// </remarks>
        public Mail(string authId, 
                    string authPassword, 
                    string host = "localhost", 
                    string fromAddress = null, 
                    string fromName = null, 
                    int port = 587)
        {
            this._smtpClient = new System.Net.Mail.SmtpClient();
            this._host = host;
            this._port = port;
            this._credentials = new System.Net.NetworkCredential(authId, authPassword);
            this.Encoding = System.Text.Encoding.UTF8;
            this.FromAddress = fromAddress != null ? fromAddress : authId;
            this.FromName = string.IsNullOrEmpty(fromName) ? "" : fromName;

            this._smtpClient.Host = this._host;
            this._smtpClient.Port = this._port;
            this._smtpClient.Credentials = this._credentials;
            this._smtpClient.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
            this._smtpClient.EnableSsl = true;
        }


        /// <summary>
        /// SMTPサーバを設定する。
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <remarks></remarks>
        public void SetServer(string host, int port)
        {
            this._host = host;
            this._port = port;
            this._smtpClient.Host = this._host;
            this._smtpClient.Port = this._port;
        }


        /// <summary>
        /// 認証情報を設定する。
        /// </summary>
        /// <param name="authId"></param>
        /// <param name="authPassword"></param>
        /// <param name="useSsl"></param>
        /// <remarks></remarks>
        public void SetAuth(string authId, string authPassword, bool useSsl = true)
        {
            this._credentials = new System.Net.NetworkCredential(authId, authPassword);
            this._smtpClient.Credentials = this._credentials;
            this._smtpClient.EnableSsl = useSsl;
        }


        /// <summary>
        /// メールを送信する。
        /// </summary>
        /// <param name="toAddress"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="toName"></param>
        /// <remarks></remarks>
        public void Send(string toAddress, string subject, string body, string toName = "")
        {
            string tmpFromName = this.FromName;
            if (!string.IsNullOrEmpty(tmpFromName))
            {
                tmpFromName = this.EncodeMailHeader("\"" + tmpFromName + "\"", this.Encoding);
            }

            if (!string.IsNullOrEmpty(toName))
            {
                toName = this.EncodeMailHeader("\"" + toName + "\"", this.Encoding);
            }


            System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();

            //message.SubjectEncoding = Me.Encoding
            message.BodyEncoding = this.Encoding;



            //送信元アドレス
            message.From = this.CreateMailAddress(this.FromAddress, tmpFromName);

            //送信先アドレス
            message.To.Add(this.CreateMailAddress(toAddress, toName));

            //件名
            message.Subject = this.EncodeMailHeader(subject, this.Encoding);

            //本文
            message.Body = body;

            //送信する。
            this._smtpClient.Send(message);

            //メッセージオブジェクトを破棄
            message.Dispose();
        }


        /// <summary>
        /// メッセージヘッダのためのRFC2047形式の文字列に変換する（Base64）
        /// </summary>
        /// <param name="text">変換もとの文字列</param>
        /// <param name="encoding">エンコーディング</param>
        /// <returns></returns>
        /// <remarks>
        /// ヘッダをBase64でエンコードする
        /// http://dobon.net/vb/dotnet/internet/smtpclient.html#section5
        /// </remarks>
        private string EncodeMailHeader(string text, System.Text.Encoding encoding)
        {
            //Base64でエンコードする
            string ret = System.Convert.ToBase64String(encoding.GetBytes(text));
            //RFC2047形式に
            return string.Format("=?{0}?B?{1}?=", encoding.BodyName, ret);
        }

        /// <summary>
        /// MailAddressオブジェクトを作成する
        /// </summary>
        /// <param name="address">メールアドレス。</param>
        /// <param name="displayName">表示名。省略する時は、Nothing。</param>
        /// <returns>MailAddressオブジェクト</returns>
        /// <remarks>
        /// SmtpClientでメールを送信しようとするとFormatExceptionが発生する問題の解決法
        /// http://dobon.net/vb/dotnet/internet/smtpclientformatexception.html
        /// </remarks>
        private System.Net.Mail.MailAddress CreateMailAddress(string address, string displayName = null)
        {

            if (displayName == null)
            {
                displayName = string.Empty;
            }

            //メールアドレスをユーザーとホストに分解する
            //（ユーザー名に@が含まれることは考慮していない）
            string[] ss = address.Split(new char[] { '@' }, 2);

            //MailAddressオブジェクトを作成する
            return (System.Net.Mail.MailAddress)typeof(System.Net.Mail.MailAddress)
                    .InvokeMember(null, System.Reflection.BindingFlags.CreateInstance 
                                        | System.Reflection.BindingFlags.NonPublic 
                                        | System.Reflection.BindingFlags.Instance, 
                                  null, 
                                  null, 
                                  new object[] {
                                      displayName,
                                      ss[0],
                                      ss[1]
                                  });
        }

        #region "IDisposable Support"
        // 重複する呼び出しを検出するには
        private bool disposedValue;

        // IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                }

                if ((this._smtpClient != null))
                {
                    this._smtpClient.Dispose();
                }

                this._smtpClient = null;
                this._host = null;
                this._credentials = null;
            }
            this.disposedValue = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
