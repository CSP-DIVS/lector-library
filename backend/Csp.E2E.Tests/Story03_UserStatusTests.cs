
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;
using Xunit;

namespace Csp.E2E.Tests
{
    public class Story03_UserStatusTests : IClassFixture<DriverFixture>
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public Story03_UserStatusTests(DriverFixture fixture)
        {
            _driver = fixture.Driver;
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        }

        private void LoginAsAdmin()
        {
            _driver.Navigate().GoToUrl(Config.BaseUrl);
            _driver.FindElement(By.Id("username")).SendKeys("admin");
            _driver.FindElement(By.Id("password")).SendKeys("admin123!");
            _driver.FindElement(By.CssSelector("button.submit-btn")).Click();
            _wait.Until(d => d.FindElements(By.XPath("//h3[text()='Admin Actions']")).Any());
        }

        [Fact]
        public void Admin_Can_Deactivate_And_Reactivate_User()
        {
            // --- Deactivate --- //
            // Arrange
            LoginAsAdmin();
            _driver.FindElement(By.LinkText("Member Management")).Click();
            _wait.Until(d => d.FindElements(By.CssSelector("tbody tr")).Any());
            var memberRow = _driver.FindElement(By.XPath("//td[text()='member@example.com']/.."));

            // Act
            memberRow.FindElement(By.CssSelector("input[type='checkbox']")).Click();

            // Assert
            Assert.True(_wait.Until(d => d.FindElement(By.ClassName("alert-success")).Text.Contains("status updated")));
            _driver.FindElement(By.CssSelector(".user-menu button")).Click(); // Logout
            _driver.FindElement(By.LinkText("Logout")).Click();

            // --- Verify Deactivated User Cannot Login --- //
            // Arrange
            _wait.Until(d => d.FindElement(By.Id("username")));

            // Act
            _driver.FindElement(By.Id("username")).SendKeys("member@example.com");
            _driver.FindElement(By.Id("password")).SendKeys("member123!");
            _driver.FindElement(By.CssSelector("button.submit-btn")).Click();

            // Assert
            Assert.True(_wait.Until(d => d.FindElement(By.ClassName("error-message")).Text.Contains("account is inactive")));

            // --- Reactivate --- //
            // Arrange
            LoginAsAdmin();
             _driver.FindElement(By.LinkText("Member Management")).Click();
            _wait.Until(d => d.FindElements(By.CssSelector("tbody tr")).Any());
            memberRow = _driver.FindElement(By.XPath("//td[text()='member@example.com']/.."));

            // Act
            memberRow.FindElement(By.CssSelector("input[type='checkbox']")).Click();

            // Assert
            Assert.True(_wait.Until(d => d.FindElement(By.ClassName("alert-success")).Text.Contains("status updated")));
        }

        [Fact]
        public void Admin_Cannot_Deactivate_Own_Account()
        {
            // Arrange
            LoginAsAdmin();

            // Act
            _driver.FindElement(By.LinkText("Member Management")).Click();
            _wait.Until(d => d.FindElements(By.CssSelector("tbody tr")).Any());
            var adminRow = _driver.FindElement(By.XPath("//td[text()='admin@example.com']/.."));
            var toggle = adminRow.FindElement(By.CssSelector("input[type='checkbox']"));

            // Assert
            Assert.False(toggle.Enabled);
        }
    }
}
