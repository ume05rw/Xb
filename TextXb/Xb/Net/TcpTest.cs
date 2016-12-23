using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsXb
{
    [TestClass()]
    public class TcpTests
    {
        [TestMethod()]
        public void ConnectTest()
        {
            var tcp = new Xb.Net.Tcp();
            tcp.Connect("192.168.254.11", 80);
        }
    }
}
