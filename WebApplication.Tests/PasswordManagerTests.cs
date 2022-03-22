using System;
using Xunit;

namespace WebApplication.Tests
{
    public class PasswordManagerTests
    {
        #region GetTempPassword
        [Fact]
        public void GetTempPassword_ValidInput_ReturnsCorrectTempPassword()
        {
            string userID = "TestUser";
            var specificDate = DateTime.Parse("3/22/2022 7:54:11 PM");

            var tempPassword = PasswordManager.GetTempPassword(userID, specificDate, out DateTime? expireDate);

            Assert.Equal("29GDBxJQ/q6azVWoamTrdQ==My8yMi8yMDIyIDU6NTQ6MTEgUE0=", tempPassword);
        }

        [Fact]
        public void GetTempPassword_ValidInput_ExpirationDateCalculatedCorrectly()
        {
            string userID = "TestUser";
            var specificDate = DateTime.Parse("3/22/2022 7:54:11 PM");

            PasswordManager.GetTempPassword(userID, specificDate, out DateTime? expireDate);

            var expectedExpireDate = specificDate.AddSeconds(PasswordManager.ExpireTimeout);
            Assert.Equal(expectedExpireDate, expireDate);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void GetTempPassword_InvalidUserIDInput_ShouldThrowArgumentNullException(string userID)
        {
            Action methodCall = () => PasswordManager.GetTempPassword(userID, DateTime.Now, out DateTime? expireDate);
            Assert.Throws<ArgumentNullException>(methodCall);
        }
        #endregion

        #region LoginUser
        [Fact]
        public void LoginUser_ValidInput_ShouldReturnTrue()
        {
            string userID = "TestUser";

            var tempPassword = PasswordManager.GetTempPassword(userID, DateTime.Now, out DateTime? expireDate);
            bool loginResult = PasswordManager.LoginUser(userID, tempPassword);

            Assert.True(loginResult);
        }

        [Fact]
        public void LoginUser_WrongUser_ShouldReturnFalse()
        {
            string userID = "TestUser";

            var tempPassword = PasswordManager.GetTempPassword(userID, DateTime.Now, out DateTime? expireDate);
            bool loginResult = PasswordManager.LoginUser("OtherUser", tempPassword);

            Assert.False(loginResult);
        }

        [Fact]
        public void LoginUser_WrongPassword_ShouldReturnFalse()
        {
            bool loginResult = PasswordManager.LoginUser("TestUser", "InvalidPassword");

            Assert.False(loginResult);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void LoginUser_InvalidPasswordInput_ShouldReturnFalse(string password)
        {
            bool loginResult = PasswordManager.LoginUser("TestUser", password);

            Assert.False(loginResult);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void LoginUser_InvalidUserIDInput_ShouldThrowArgumentNullException(string userID)
        {
            bool loginResult = PasswordManager.LoginUser(userID, "dummypassword");

            Assert.False(loginResult);
        }

        [Fact]
        public void LoginUser_ExpiredTime_ShouldReturnFalse()
        {
            string userID = "TestUser";
            var pastDate = DateTime.Now.AddSeconds(-PasswordManager.ExpireTimeout - 1);

            var tempPassword = PasswordManager.GetTempPassword(userID, pastDate, out DateTime? expireDate);
            bool loginResult = PasswordManager.LoginUser(userID, tempPassword);

            Assert.False(loginResult);
        }
        #endregion
    }
}
