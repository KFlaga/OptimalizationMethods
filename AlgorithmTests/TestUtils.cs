using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgorithmTests
{
    public class TestUtils
    {
        public static void ExpectThrow(Action f)
        {
            try
            {
                f();
                Assert.Fail("Throw was expected");
            }
            catch (Exception) { }
        }

        public static void ExpectThrow<T>(Action f) where T : Exception
        {
            try
            {
                f();
                Assert.Fail("Throw was expected");
            }
            catch (T) { }
        }
    }

}
