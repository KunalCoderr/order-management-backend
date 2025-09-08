using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonUtils;

namespace CommonUtils.Tests
{
    public class CommomUtilsTest
    {
        [Fact]
        public void IsValidEmail_ValidAndInvalidEmails_ReturnsExpected()
        {
            Assert.True(CommonUtils.IsValidEmail("test@example.com"));
            Assert.False(CommonUtils.IsValidEmail("bad-email"));
            Assert.False(CommonUtils.IsValidEmail(null));
            Assert.False(CommonUtils.IsValidEmail("   "));
        }

        [Fact]
        public void IsNotNullOrEmpty_NullEmptyWhitespace_ReturnsFalse()
        {
            Assert.False(CommonUtils.IsNotNullOrEmpty(null));
            Assert.False(CommonUtils.IsNotNullOrEmpty(""));
            Assert.False(CommonUtils.IsNotNullOrEmpty("    "));
            Assert.True(CommonUtils.IsNotNullOrEmpty("value"));
        }

        [Fact]
        public void ToTitleCase_ConvertsCorrectly()
        {
            string input = "hello world";
            string expected = "Hello World";
            Assert.Equal(expected, CommonUtils.ToTitleCase(input));

            // Null or empty returns as is
            Assert.Null(CommonUtils.ToTitleCase(null));
            Assert.Equal("", CommonUtils.ToTitleCase(""));
        }

        [Fact]
        public void TryParseInt_ValidAndInvalidInputs()
        {
            int result = 0;
            Assert.True(CommonUtils.TryParseInt("123", ref result));
            Assert.Equal(123, result);

            Assert.False(CommonUtils.TryParseInt("abc", ref result));
        }

        [Fact]
        public void EncryptDecryptString_RoundTrip_Success()
        {
            string original = "Secret message!";
            string key = "MySuperSecretKey1234567890";

            string encrypted = CommonUtils.EncryptString(original, key);
            Assert.NotNull(encrypted);
            Assert.NotEqual(original, encrypted);

            string decrypted = CommonUtils.DecryptString(encrypted, key);
            Assert.Equal(original, decrypted);
        }

        [Fact]
        public void EncryptString_InvalidInputs_ReturnsNull()
        {
            Assert.Null(CommonUtils.EncryptString(null, "key"));
            Assert.Null(CommonUtils.EncryptString("data", null));
        }

        [Fact]
        public void DecryptString_InvalidInputs_ReturnsNull()
        {
            Assert.Null(CommonUtils.DecryptString(null, "key"));
            Assert.Null(CommonUtils.DecryptString("invalidbase64", "key"));
            Assert.Null(CommonUtils.DecryptString("validbase64butwrongdata", null));
        }

        [Fact]
        public void GenerateRandomString_ReturnsCorrectLengthAndContent()
        {
            int length = 10;
            string randomStr = CommonUtils.GenerateRandomString(length);
            Assert.NotNull(randomStr);
            Assert.Equal(length, randomStr.Length);
            Assert.Matches("^[A-Z0-9]+$", randomStr);
        }

        [Fact]
        public void SafeToString_NullInput_ReturnsEmptyString()
        {
            string result = CommonUtils.SafeToString(null);
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void SafeToString_StringInput_ReturnsSameString()
        {
            string result = CommonUtils.SafeToString("hello");
            Assert.Equal("hello", result);
        }

        [Fact]
        public void SafeToString_IntegerInput_ReturnsStringRepresentation()
        {
            string result = CommonUtils.SafeToString(123);
            Assert.Equal("123", result);
        }

        [Fact]
        public void SafeToInt_NullInput_ReturnsDefaultValue()
        {
            int result = CommonUtils.SafeToInt(null);
            Assert.Equal(0, result);
        }

        [Fact]
        public void SafeToInt_ValidIntegerString_ReturnsParsedInteger()
        {
            int result = CommonUtils.SafeToInt("456");
            Assert.Equal(456, result);
        }

        [Fact]
        public void SafeToInt_InvalidIntegerString_ReturnsDefaultValue()
        {
            int result = CommonUtils.SafeToInt("abc");
            Assert.Equal(0, result);
        }

        [Fact]
        public void SafeToInt_InvalidIntegerString_WithCustomDefaultValue_ReturnsCustomDefault()
        {
            int result = CommonUtils.SafeToInt("abc", 999);
            Assert.Equal(999, result);
        }

        [Fact]
        public void SafeToInt_IntegerInput_ReturnsInteger()
        {
            int result = CommonUtils.SafeToInt(789);
            Assert.Equal(789, result);
        }

        [Theory]
        [InlineData("hello123", @"^\w+$", true)]
        [InlineData("hello!", @"^\w+$", false)]
        [InlineData("", @"^\w+$", false)]
        [InlineData("test", "", false)]
        [InlineData(null, @"^\w+$", false)]
        public void MatchesPattern_ValidAndInvalidInputs_ReturnsExpected(string value, string pattern, bool expected)
        {
            bool result = CommonUtils.MatchesPattern(value, pattern);
            Assert.Equal(expected, result);
        }
        public static bool TryParseDate(string value, out DateTime result)
        {
            return DateTime.TryParse(value, out result);
        }

        [Fact]
        public void TryParseDate_ValidDateString_ReturnsTrue()
        {
            string input = "2023-09-05";
            bool success = TryParseDate(input, out DateTime result);

            Assert.True(success);
            Assert.Equal(2023, result.Year);
            Assert.Equal(9, result.Month);
            Assert.Equal(5, result.Day);
        }

        [Fact]
        public void TryParseDate_InvalidDateString_ReturnsFalse()
        {
            string input = "not-a-date"; // <-- invalid string now
            bool success = TryParseDate(input, out DateTime result);

            Assert.False(success);
            Assert.Equal(default, result);
        }

        [Fact]
        public void GetCurrentUtcIsoDate_ReturnsValidIso8601Format()
        {
            string isoDate = CommonUtils.GetCurrentUtcIsoDate();

            // This preserves the UTC kind
            var result = DateTime.Parse(isoDate, null, System.Globalization.DateTimeStyles.AdjustToUniversal);

            Assert.Equal(DateTimeKind.Utc, result.Kind);
        }

        [Fact]
        public void GetAppSetting_ExistingKey_ReturnsValue()
        {
            // Arrange: mock or set via ConfigurationManager in app.config or test context
            string key = "TestSetting";
            string expectedValue = "TestValue";

            // Simulate (since real config access is limited in unit test context)
            System.Configuration.ConfigurationManager.AppSettings.Set(key, expectedValue);

            // Act
            string value = CommonUtils.GetAppSetting(key);

            // Assert
            Assert.Equal(expectedValue, value);
        }

        [Fact]
        public void GetAppSetting_NonExistingKey_ReturnsNull()
        {
            string value = CommonUtils.GetAppSetting("NonExistingKey");
            Assert.Null(value);
        }
    }
}
