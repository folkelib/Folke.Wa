using System;
using Folke.Wa.Routing;
using Moq;
using Xunit;

namespace Folke.Wa.Test.Routing
{
    public class TestBareApiRoute
    {
        private readonly Type testControllerType;
        private readonly Mock<IWaConfig> configMock;

        private class TestController
        {
            public void Text(string text)
            {

            }
        }

        public TestBareApiRoute()
        {
            configMock = new Mock<IWaConfig>();
            testControllerType = typeof(TestController);
        }
        
        [Fact]
        public void TestTextMethodMatchSuccess()
        {
            var textMethod = testControllerType.GetMethod("Text");
            var route = new BareApiRoute("test/{text}", textMethod, configMock.Object);

            Assert.True(route.Match(new[] { "test", "toto" }), "route match");
        }

        [Fact]
        public void TestTextMethodMatchFail()
        {
            var textMethod = testControllerType.GetMethod("Text");
            var route = new BareApiRoute("test/{text}/az", textMethod, configMock.Object);

            Assert.False(route.Match(new[] { "test", "toto" }), "route match");
        }
    }
}
