using FluentAssertions;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Application.Services;
using Xunit;

namespace ShieldPrompt.Tests.Unit.Application.Services;

public class StatusBarServiceTests
{
    private readonly StatusBarService _sut;

    public StatusBarServiceTests()
    {
        _sut = new StatusBarService();
    }

    #region GetFileSelectionSummary Tests

    [Fact]
    public void GetFileSelectionSummary_ReturnsCurrentCounts()
    {
        _sut.UpdateFileSelection(5, 20);

        var summary = _sut.GetFileSelectionSummary();

        summary.SelectedCount.Should().Be(5);
        summary.TotalCount.Should().Be(20);
    }

    [Fact]
    public void FileSelectionSummary_Display_FormatsCorrectly()
    {
        _sut.UpdateFileSelection(10, 50);

        var summary = _sut.GetFileSelectionSummary();

        summary.Display.Should().Be("10/50 files");
    }

    [Fact]
    public void GetFileSelectionSummary_InitiallyZero()
    {
        var summary = _sut.GetFileSelectionSummary();

        summary.SelectedCount.Should().Be(0);
        summary.TotalCount.Should().Be(0);
    }

    #endregion

    #region GetTokenUsageSummary Tests

    [Fact]
    public void GetTokenUsageSummary_CalculatesPercentage()
    {
        _sut.UpdateTokenUsage(64000);

        var summary = _sut.GetTokenUsageSummary(128000);

        summary.Percentage.Should().Be(50);
    }

    [Fact]
    public void TokenUsageSummary_IsCritical_WhenAbove95Percent()
    {
        _sut.UpdateTokenUsage(125000);

        var summary = _sut.GetTokenUsageSummary(128000);

        summary.IsCritical.Should().BeTrue();
    }

    [Fact]
    public void TokenUsageSummary_IsHigh_WhenAbove80Percent()
    {
        _sut.UpdateTokenUsage(110000);

        var summary = _sut.GetTokenUsageSummary(128000);

        summary.IsHigh.Should().BeTrue();
        summary.IsCritical.Should().BeFalse();
    }

    [Fact]
    public void TokenUsageSummary_Display_FormatsWithThousandsSeparator()
    {
        _sut.UpdateTokenUsage(64000);

        var summary = _sut.GetTokenUsageSummary(128000);

        summary.Display.Should().Contain("64");
        summary.Display.Should().Contain("128");
    }

    [Fact]
    public void GetTokenUsageSummary_WhenContextLimitIsZero_ReturnsZeroPercentage()
    {
        _sut.UpdateTokenUsage(1000);

        var summary = _sut.GetTokenUsageSummary(0);

        summary.Percentage.Should().Be(0);
    }

    #endregion

    #region GetSessionInfo Tests

    [Fact]
    public void GetSessionInfo_ReturnsDuration()
    {
        var summary = _sut.GetSessionInfo();

        summary.Duration.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);
    }

    [Fact]
    public void GetSessionInfo_ReturnsStartTime()
    {
        var beforeCreation = DateTime.UtcNow;
        var service = new StatusBarService();

        var summary = service.GetSessionInfo();

        summary.StartTime.Should().BeOnOrAfter(beforeCreation);
        summary.StartTime.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public void SessionSummary_Display_FormatsCorrectly()
    {
        var summary = new SessionSummary(TimeSpan.FromMinutes(65), DateTime.UtcNow);

        summary.Display.Should().Contain("1:05");
    }

    #endregion

    #region UpdateFileSelection Tests

    [Fact]
    public void UpdateFileSelection_UpdatesCounts()
    {
        _sut.UpdateFileSelection(3, 10);
        _sut.UpdateFileSelection(7, 15);

        var summary = _sut.GetFileSelectionSummary();

        summary.SelectedCount.Should().Be(7);
        summary.TotalCount.Should().Be(15);
    }

    #endregion

    #region UpdateTokenUsage Tests

    [Fact]
    public void UpdateTokenUsage_UpdatesUsedTokens()
    {
        _sut.UpdateTokenUsage(5000);
        _sut.UpdateTokenUsage(10000);

        var summary = _sut.GetTokenUsageSummary(100000);

        summary.UsedTokens.Should().Be(10000);
    }

    #endregion
}
