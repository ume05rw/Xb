using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TextXb
{
    [TestClass()]
    public class MailTests
    {
        //var client = new Xb.Net.MailClient("everybody@nobody.com");
        //client.AuthId = "XXXX@XXXX.com";
        //client.AuthPassword = "********";
        //await client.SendAsync("マルチバイト", "hello!\r\nマルチバイト\r\nhow?\r改行コード\nいろいろ", "anyone@nobody.com", "マルチバイト");

        [TestMethod()]
        public async Task  GmailSendTest1()
        {
            var client = new Xb.Net.MailClient("everybody@nobody.com");
            client.Server = "smtp.gmail.com";
            client.Port = 465;          //
            client.IsUseSsl = true;     //465接続時はSSL使用
            client.IsAuth = true;       //

            client.AuthId = "XXXX@XXXX.com";
            client.AuthPassword = "********";
            client.FromName = "oresamada-";

            await client.SendAsync("マルチバイト", "hello!\r\nマルチバイト\r\nhow?\r改行コード\nいろいろ", "anyone@nobody.com", "マルチバイト");
        }

        [TestMethod()]
        public async Task GmailSendTest2()
        {
            var client = new Xb.Net.MailClient("everybody@nobody.com");
            client.Server = "smtp.gmail.com";
            client.Port = 587;          //
            client.IsUseSsl = false;    //
            client.IsAuth = true;       //

            client.AuthId = "XXXX@XXXX.com";
            client.AuthPassword = "********";
            client.FromName = "oresamada-";

            await client.SendAsync("マルチバイト", "hello!\r\nマルチバイト\r\nhow?\r改行コード\nいろいろ", "anyone@nobody.com", "マルチバイト");
        }

        [TestMethod()]
        public async Task HotmailMailSendTest()
        {
            //Outlook.comの送信はめちゃくちゃ遅い

            var client = new Xb.Net.MailClient("everybody@nobody.com");
            client.Server = "smtp-mail.outlook.com";
            client.Port = 587;          //
            client.IsUseSsl = false;    //
            client.IsAuth = true;       //

            client.AuthId = "XXXX@XXXX.com";
            client.AuthPassword = "********";
            client.FromName = "oresamada-";

            await client.SendAsync("マルチバイト", "hello!\r\nマルチバイト\r\nhow?\r改行コード\nいろいろ", "anyone@nobody.com", "マルチバイト");
        }

        [TestMethod()]
        public async Task MuumuuMailSendTest()
        {
            var client = new Xb.Net.MailClient("everybody@nobody.com");
            client.Server = "smtp.muumuu-mail.com";
            client.Port = 465;          //
            client.IsUseSsl = true;     //465接続時はSSL使用
            client.IsAuth = true;       //ムームーメールは認証時エラーが出た。[but you already said HELO ...]

            client.AuthId = "XXXX@XXXX.com";
            client.AuthPassword = "********";
            client.FromName = "umemoto";

            await client.SendAsync("マルチバイト", "hello!\r\nマルチバイト\r\nhow?\r改行コード\nいろいろ", "anyone@nobody.com", "マルチバイト");
        }

        [TestMethod()]
        public async Task ElseMailSendTest()
        {
            var client = new Xb.Net.MailClient("everybody@nobody.com");
            client.Server = "mw2p0hhgkv.bizmw.com";
            client.Port = 587;          //
            client.IsUseSsl = false;    //
            client.IsAuth = true;       //※ここのサーバはどちらでもよかった。

            client.AuthId = "XXXX@XXXX.com";
            client.AuthPassword = "********";
            client.FromName = "oresamada-";

            await client.SendAsync("マルチバイト", "hello!\r\nマルチバイト\r\nhow?\r改行コード\nいろいろ", "anyone@nobody.com", "マルチバイト");
        }
    }
}
