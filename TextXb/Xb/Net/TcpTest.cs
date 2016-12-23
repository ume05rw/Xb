using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;

namespace TestsXb
{
    [TestClass()]
    public class TcpTests
    {
        [TestMethod()]
        public void OnePointOneTest()
        {
            //TODO: 非同期時イベントがハンドルされなかった場合のテストは、どう書く？
            //TODO: タイムアウトのテストはどうする？

            var server = new Xb.Net.Tcp(1026);
            server.Accepted += (sender, ev) =>
            {
                Assert.IsTrue(ev.EndPoint.ToString().IndexOf("127.0.0.1") >= 0);
            };
            server.Recieved += (sender, ev) =>
            {
                Assert.IsTrue(ev.EndPoint.ToString().IndexOf("127.0.0.1") >= 0);
                Assert.AreEqual("hello!", Encoding.UTF8.GetString(ev.Bytes));
            };
            server.Sended += (sender, ev) =>
            {
                Assert.IsTrue(ev.EndPoint.ToString().IndexOf("127.0.0.1") >= 0);
            };
            server.Disconnected += (sender, ev) =>
            {
                Assert.IsTrue(ev.EndPoint.ToString().IndexOf("127.0.0.1") >= 0);
            };


            var client = new Xb.Net.Tcp("127.0.0.1", 1026);
            client.Connected += (sender, ev) =>
            {
                Assert.IsTrue(ev.EndPoint.ToString().IndexOf("127.0.0.1") >= 0);
            };
            client.Recieved += (sender, ev) =>
            {
                Assert.IsTrue(ev.EndPoint.ToString().IndexOf("127.0.0.1") >= 0);
                Assert.AreEqual("日本語ΠΩЙ", Encoding.UTF8.GetString(ev.Bytes));
            };
            client.Sended += (sender, ev) =>
            {
                Assert.IsTrue(ev.EndPoint.ToString().IndexOf("127.0.0.1") >= 0);
            };
            client.Disconnected += (sender, ev) =>
            {
                Assert.IsTrue(ev.EndPoint.ToString().IndexOf("127.0.0.1") >= 0);
            };

            client.Send("hello!");
            server.Send("日本語ΠΩЙ");
        }
    }
}
