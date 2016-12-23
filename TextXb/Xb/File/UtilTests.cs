using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextXb
{
    [TestClass()]
    public class UtilTests
    {
        [TestMethod()]
        public void GetBytesTest()
        {
            Xb.File.Util.WriteText("testing.txt", "hello");
            var bytes = Xb.File.Util.GetBytes("testing.txt");
            Assert.AreEqual("hello", Encoding.UTF8.GetString(bytes));

            Xb.File.Util.WriteText("testing.txt", "マルチバイトЙΩ℃");
            bytes = Xb.File.Util.GetBytes("testing.txt");
            Assert.AreEqual("マルチバイトЙΩ℃", Encoding.UTF8.GetString(bytes));

            Xb.File.Util.WriteText("testing.txt", "マルチバイトЙΩ℃", Encoding.GetEncoding("Shift_JIS"));
            bytes = Xb.File.Util.GetBytes("testing.txt");
            Assert.AreEqual("マルチバイトЙΩ℃", Encoding.GetEncoding("Shift_JIS").GetString(bytes));

        }

        [TestMethod()]
        public void GetTextTest()
        {
            Xb.File.Util.WriteText("testing.txt", "hello");
            var text = Xb.File.Util.GetText("testing.txt");
            Assert.AreEqual("hello", text);

            Xb.File.Util.WriteText("testing.txt", "マルチバイトЙΩ℃");
            text = Xb.File.Util.GetText("testing.txt");
            Assert.AreEqual("マルチバイトЙΩ℃", text);

            Xb.File.Util.WriteText("testing.txt", "マルチバイトЙΩ℃", Encoding.GetEncoding("Shift_JIS"));
            text = Xb.File.Util.GetText("testing.txt", Encoding.GetEncoding("Shift_JIS"));
            Assert.AreEqual("マルチバイトЙΩ℃", text);

            Xb.File.Util.WriteText("testing.txt", "マルチバイトЙΩ℃", Encoding.GetEncoding("Shift_JIS"));
            text = Xb.File.Util.GetText("testing.txt");
            Assert.AreEqual("マルチバイトЙΩ℃", text);
        }
    }
}
