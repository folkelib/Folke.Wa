using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Folke.Wa.Test
{
    [TestClass]
    public class TestBareApiRoute
    {
        private Type testControllerType;
        private Mock<IWaConfig> configMock;

        private class TestController
        {
            public void Text(string text)
            {

            }
        }

        [TestInitialize]
        public void Initialize()
        {
            configMock = new Mock<IWaConfig>();
            testControllerType = typeof(TestController);
        }

        [TestCleanup]
        public void Cleanup()
        {
        }

        [TestMethod]
        public void TestTextMethodMatchSuccess()
        {
            var textMethod = testControllerType.GetMethod("Text");
            var route = new BareApiRoute("test/{text}", textMethod, configMock.Object);

            Assert.IsTrue(route.Match(new[] { "test", "toto" }), "route match");
        }

        [TestMethod]
        public void TestTextMethodMatchFail()
        {
            var textMethod = testControllerType.GetMethod("Text");
            var route = new BareApiRoute("test/{text}/az", textMethod, configMock.Object);

            Assert.IsFalse(route.Match(new[] { "test", "toto" }), "route match");
        }
    }
}
