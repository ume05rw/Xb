using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestXb;

namespace TextXb
{
    [TestClass()]
    public class LoggerTests : TestBase
    {
        [TestMethod()]
        public void LogTest()
        {
            var logfileName = $"xbstlogger_{DateTime.Now:yyyyMMdd}.log";
            var dir = System.IO.Directory.GetCurrentDirectory();
            var fullPath = System.IO.Path.Combine(dir, logfileName);
            Xb.File.Util.Delete(logfileName);

            Assert.IsFalse(System.IO.File.Exists(fullPath));

            var message = "hello?";
            Xb.App.Logger.Out(message);

            Assert.IsTrue(System.IO.File.Exists(fullPath));

            var logText = Xb.File.Util.GetText(fullPath);
            Assert.IsTrue(logText.IndexOf(message) >= 0);

            Xb.App.Logger.Out("{0} vs {1}", "IronMan", "BatMan");


            try
            {
                throw new Exception("hello!!!");
            }
            catch (Exception ex)
            {
                Xb.App.Logger.Out(ex);
            }

            var query = new Xb.Net.Http("http://gigazine.net/");
            var html = Task.Run(() => query.GetAsync()).GetAwaiter().GetResult();
            Xb.App.Logger.Out(html);
            Xb.App.Logger.Out(html);
            Xb.App.Logger.Out(html);
            Xb.App.Logger.Out(html);
            Xb.App.Logger.Out(html);
            Xb.App.Logger.Out(html);
            Xb.App.Logger.Out(html);
        }

        [TestMethod()]
        public void InstaceTest()
        {
            var logfileName = "logfile.txt";
            var dir = System.IO.Directory.GetCurrentDirectory();
            var fullPath = System.IO.Path.Combine(dir, logfileName);
            Xb.File.Util.Delete(logfileName);

            Assert.IsFalse(System.IO.File.Exists(fullPath));

            var message = "hello?";
            var logger = new Xb.App.Logger(logfileName, dir);

            Assert.IsTrue(System.IO.File.Exists(fullPath));

            logger.Write(message);
            logger.Write(message);
            logger.Write(message);
            logger.Write(message);
            logger.Write(message);
            logger.Write(message);

            logger.Write("{0}:{1}", "aiueo", 12345);

            try
            {
                throw new Exception("hello!!!");
            }
            catch (Exception ex)
            {
                logger.Write(ex);
            }

            logger.Dispose();

            var logText = Xb.File.Util.GetText(fullPath);
            Assert.IsTrue(logText.IndexOf(message) >= 0);



            logger = new Xb.App.Logger(logfileName, dir);

            var query = new Xb.Net.Http("http://gigazine.net/");
            var html = Task.Run(() => query.GetAsync()).GetAwaiter().GetResult();

            logger.Write(html);
            logger.Write(html);
            logger.Write(html);
            logger.Write(html);
            logger.Write(html);
            logger.Write(html);
            logger.Write(html);
            logger.Write(html);
            logger.Write(html);
            logger.Write(html);
            logger.Write(html);


            ////複数スレッドがおのおの勝手に書き込もうとするときの挙動
            //logger.WriteAsync(html);
            //logger.WriteAsync(html);
            //logger.WriteAsync(html);
            //logger.WriteAsync(html);
            //logger.WriteAsync(html);
            //logger.WriteAsync(html);
            //logger.WriteAsync(html);
            //logger.WriteAsync(html);

            logger.Dispose();
        }

    }
}
