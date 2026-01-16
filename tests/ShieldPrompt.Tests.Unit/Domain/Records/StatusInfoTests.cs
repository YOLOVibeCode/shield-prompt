using FluentAssertions;
using ShieldPrompt.Domain.Enums;
using ShieldPrompt.Domain.Records;
using Xunit;

namespace ShieldPrompt.Tests.Unit.Domain.Records;

public class StatusInfoTests
{
    [Fact]
    public void Ready_ReturnsDefaultReadyStatus()
    {
        var status = StatusInfo.Ready;

        status.Message.Should().Be("Ready");
        status.Severity.Should().Be(StatusSeverity.Info);
        status.IsLoading.Should().BeFalse();
        status.Progress.Should().BeNull();
    }

    [Fact]
    public void Info_CreatesInfoStatus()
    {
        var status = StatusInfo.Info("Test message");

        status.Message.Should().Be("Test message");
        status.Severity.Should().Be(StatusSeverity.Info);
        status.IsLoading.Should().BeFalse();
    }

    [Fact]
    public void Success_CreatesSuccessStatus()
    {
        var status = StatusInfo.Success("Operation completed");

        status.Message.Should().Be("Operation completed");
        status.Severity.Should().Be(StatusSeverity.Success);
        status.IsLoading.Should().BeFalse();
    }

    [Fact]
    public void Warning_CreatesWarningStatus()
    {
        var status = StatusInfo.Warning("Careful!");

        status.Message.Should().Be("Careful!");
        status.Severity.Should().Be(StatusSeverity.Warning);
        status.IsLoading.Should().BeFalse();
    }

    [Fact]
    public void Error_CreatesErrorStatus()
    {
        var status = StatusInfo.Error("Something failed");

        status.Message.Should().Be("Something failed");
        status.Severity.Should().Be(StatusSeverity.Error);
        status.IsLoading.Should().BeFalse();
    }

    [Fact]
    public void Loading_CreatesLoadingStatus()
    {
        var progress = new ProgressInfo(50, 100);
        var status = StatusInfo.Loading("Processing...", progress);

        status.Message.Should().Be("Processing...");
        status.Severity.Should().Be(StatusSeverity.Info);
        status.IsLoading.Should().BeTrue();
        status.Progress.Should().Be(progress);
    }

    [Fact]
    public void Loading_WithoutProgress_HasNullProgress()
    {
        var status = StatusInfo.Loading("Loading...");

        status.IsLoading.Should().BeTrue();
        status.Progress.Should().BeNull();
    }
}
