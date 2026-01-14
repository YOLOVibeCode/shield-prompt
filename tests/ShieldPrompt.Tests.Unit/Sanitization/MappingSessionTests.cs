using FluentAssertions;
using ShieldPrompt.Domain.Enums;
using ShieldPrompt.Sanitization.Interfaces;
using ShieldPrompt.Sanitization.Services;

namespace ShieldPrompt.Tests.Unit.Sanitization;

public class MappingSessionTests
{
    [Fact]
    public void MappingSession_WhenCreated_HasValidSessionId()
    {
        // Act
        var sut = new MappingSession();

        // Assert
        sut.SessionId.Should().NotBeNullOrEmpty();
        Guid.TryParse(sut.SessionId, out _).Should().BeTrue();
    }

    [Fact]
    public void MappingSession_WhenCreated_HasCreatedAtTimestamp()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var sut = new MappingSession();

        // Assert
        var after = DateTime.UtcNow;
        sut.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void MappingSession_WhenCreated_ExpiresInFourHours()
    {
        // Arrange
        var sut = new MappingSession();

        // Act
        var expectedExpiry = sut.CreatedAt.AddHours(4);

        // Assert
        sut.ExpiresAt.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void AddMapping_WithNewValue_StoresMapping()
    {
        // Arrange
        var sut = new MappingSession();

        // Act
        sut.AddMapping("ProductionDB", "DATABASE_0", PatternCategory.Database);

        // Assert
        sut.GetOriginal("DATABASE_0").Should().Be("ProductionDB");
        sut.GetAlias("ProductionDB").Should().Be("DATABASE_0");
    }

    [Fact]
    public void AddMapping_WithSameOriginalTwice_KeepsFirstAlias()
    {
        // Arrange
        var sut = new MappingSession();

        // Act
        sut.AddMapping("ProductionDB", "DATABASE_0", PatternCategory.Database);
        sut.AddMapping("ProductionDB", "DATABASE_1", PatternCategory.Database); // Attempt to overwrite

        // Assert
        sut.GetAlias("ProductionDB").Should().Be("DATABASE_0"); // Should keep first
    }

    [Fact]
    public void GetOriginal_WithNonExistentAlias_ReturnsNull()
    {
        // Arrange
        var sut = new MappingSession();

        // Act
        var result = sut.GetOriginal("NONEXISTENT_0");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetAlias_WithNonExistentOriginal_ReturnsNull()
    {
        // Arrange
        var sut = new MappingSession();

        // Act
        var result = sut.GetAlias("NonExistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetAllMappings_WithMultipleMappings_ReturnsAll()
    {
        // Arrange
        var sut = new MappingSession();
        sut.AddMapping("ProductionDB", "DATABASE_0", PatternCategory.Database);
        sut.AddMapping("192.168.1.50", "IP_ADDRESS_0", PatternCategory.IPAddress);

        // Act
        var mappings = sut.GetAllMappings();

        // Assert
        mappings.Should().HaveCount(2);
        mappings["DATABASE_0"].Should().Be("ProductionDB");
        mappings["IP_ADDRESS_0"].Should().Be("192.168.1.50");
    }

    [Fact]
    public void Clear_RemovesAllMappings()
    {
        // Arrange
        var sut = new MappingSession();
        sut.AddMapping("ProductionDB", "DATABASE_0", PatternCategory.Database);
        sut.AddMapping("192.168.1.50", "IP_ADDRESS_0", PatternCategory.IPAddress);

        // Act
        sut.Clear();

        // Assert
        sut.GetAllMappings().Should().BeEmpty();
        sut.GetOriginal("DATABASE_0").Should().BeNull();
    }

    [Fact]
    public void ExtendSession_IncreasesExpiryTime()
    {
        // Arrange
        var sut = new MappingSession();
        var originalExpiry = sut.ExpiresAt;

        // Act
        sut.ExtendSession(TimeSpan.FromHours(2));

        // Assert
        sut.ExpiresAt.Should().BeAfter(originalExpiry);
        sut.ExpiresAt.Should().BeCloseTo(
            originalExpiry.AddHours(2),
            TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MappingSession_IsThreadSafe()
    {
        // Arrange
        var sut = new MappingSession();
        var tasks = new List<Task>();

        // Act - Add mappings from multiple threads
        for (int i = 0; i < 10; i++)
        {
            var index = i;
            tasks.Add(Task.Run(() =>
            {
                sut.AddMapping($"Value{index}", $"ALIAS_{index}", PatternCategory.Custom);
            }));
        }

        Task.WaitAll(tasks.ToArray());

        // Assert
        sut.GetAllMappings().Should().HaveCount(10);
    }
}

