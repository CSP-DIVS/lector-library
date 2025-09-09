
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;
using Xunit;

namespace Csp.E2E.Tests
{
    public class Story04_MyProfileTests : IClassFixture<DriverFixture>
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public Story04_MyProfileTests(DriverFixture fixture)
        {
            _driver = fixture.Driver;
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        }

        private void LoginAsMember()
        {
            _driver.Navigate().GoToUrl(Config.BaseUrl);
            _driver.FindElement(By.Id("username")).SendKeys("member@example.com");
            _driver.FindElement(By.Id("password")).SendKeys("member123!");
            _driver.FindElement(By.CssSelector("button.submit-btn")).Click();
            _wait.Until(d => d.FindElements(By.XPath("//h3[text()='My Issued Books']")).Any());
        }

        [Fact]
        public void Member_Can_Update_Profile_Information()
        {
            // Arrange
            LoginAsMember();

            // Act
            _driver.FindElement(By.LinkText("My Profile")).Click();
            _wait.Until(d => d.FindElement(By.Id("phone")));
            var phoneInput = _driver.FindElement(By.Id("phone"));
            phoneInput.Clear();
            var newPhone = "123-456-7890";
            phoneInput.SendKeys(newPhone);
            _driver.FindElement(By.CssSelector("button.primary-btn")).Click();

            // Assert
            Assert.True(_wait.Until(d => d.FindElement(By.ClassName("alert-success")).Text.Contains("Profile updated successfully")));
            _driver.Navigate().Refresh();
            Assert.Equal(newPhone, _wait.Until(d => d.FindElement(By.Id("phone"))).GetAttribute("value"));
        }

        [Fact]
        public void Member_Can_Change_Password_And_ReLogin()
        {
            // Arrange
            LoginAsMember();
            var newPassword = $"newPass-{Guid.NewGuid()}";

            // Act: Change password
            _driver.FindElement(By.LinkText("My Profile")).Click();
            _wait.Until(d => d.FindElement(By.Id("currentPassword"))).SendKeys("member123!");
            _driver.FindElement(By.Id("newPassword")).SendKeys(newPassword);
            _driver.FindElement(By.Id("confirmPassword")).SendKeys(newPassword);
            _driver.FindElement(By.CssSelector("button.secondary-btn")).Click();
            Assert.True(_wait.Until(d => d.FindElement(By.ClassName("alert-success")).Text.Contains("Password updated successfully")));

            // Act: Logout and re-login
            _driver.FindElement(By.CssSelector(".user-menu button")).Click();
            _driver.FindElement(By.LinkText("Logout")).Click();
            _wait.Until(d => d.FindElement(By.Id("username"))).SendKeys("member@example.com");
            _driver.FindElement(By.Id("password")).SendKeys(newPassword);
            _driver.FindElement(By.CssSelector("button.submit-btn")).Click();

            // Assert
            Assert.True(_wait.Until(d => d.FindElements(By.XPath("//h3[text()='My Issued Books']")).Any()));
        }

        [Fact]
        public void Update_Profile_With_Invalid_Email_Fails()
        {
            // Arrange
            LoginAsMember();

            // Act
            _driver.FindElement(By.LinkText("My Profile")).Click();
            var emailInput = _wait.Until(d => d.FindElement(By.Id("email")));
            emailInput.Clear();
            emailInput.SendKeys("not-an-email");
            _driver.FindElement(By.CssSelector("button.primary-btn")).Click();

            // Assert
            // We expect the form submission to be blocked by client-side validation,
            // so no success message should appear.
            Assert.Throws<WebDriverTimeoutException>(() => _wait.Until(d => d.FindElements(By.ClassName("alert-success")).Any()));
            Assert.Equal("not-an-email", emailInput.GetAttribute("value")); // Value remains
        }

        [Fact]
        public void Change_Password_With_Incorrect_Current_Password_Fails()
        {
             // Arrange
            LoginAsMember();

            // Act
            _driver.FindElement(By.LinkText("My Profile")).Click();
            _wait.Until(d => d.FindElement(By.Id("currentPassword"))).SendKeys("wrong-current-password");
            _driver.FindElement(By.Id("newPassword")).SendKeys("any-new-password");
            _driver.FindElement(By.Id("confirmPassword")).SendKeys("any-new-password");
            _driver.FindElement(By.CssSelector("button.secondary-btn")).Click();

            // Assert
            Assert.True(_wait.Until(d => d.FindElement(By.ClassName("alert-error")).Text.Contains("Incorrect current password")));
        }
    }
}
