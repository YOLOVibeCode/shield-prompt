using FluentAssertions;
using ShieldPrompt.App.ViewModels;
using ShieldPrompt.Domain.Enums;
using Xunit;

namespace ShieldPrompt.Tests.Unit.ViewModels;

/// <summary>
/// Unit tests for ResponseStatistics record.
/// Validates statistics aggregation for LLM responses.
/// </summary>
public class ResponseStatisticsTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesInstance()
    {
        // Arrange & Act
        var stats = new ResponseStatistics(
            TotalOperations: 5,
            UpdateCount: 2,
            CreateCount: 2,
            DeleteCount: 1,
            WarningCount: 1,
            EstimatedLinesAffected: 450,
            DetectedFormat: ResponseFormat.HybridXmlMarkdown,
            ParsedAt: DateTime.UtcNow);
        
        // Assert
        stats.TotalOperations.Should().Be(5);
        stats.UpdateCount.Should().Be(2);
        stats.CreateCount.Should().Be(2);
        stats.DeleteCount.Should().Be(1);
        stats.WarningCount.Should().Be(1);
        stats.EstimatedLinesAffected.Should().Be(450);
        stats.DetectedFormat.Should().Be(ResponseFormat.HybridXmlMarkdown);
    }
    
    [Fact]
    public void HasWarnings_WithWarningCount_ReturnsTrue()
    {
        // Arrange
        var stats = new ResponseStatistics(
            TotalOperations: 3,
            UpdateCount: 2,
            CreateCount: 1,
            DeleteCount: 0,
            WarningCount: 2,
            EstimatedLinesAffected: 100,
            DetectedFormat: ResponseFormat.HybridXmlMarkdown,
            ParsedAt: DateTime.UtcNow);
        
        // Act & Assert
        stats.HasWarnings.Should().BeTrue();
    }
    
    [Fact]
    public void HasWarnings_WithNoWarnings_ReturnsFalse()
    {
        // Arrange
        var stats = new ResponseStatistics(
            TotalOperations: 3,
            UpdateCount: 2,
            CreateCount: 1,
            DeleteCount: 0,
            WarningCount: 0,
            EstimatedLinesAffected: 100,
            DetectedFormat: ResponseFormat.HybridXmlMarkdown,
            ParsedAt: DateTime.UtcNow);
        
        // Act & Assert
        stats.HasWarnings.Should().BeFalse();
    }
    
    [Fact]
    public void HasDestructiveOperations_WithDeletes_ReturnsTrue()
    {
        // Arrange
        var stats = new ResponseStatistics(
            TotalOperations: 2,
            UpdateCount: 1,
            CreateCount: 0,
            DeleteCount: 1,
            WarningCount: 0,
            EstimatedLinesAffected: 50,
            DetectedFormat: ResponseFormat.HybridXmlMarkdown,
            ParsedAt: DateTime.UtcNow);
        
        // Act & Assert
        stats.HasDestructiveOperations.Should().BeTrue();
    }
    
    [Fact]
    public void HasDestructiveOperations_WithNoDeletes_ReturnsFalse()
    {
        // Arrange
        var stats = new ResponseStatistics(
            TotalOperations: 3,
            UpdateCount: 2,
            CreateCount: 1,
            DeleteCount: 0,
            WarningCount: 0,
            EstimatedLinesAffected: 100,
            DetectedFormat: ResponseFormat.HybridXmlMarkdown,
            ParsedAt: DateTime.UtcNow);
        
        // Act & Assert
        stats.HasDestructiveOperations.Should().BeFalse();
    }
    
    [Fact]
    public void TotalOperations_MatchesSumOfOperationTypes()
    {
        // Arrange
        var stats = new ResponseStatistics(
            TotalOperations: 6,
            UpdateCount: 3,
            CreateCount: 2,
            DeleteCount: 1,
            WarningCount: 0,
            EstimatedLinesAffected: 200,
            DetectedFormat: ResponseFormat.HybridXmlMarkdown,
            ParsedAt: DateTime.UtcNow);
        
        // Act
        var sum = stats.UpdateCount + stats.CreateCount + stats.DeleteCount;
        
        // Assert
        sum.Should().Be(stats.TotalOperations);
    }
    
    [Fact]
    public void IsEmpty_WithNoOperations_ReturnsTrue()
    {
        // Arrange
        var stats = new ResponseStatistics(
            TotalOperations: 0,
            UpdateCount: 0,
            CreateCount: 0,
            DeleteCount: 0,
            WarningCount: 0,
            EstimatedLinesAffected: 0,
            DetectedFormat: ResponseFormat.HybridXmlMarkdown,
            ParsedAt: DateTime.UtcNow);
        
        // Act & Assert
        stats.IsEmpty.Should().BeTrue();
    }
    
    [Fact]
    public void IsEmpty_WithOperations_ReturnsFalse()
    {
        // Arrange
        var stats = new ResponseStatistics(
            TotalOperations: 1,
            UpdateCount: 1,
            CreateCount: 0,
            DeleteCount: 0,
            WarningCount: 0,
            EstimatedLinesAffected: 10,
            DetectedFormat: ResponseFormat.HybridXmlMarkdown,
            ParsedAt: DateTime.UtcNow);
        
        // Act & Assert
        stats.IsEmpty.Should().BeFalse();
    }
    
    [Fact]
    public void SummaryText_WithMixedOperations_ReturnsFormattedString()
    {
        // Arrange
        var stats = new ResponseStatistics(
            TotalOperations: 5,
            UpdateCount: 2,
            CreateCount: 2,
            DeleteCount: 1,
            WarningCount: 1,
            EstimatedLinesAffected: 450,
            DetectedFormat: ResponseFormat.HybridXmlMarkdown,
            ParsedAt: DateTime.UtcNow);
        
        // Act
        var summary = stats.SummaryText;
        
        // Assert
        summary.Should().Contain("5 operations");
        summary.Should().Contain("2 updates");
        summary.Should().Contain("2 creates");
        summary.Should().Contain("1 delete");
    }
    
    [Fact]
    public void SummaryText_WithEmptyStats_ReturnsNoOperationsMessage()
    {
        // Arrange
        var stats = new ResponseStatistics(
            TotalOperations: 0,
            UpdateCount: 0,
            CreateCount: 0,
            DeleteCount: 0,
            WarningCount: 0,
            EstimatedLinesAffected: 0,
            DetectedFormat: ResponseFormat.HybridXmlMarkdown,
            ParsedAt: DateTime.UtcNow);
        
        // Act
        var summary = stats.SummaryText;
        
        // Assert
        summary.Should().Contain("No operations");
    }
}

