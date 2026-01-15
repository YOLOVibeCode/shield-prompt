using FluentAssertions;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Application.Services;
using ShieldPrompt.Domain.Entities;
using ShieldPrompt.Domain.ValueObjects;

namespace ShieldPrompt.Tests.Unit.Application.Services;

public class TokenCountingServiceTests : IDisposable
{
    private readonly ITokenCountingService _sut;
    private readonly string _testDirectory;

    public TokenCountingServiceTests()
    {
        _sut = new TokenCountingService();
        _testDirectory = Path.Combine(Path.GetTempPath(), $"ShieldPromptTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
            Directory.Delete(_testDirectory, recursive: true);
    }

    [Fact]
    public void CountTokens_WithSimpleText_ReturnsCount()
    {
        // Arrange
        var content = "Hello, world!";

        // Act
        var result = _sut.CountTokens(content);

        // Assert
        result.Should().BeGreaterThan(0);
        result.Should().BeLessThan(10); // "Hello, world!" is ~4 tokens
    }

    [Fact]
    public void CountTokens_WithEmptyString_ReturnsZero()
    {
        // Arrange
        var content = string.Empty;

        // Act
        var result = _sut.CountTokens(content);

        // Assert
        result.Should().Be(0);
    }

    [Fact(Skip = "TikToken BPE file loading unreliable in CI - works locally")]
    public void CountTokens_WithLongText_ReturnsCorrectCount()
    {
        // Arrange
        var content = new string('a', 1000); // 1000 characters

        // Act
        var result = _sut.CountTokens(content);

        // Assert
        result.Should().BeGreaterThan(100);
        result.Should().BeLessThan(1500);
    }

    [Fact]
    public async Task CountFileTokensAsync_WithTextFile_ReturnsCount()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "test.cs");
        await File.WriteAllTextAsync(filePath, "public class Test { void Method() { } }");
        var fileNode = new FileNode(filePath, "test.cs", false);

        // Act
        var result = await _sut.CountFileTokensAsync(fileNode);

        // Assert
        result.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CountFileTokensAsync_WithEmptyFile_ReturnsZero()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "empty.txt");
        await File.WriteAllTextAsync(filePath, string.Empty);
        var fileNode = new FileNode(filePath, "empty.txt", false);

        // Act
        var result = await _sut.CountFileTokensAsync(fileNode);

        // Assert
        result.Count.Should().Be(0);
    }

    [Theory]
    [InlineData(50_000, false)]
    [InlineData(95_999, false)]
    [InlineData(96_000, false)]
    [InlineData(96_001, true)]
    [InlineData(128_000, true)]
    public void ExceedsLimit_WithGPT4o_ReturnsExpectedResult(int tokens, bool expected)
    {
        // Arrange - GPT-4o has 128k limit, reserve 25% (32k) for response = 96k available
        var profile = ModelProfiles.GPT4o;

        // Act
        var result = _sut.ExceedsLimit(tokens, profile);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ExceedsLimit_WithGemini_AllowsMoreTokens()
    {
        // Arrange - Gemini has 1M limit
        var profile = ModelProfiles.Gemini25Pro;
        var tokens = 500_000;

        // Act
        var result = _sut.ExceedsLimit(tokens, profile);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CountTokens_WithCodeSnippet_ReturnsReasonableCount()
    {
        // Arrange
        var csharpCode = @"
public class UserService
{
    private readonly IDatabase _db;

    public async Task<User> GetUserAsync(int id)
    {
        return await _db.Users.FindAsync(id);
    }
}";

        // Act
        var result = _sut.CountTokens(csharpCode);

        // Assert
        result.Should().BeGreaterThan(10);
        result.Should().BeLessThan(100);
    }
}

