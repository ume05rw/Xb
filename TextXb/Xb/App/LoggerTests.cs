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
            var logfileName = "logfile.txt";
            var dir = System.IO.Directory.GetCurrentDirectory();
            var fullPath = System.IO.Path.Combine(dir, logfileName);
            Xb.File.Util.Delete(logfileName);

            Assert.IsFalse(System.IO.File.Exists(fullPath));

            var message = "hello?";
            Xb.App.Logger.Log(message, logfileName, dir);

            Assert.IsTrue(System.IO.File.Exists(fullPath));

            var logText = Xb.File.Util.GetText(fullPath);
            Assert.IsTrue(logText.IndexOf(message) >= 0);
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

            logger.Dispose();

            var logText = Xb.File.Util.GetText(fullPath);
            Assert.IsTrue(logText.IndexOf(message) >= 0);
        }

    }
}
