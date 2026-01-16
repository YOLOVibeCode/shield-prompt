using FluentAssertions;
using ShieldPrompt.Domain.Enums;
using ShieldPrompt.Domain.Records;
using Xunit;

namespace ShieldPrompt.Tests.Unit.Domain.Records;

public class ApplyResultTests
{
    [Fact]
    public void AllSucceeded_WhenNoFailures_ReturnsTrue()
    {
        var result = new ApplyResult(3, 0, Array.Empty<AppliedOperation>(), Array.Empty<string>(), "backup-1");

        result.AllSucceeded.Should().BeTrue();
    }

    [Fact]
    public void AllSucceeded_WhenHasFailures_ReturnsFalse()
    {
        var result = new ApplyResult(2, 1, Array.Empty<AppliedOperation>(), new[] { "error" }, "backup-1");

        result.AllSucceeded.Should().BeFalse();
    }

    [Fact]
    public void AnySucceeded_WhenHasSuccesses_ReturnsTrue()
    {
        var result = new ApplyResult(1, 2, Array.Empty<AppliedOperation>(), Array.Empty<string>(), "backup-1");

        result.AnySucceeded.Should().BeTrue();
    }

    [Fact]
    public void AnySucceeded_WhenNoSuccesses_ReturnsFalse()
    {
        var result = new ApplyResult(0, 3, Array.Empty<AppliedOperation>(), Array.Empty<string>(), "backup-1");

        result.AnySucceeded.Should().BeFalse();
    }

    [Fact]
    public void TotalCount_ReturnsSumOfSuccessAndFailure()
    {
        var result = new ApplyResult(5, 3, Array.Empty<AppliedOperation>(), Array.Empty<string>(), "backup-1");

        result.TotalCount.Should().Be(8);
    }
}
