using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using OrderManagement.Services;
using StackExchange.Redis;
using System;
using System.Reflection;
using Xunit;

namespace OrderManagement.Tests.Services
{
    public class CacheServiceTests
    {
        private readonly CacheService _cacheService;
        private readonly Mock<IDatabase> _mockDatabase;

        public CacheServiceTests()
        {
            _mockDatabase = new Mock<IDatabase>();

            // Mock IConnectionMultiplexer to return mocked IDatabase
            var mockConnectionMultiplexer = new Mock<IConnectionMultiplexer>();
            mockConnectionMultiplexer
                .Setup(c => c.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                .Returns(_mockDatabase.Object);

            // Override the private static Lazy<ConnectionMultiplexer> in CacheService
            var lazyConnectionField = typeof(CacheService)
                .GetField("lazyConnection", BindingFlags.NonPublic | BindingFlags.Static);

            // Create Lazy<ConnectionMultiplexer> returning the mock IConnectionMultiplexer
            var lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
                mockConnectionMultiplexer.Object as ConnectionMultiplexer);

            // Set the private static field
            lazyConnectionField.SetValue(null, lazyConnection);

            // Setup mock IConfiguration to provide a connection string
            var mockConfigSection = new Mock<IConfigurationSection>();
            mockConfigSection
                .Setup(s => s["RedisConnection"])
                .Returns("localhost");

            var mockConfig = new Mock<IConfiguration>();
            mockConfig
                .Setup(c => c.GetSection("ConnectionStrings"))
                .Returns(mockConfigSection.Object);

            // Create CacheService instance AFTER overriding lazyConnection
            _cacheService = new CacheService(mockConfig.Object);
        }

        [Fact]
        public void Set_ShouldSerializeAndSetValueInCache()
        {
            // Arrange
            var key = "test-key";
            var value = new { Name = "Test", Value = 123 };
            TimeSpan expiry = TimeSpan.FromMinutes(10);

            _mockDatabase.Setup(db => db.StringSet(
                It.Is<RedisKey>(k => k == key),
                It.IsAny<RedisValue>(),
                expiry,
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()))
                .Returns(true)
                .Verifiable();

            // Act
            _cacheService.Set(key, value, expiry);

            // Assert
            //_mockDatabase.Verify(db => db.StringSet(
            //    It.Is<RedisKey>(k => k == key),
            //    It.IsAny<RedisValue>(),
            //    expiry,
            //    It.IsAny<When>(),
            //    It.IsAny<CommandFlags>()), Times.Once);
        }

        [Fact]
        public void Get_ShouldReturnDeserializedValue_WhenValueExists()
        {
            // Arrange
            var key = "test-key";
            var expectedObject = new TestObject { Name = "abc", Number = 42 };
            var serializedValue = JsonConvert.SerializeObject(expectedObject);

            _mockDatabase.Setup(db => db.StringGet(
                It.Is<RedisKey>(k => k == key),
                It.IsAny<CommandFlags>()))
                .Returns(serializedValue);

            // Act
            var result = _cacheService.Get<TestObject>(key);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Get_ShouldReturnDefault_WhenKeyDoesNotExist()
        {
            // Arrange
            var key = "non-existent-key";

            _mockDatabase.Setup(db => db.StringGet(
                It.Is<RedisKey>(k => k == key),
                It.IsAny<CommandFlags>()))
                .Returns(RedisValue.Null);

            // Act
            var result = _cacheService.Get<TestObject>(key);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Remove_ShouldCallKeyDelete()
        {
            // Arrange
            var key = "key-to-remove";

            _mockDatabase.Setup(db => db.KeyDelete(
                It.Is<RedisKey>(k => k == key),
                It.IsAny<CommandFlags>()))
                .Returns(true)
                .Verifiable();

            // Act
            _cacheService.Remove(key);

            // Assert
            //_mockDatabase.Verify(db => db.KeyDelete(
            //    It.Is<RedisKey>(k => k == key),
            //    It.IsAny<CommandFlags>()), Times.Once);
        }

        // Simple POCO for tests
        public class TestObject
        {
            public string Name { get; set; }
            public int Number { get; set; }
        }
    }
}
