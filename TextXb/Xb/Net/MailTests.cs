using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MimeKit;
using TestXb;

namespace TextXb
{
    [TestClass()]
    public class MailTests : TestBase
    {
        private Xb.Net.MailSender _client;
        private string _sendTo;

        public MailTests()
        {
            this._client = new Xb.Net.MailSender("everybody@nobody.com");
            this._client.AuthId = "XXXX@XXXX.com";
            this._client.AuthPassword = "********";
            this._sendTo = "anyone@nobody.com";
        }



        [TestMethod()]
        public async Task  GmailSendTest1()
        {
            this._client.Server = "smtp.gmail.com";
            this._client.Port = 465;          //
            this._client.IsUseSsl = true;     //465接続時はSSL使用
            this._client.IsAuth = true;       //
            this._client.FromName = "nobody_knows";

            await this._client.SendAsync("hello!\r\nマルチバイト\r\nhow?\r改行コード\nいろいろ"
                                 , "マルチバイト"
                                 , this._sendTo
                                 , "マルチバイト");
        }

        [TestMethod()]
        public async Task GmailSendTest2()
        {
            this._client.Server = "smtp.gmail.com";
            this._client.Port = 587;          //
            this._client.IsUseSsl = false;    //
            this._client.IsAuth = true;       //
            this._client.FromName = "nobody_knows";

            await this._client.SendAsync("hello!\r\nマルチバイト\r\nhow?\r改行コード\nいろいろ"
                                 , "マルチバイト"
                                 , this._sendTo
                                 , "マルチバイト");
        }


        [TestMethod()]
        public async Task MultiSendTest1()
        {
            this._client.Server = "smtp.muumuu-mail.com";
            this._client.Port = 465;          //
            this._client.IsUseSsl = true;     //465接続時はSSL使用
            this._client.IsAuth = true;       //ムームーメールは認証時エラーが出た。[but you already said HELO ...]
            this._client.FromName = "nobody_knows";
            this._client.EhloAfterAuth = false;

            var msg1 = this._client.GetMessage();
            msg1.To.Add(new MailboxAddress("XXXX@XXXX.com"));
            msg1.Subject = "Hello!";
            msg1.Body = new TextPart("plain")
            {
                Text = "how?\r\n are?\r\n you?"
            };

            var msg2 = this._client.GetMessage();
            msg2.To.Add(new MailboxAddress("XXXX@XXXX.com"));
            msg2.Subject = "Hello!";
            msg2.Body = new TextPart("plain")
            {
                Text = "how?\r\n are?\r\n you?"
            };

            var msg3 = this._client.GetMessage();
            msg3.To.Add(new MailboxAddress("XXXX@XXXX.com"));
            msg3.Subject = "Hello!";
            msg3.Body = new TextPart("plain")
            {
                Text = "how?\r\n are?\r\n you?"
            };

            await this._client.SendMultiAsync(new MimeMessage[] {msg1, msg2, msg3});
        }


        [TestMethod()]
        public async Task HotmailMailSendTest()
        {
            //Outlook.comの送信はめちゃくちゃ遅い
            this._client.Server = "smtp-mail.outlook.com";
            this._client.Port = 587;          //
            this._client.IsUseSsl = false;    //
            this._client.IsAuth = true;       //
            this._client.FromName = "nobody_knows";

            await this._client.SendAsync("hello!\r\nマルチバイト\r\nhow?\r改行コード\nいろいろ"
                                 , "マルチバイト"
                                 , this._sendTo
                                 , "マルチバイト");
        }

        [TestMethod()]
        public async Task MuumuuMailSendTest()
        {
            this._client.Server = "smtp.muumuu-mail.com";
            this._client.Port = 465;          //
            this._client.IsUseSsl = true;     //465接続時はSSL使用
            this._client.IsAuth = true;       //ムームーメールは認証時エラーが出た。[but you already said HELO ...]
            this._client.FromName = "nobody_knows";
            this._client.EhloAfterAuth = false;


            await this._client.SendAsync("hello!\r\nマルチバイト\r\nhow?\r改行コード\nいろいろ"
                                 , "マルチバイト"
                                 , this._sendTo
                                 , "マルチバイト");
        }

        [TestMethod()]
        public async Task ElseMailSendTest()
        {
            this._client.Server = "mw2p0hhgkv.bizmw.com";
            this._client.Port = 587;          //
            this._client.IsUseSsl = false;    //
            this._client.IsAuth = true;       //※ここのサーバはどちらでもよかった。
            this._client.FromName = "nobody_knows";

            await this._client.SendAsync("hello!\r\nマルチバイト\r\nhow?\r改行コード\nいろいろ"
                                 , "マルチバイト"
                                 , this._sendTo
                                 , "マルチバイト");
        }


        public override void Dispose()
        {
            this._client.Dispose();
            base.Dispose();
        }
    }
}
