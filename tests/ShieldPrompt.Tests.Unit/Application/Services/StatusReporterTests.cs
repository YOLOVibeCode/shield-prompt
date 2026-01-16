using FluentAssertions;
using ShieldPrompt.Application.Services;
using ShieldPrompt.Domain.Enums;
using ShieldPrompt.Domain.Records;
using Xunit;

namespace ShieldPrompt.Tests.Unit.Application.Services;

public class StatusReporterTests
{
    private readonly StatusReporter _sut;

    public StatusReporterTests()
    {
        _sut = new StatusReporter();
    }

    #region Initial State

    [Fact]
    public void InitialStatus_IsReady()
    {
        _sut.CurrentStatus.Message.Should().Be("Ready");
        _sut.CurrentStatus.Severity.Should().Be(StatusSeverity.Info);
        _sut.CurrentStatus.IsLoading.Should().BeFalse();
    }

    #endregion

    #region ReportInfo Tests

    [Fact]
    public void ReportInfo_SetsCorrectSeverity()
    {
        _sut.ReportInfo("Test message");

        _sut.CurrentStatus.Severity.Should().Be(StatusSeverity.Info);
        _sut.CurrentStatus.Message.Should().Be("Test message");
    }

    [Fact]
    public void ReportInfo_SetsIsLoadingFalse()
    {
        _sut.ReportInfo("Test");

        _sut.CurrentStatus.IsLoading.Should().BeFalse();
    }

    #endregion

    #region ReportSuccess Tests

    [Fact]
    public void ReportSuccess_SetsCorrectSeverity()
    {
        _sut.ReportSuccess("Done!");

        _sut.CurrentStatus.Severity.Should().Be(StatusSeverity.Success);
        _sut.CurrentStatus.Message.Should().Be("Done!");
    }

    #endregion

    #region ReportWarning Tests

    [Fact]
    public void ReportWarning_SetsCorrectSeverity()
    {
        _sut.ReportWarning("Careful!");

        _sut.CurrentStatus.Severity.Should().Be(StatusSeverity.Warning);
        _sut.CurrentStatus.Message.Should().Be("Careful!");
    }

    #endregion

    #region ReportError Tests

    [Fact]
    public void ReportError_SetsCorrectSeverity()
    {
        _sut.ReportError("Failed!");

        _sut.CurrentStatus.Severity.Should().Be(StatusSeverity.Error);
        _sut.CurrentStatus.Message.Should().Be("Failed!");
    }

    #endregion

    #region ReportProgress Tests

    [Fact]
    public void ReportProgress_SetsIsLoadingTrue()
    {
        _sut.ReportProgress(50, 100);

        _sut.CurrentStatus.IsLoading.Should().BeTrue();
    }

    [Fact]
    public void ReportProgress_CalculatesPercentage()
    {
        _sut.ReportProgress(50, 100, "Loading...");

        _sut.CurrentStatus.Progress.Should().NotBeNull();
        _sut.CurrentStatus.Progress!.Percentage.Should().Be(50);
    }

    [Fact]
    public void ReportProgress_UsesDescription()
    {
        _sut.ReportProgress(25, 100, "Processing files...");

        _sut.CurrentStatus.Message.Should().Be("Processing files...");
    }

    [Fact]
    public void ReportProgress_WithoutDescription_GeneratesMessage()
    {
        _sut.ReportProgress(50, 100);

        _sut.CurrentStatus.Message.Should().Contain("50");
    }

    #endregion

    #region ClearProgress Tests

    [Fact]
    public void ClearProgress_ResetsToReady()
    {
        _sut.ReportProgress(50, 100);

        _sut.ClearProgress();

        _sut.CurrentStatus.Message.Should().Be("Ready");
        _sut.CurrentStatus.IsLoading.Should().BeFalse();
        _sut.CurrentStatus.Progress.Should().BeNull();
    }

    #endregion

    #region StatusChanged Event Tests

    [Fact]
    public void StatusChanged_FiresOnReportInfo()
    {
        StatusInfo? received = null;
        _sut.StatusChanged += (s, e) => received = e;

        _sut.ReportInfo("Test");

        received.Should().NotBeNull();
        received!.Message.Should().Be("Test");
    }

    [Fact]
    public void StatusChanged_FiresOnReportSuccess()
    {
        StatusInfo? received = null;
        _sut.StatusChanged += (s, e) => received = e;

        _sut.ReportSuccess("Done");

        received.Should().NotBeNull();
        received!.Severity.Should().Be(StatusSeverity.Success);
    }

    [Fact]
    public void StatusChanged_FiresOnReportProgress()
    {
        StatusInfo? received = null;
        _sut.StatusChanged += (s, e) => received = e;

        _sut.ReportProgress(50, 100);

        received.Should().NotBeNull();
        received!.IsLoading.Should().BeTrue();
    }

    [Fact]
    public void StatusChanged_FiresOnClearProgress()
    {
        StatusInfo? received = null;
        _sut.ReportProgress(50, 100);
        _sut.StatusChanged += (s, e) => received = e;

        _sut.ClearProgress();

        received.Should().NotBeNull();
        received!.IsLoading.Should().BeFalse();
    }

    #endregion
}
