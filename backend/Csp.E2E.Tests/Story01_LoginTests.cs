
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;
using Xunit;

namespace Csp.E2E.Tests
{
    public class Story01_LoginTests : IClassFixture<DriverFixture>
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public Story01_LoginTests(DriverFixture fixture)
        {
            _driver = fixture.Driver;
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        }

        private void NavigateToLogin()
        {
            _driver.Navigate().GoToUrl(Config.BaseUrl);
            _wait.Until(d => d.FindElement(By.Id("username")));
        }

        [Fact]
        public void Admin_Can_Login_Successfully()
        {
            // Arrange
            NavigateToLogin();

            // Act
            _driver.FindElement(By.Id("username")).SendKeys("admin");
            _driver.FindElement(By.Id("password")).SendKeys("admin123!");
            _driver.FindElement(By.CssSelector("button.submit-btn")).Click();

            // Assert
            Assert.True(_wait.Until(d => d.FindElements(By.XPath("//h3[text()='Admin Actions']")).Any()));
        }

        [Fact]
        public void Member_Can_Login_Successfully()
        {
            // Arrange
            NavigateToLogin();

            // Act
            _driver.FindElement(By.Id("username")).SendKeys("member@example.com");
            _driver.FindElement(By.Id("password")).SendKeys("member123!");
            _driver.FindElement(By.CssSelector("button.submit-btn")).Click();

            // Assert
            Assert.True(_wait.Until(d => d.FindElements(By.XPath("//h3[text()='My Issued Books']")).Any()));
        }

        [Fact]
        public void Login_With_Incorrect_Password_Fails()
        {
            // Arrange
            NavigateToLogin();

            // Act
            _driver.FindElement(By.Id("username")).SendKeys("admin");
            _driver.FindElement(By.Id("password")).SendKeys("wrong-password");
            _driver.FindElement(By.CssSelector("button.submit-btn")).Click();

            // Assert
            Assert.True(_wait.Until(d => d.FindElement(By.ClassName("error-message")).Text.Contains("Invalid credentials")));
            Assert.Equal(Config.BaseUrl + "login", _driver.Url.Split('?')[0]); // Remain on login page
        }

        [Fact]
        public void Login_With_Nonexistent_User_Fails()
        {
            // Arrange
            NavigateToLogin();

            // Act
            _driver.FindElement(By.Id("username")).SendKeys("notauser@example.com");
            _driver.FindElement(By.Id("password")).SendKeys("password");
            _driver.FindElement(By.CssSelector("button.submit-btn")).Click();

            // Assert
            Assert.True(_wait.Until(d => d.FindElement(By.ClassName("error-message")).Text.Contains("Invalid credentials")));
        }
    }
}
