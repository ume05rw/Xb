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
            //TODO: タイムアウトのテストはどうする？

            var isServerRecieved = false;
            var isServerSended = false;
            var isServerDisconnected = false;
            var isClientConnected = false;
            var isClientRecieved = false;
            var isClientSended = false;
            var isClientDisconnected = false;

            var server = new Xb.Net.Tcp(1026);
            server.Recieved += (sender, ev) =>
            {
                Assert.IsTrue(ev.EndPoint.ToString().IndexOf("127.0.0.1") >= 0);
                Assert.AreEqual("hello!", Encoding.UTF8.GetString(ev.Bytes));
                isServerRecieved = true;
            };
            server.Sended += (sender, ev) =>
            {
                Assert.IsTrue(ev.EndPoint.ToString().IndexOf("127.0.0.1") >= 0);
                isServerSended = true;
            };
            server.Disconnected += (sender, ev) =>
            {
                Assert.IsTrue(ev.EndPoint.ToString().IndexOf("127.0.0.1") >= 0);
                isServerDisconnected = true;
            };

            var client = new Xb.Net.Tcp("127.0.0.1", 1026);
            client.Connected += (sender, ev) =>
            {
                Assert.IsTrue(ev.EndPoint.ToString().IndexOf("127.0.0.1") >= 0);
                isClientConnected = true;
            };
            client.Recieved += (sender, ev) =>
            {
                Assert.IsTrue(ev.EndPoint.ToString().IndexOf("127.0.0.1") >= 0);
                Assert.AreEqual("日本語ΠΩЙ", Encoding.UTF8.GetString(ev.Bytes));
                isClientRecieved = true;
            };
            client.Sended += (sender, ev) =>
            {
                Assert.IsTrue(ev.EndPoint.ToString().IndexOf("127.0.0.1") >= 0);
                isClientSended = true;
            };
            client.Disconnected += (sender, ev) =>
            {
                Assert.IsTrue(ev.EndPoint.ToString().IndexOf("127.0.0.1") >= 0);
                isClientDisconnected = true;
            };

            var task = Task.Run(() =>
            {
                System.Threading.Thread.Sleep(1000);
            });
            task.Wait();

            Assert.IsTrue(isClientConnected);

            client.Send("hello!");

            task = Task.Run(() =>
            {
                System.Threading.Thread.Sleep(1000);
            });
            task.Wait();

            Assert.IsTrue(isClientSended);
            Assert.IsTrue(isServerRecieved);

            server.Send("日本語ΠΩЙ");

            task = Task.Run(() =>
            {
                System.Threading.Thread.Sleep(1000);
            });
            task.Wait();

            Assert.IsTrue(isServerSended);
            Assert.IsTrue(isClientRecieved);
            
            client.Dispose();

            task = Task.Run(() =>
            {
                System.Threading.Thread.Sleep(1000);
            });
            task.Wait();

            Assert.IsTrue(isServerDisconnected);
        }
    }
}
