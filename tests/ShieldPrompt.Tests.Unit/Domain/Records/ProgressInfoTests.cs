using FluentAssertions;
using ShieldPrompt.Domain.Records;
using Xunit;

namespace ShieldPrompt.Tests.Unit.Domain.Records;

public class ProgressInfoTests
{
    [Fact]
    public void Percentage_CalculatesCorrectly()
    {
        var progress = new ProgressInfo(50, 100);

        progress.Percentage.Should().Be(50);
    }

    [Fact]
    public void Percentage_WhenTotalIsZero_ReturnsZero()
    {
        var progress = new ProgressInfo(50, 0);

        progress.Percentage.Should().Be(0);
    }

    [Fact]
    public void IsIndeterminate_WhenTotalIsZero_ReturnsTrue()
    {
        var progress = new ProgressInfo(0, 0);

        progress.IsIndeterminate.Should().BeTrue();
    }

    [Fact]
    public void IsIndeterminate_WhenTotalIsNegative_ReturnsTrue()
    {
        var progress = new ProgressInfo(0, -1);

        progress.IsIndeterminate.Should().BeTrue();
    }

    [Fact]
    public void IsIndeterminate_WhenTotalIsPositive_ReturnsFalse()
    {
        var progress = new ProgressInfo(50, 100);

        progress.IsIndeterminate.Should().BeFalse();
    }

    [Fact]
    public void IsComplete_WhenCurrentEqualsTotal_ReturnsTrue()
    {
        var progress = new ProgressInfo(100, 100);

        progress.IsComplete.Should().BeTrue();
    }

    [Fact]
    public void IsComplete_WhenCurrentLessThanTotal_ReturnsFalse()
    {
        var progress = new ProgressInfo(50, 100);

        progress.IsComplete.Should().BeFalse();
    }

    [Fact]
    public void Indeterminate_CreatesIndeterminateProgress()
    {
        var progress = ProgressInfo.Indeterminate("Loading...");

        progress.IsIndeterminate.Should().BeTrue();
        progress.Description.Should().Be("Loading...");
    }
}
