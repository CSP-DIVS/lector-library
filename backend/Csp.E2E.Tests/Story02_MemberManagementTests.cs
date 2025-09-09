
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;
using Xunit;

namespace Csp.E2E.Tests
{
    public class Story02_MemberManagementTests : IClassFixture<DriverFixture>
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public Story02_MemberManagementTests(DriverFixture fixture)
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
        public void Admin_Can_Register_And_Search_New_Member()
        {
            // Arrange
            LoginAsAdmin();
            var uniqueEmail = $"test-{Guid.NewGuid()}@example.com";
            var memberName = "Zebra Member"; // Use a name that will be unique for searching

            // Act: Register new member
            _driver.FindElement(By.LinkText("Member Management")).Click();
            _wait.Until(d => d.FindElement(By.CssSelector("button.primary-btn"))).Click(); // Register button
            _driver.FindElement(By.Id("name")).SendKeys(memberName);
            _driver.FindElement(By.Id("email")).SendKeys(uniqueEmail);
            _driver.FindElement(By.Id("phone")).SendKeys("555-123-4567");
            _driver.FindElement(By.Id("address")).SendKeys("123 Safari Way");
            _driver.FindElement(By.CssSelector("form button.primary-btn")).Click();
            Assert.True(_wait.Until(d => d.FindElement(By.ClassName("alert-success")).Text.Contains("registered successfully")));

            // Act: Search for the new member
            _driver.FindElement(By.Id("search-input")).SendKeys(memberName);

            // Assert
            _wait.Until(d => d.FindElements(By.CssSelector("tbody tr")).Count == 1);
            Assert.Contains(uniqueEmail, _driver.FindElement(By.CssSelector("tbody tr")).Text);
        }

        [Fact]
        public void Register_Member_With_Duplicate_Email_Shows_Error()
        {
            // Arrange
            LoginAsAdmin();

            // Act
            _driver.FindElement(By.LinkText("Member Management")).Click();
            _wait.Until(d => d.FindElement(By.CssSelector("button.primary-btn"))).Click();
            _driver.FindElement(By.Id("name")).SendKeys("Another Member");
            _driver.FindElement(By.Id("email")).SendKeys("member@example.com"); // Existing email
            _driver.FindElement(By.Id("phone")).SendKeys("1112223333");
            _driver.FindElement(By.CssSelector("form button.primary-btn")).Click();

            // Assert
            Assert.True(_wait.Until(d => d.FindElement(By.ClassName("alert-error")).Text.Contains("email address is already in use")));
        }

        [Fact]
        public void Admin_Can_Update_Member_Details()
        {
            // Arrange
            LoginAsAdmin();

            // Act
            _driver.FindElement(By.LinkText("Member Management")).Click();
            _wait.Until(d => d.FindElements(By.CssSelector("tbody tr")).Any());
            var memberRow = _driver.FindElement(By.XPath("//td[text()='member@example.com']/.."));
            memberRow.FindElement(By.CssSelector("button.icon-btn")).Click(); // Edit button
            var phoneInput = _wait.Until(d => d.FindElement(By.Id("phone")));
            phoneInput.Clear();
            var newPhone = "999-888-7777";
            phoneInput.SendKeys(newPhone);
            _driver.FindElement(By.CssSelector("form button.primary-btn")).Click();

            // Assert
            Assert.True(_wait.Until(d => d.FindElement(By.ClassName("alert-success")).Text.Contains("updated successfully")));
            var updatedRow = _driver.FindElement(By.XPath("//td[text()='member@example.com']/.."));
            Assert.Contains(newPhone, updatedRow.Text);
        }
    }
}
