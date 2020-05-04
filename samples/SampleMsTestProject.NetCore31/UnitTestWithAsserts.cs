using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SampleMsTestProject.NetCore31
{
    [TestClass]
    public class UnitTestWithAsserts
    {
        [TestMethod]
        public void BooleanAssert()
        {
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void IntegerAssert()
        {
            var expected = 1;
            Assert.AreEqual(expected, 1);
        }
    }
}
