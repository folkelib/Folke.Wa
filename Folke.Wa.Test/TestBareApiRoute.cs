using System;
using Moq;
using NUnit.Framework;

namespace Folke.Wa.Test
{
    [TestFixture]
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

        [SetUp]
        public void Initialize()
        {
            configMock = new Mock<IWaConfig>();
            testControllerType = typeof(TestController);
        }

        [TearDown]
        public void Cleanup()
        {
        }

        [Test]
        public void TestTextMethodMatchSuccess()
        {
            var textMethod = testControllerType.GetMethod("Text");
            var route = new BareApiRoute("test/{text}", textMethod, configMock.Object);

            Assert.IsTrue(route.Match(new[] { "test", "toto" }), "route match");
        }

        [Test]
        public void TestTextMethodMatchFail()
        {
            var textMethod = testControllerType.GetMethod("Text");
            var route = new BareApiRoute("test/{text}/az", textMethod, configMock.Object);

            Assert.IsFalse(route.Match(new[] { "test", "toto" }), "route match");
        }
    }
}
