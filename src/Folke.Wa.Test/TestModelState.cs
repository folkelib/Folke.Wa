using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Folke.Wa.Test
{
    public class TestModelState
    {
        private class TestClass
        {
            [Required]
            public string Required { get; set;}

            [EmailAddress]
            public string Email { get; set; }
        
            [MinLength(4)]
            public string Nickname { get; set; }
            
            public string Password { get; set; }
            [Compare("Password")]
            public string PasswordConfirm { get; set; }
        }

        [Fact]
        public void TestIsValidTrue()
        {
            var test = new TestClass { Required = "Required", Email = "a@a.com", Nickname = "azrt", Password = "pass", PasswordConfirm = "pass" };
            var modelState = new ModelState(test);
            Assert.True(modelState.IsValid);
        }

        [Fact]
        public void TestIsValidFalse()
        {
            var test = new TestClass { Required = null, Email = "aa.com", Nickname = "aa", Password = "pass", PasswordConfirm = "passs" };
            var modelState = new ModelState(test);
            Assert.False(modelState.IsValid);
            Assert.Equal(modelState.Messages["Required"].Count, 1);
            Assert.Equal(modelState.Messages["Email"].Count, 1);
            Assert.Equal(modelState.Messages["Nickname"].Count, 1);
            Assert.Equal(modelState.Messages["PasswordConfirm"].Count, 1);
        }
    }
}
