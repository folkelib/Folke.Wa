using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Folke.Wa.Test
{
    [TestFixture]
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

        [Test]
        public void TestIsValidTrue()
        {
            var test = new TestClass { Required = "Required", Email = "a@a.com", Nickname = "azrt", Password = "pass", PasswordConfirm = "pass" };
            var modelState = new ModelState(test);
            Assert.IsTrue(modelState.IsValid);
        }

        [Test]
        public void TestIsValidFalse()
        {
            var test = new TestClass { Required = null, Email = "aa.com", Nickname = "aa", Password = "pass", PasswordConfirm = "passs" };
            var modelState = new ModelState(test);
            Assert.IsFalse(modelState.IsValid);
            Assert.AreEqual(modelState.Messages["Required"].Count, 1);
            Assert.AreEqual(modelState.Messages["Email"].Count, 1);
            Assert.AreEqual(modelState.Messages["Nickname"].Count, 1);
            Assert.AreEqual(modelState.Messages["PasswordConfirm"].Count, 1);
        }
    }
}
