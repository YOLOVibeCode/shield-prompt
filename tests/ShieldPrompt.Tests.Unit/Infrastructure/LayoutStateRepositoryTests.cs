using FluentAssertions;
using ShieldPrompt.Infrastructure.Interfaces;
using ShieldPrompt.Infrastructure.Services;

namespace ShieldPrompt.Tests.Unit.Infrastructure;

/// <summary>
/// Tests for layout state persistence.
/// TDD: Tests written BEFORE implementation.
/// </summary>
public class LayoutStateRepositoryTests
{
    private readonly string _testDirectory;
    private readonly JsonLayoutStateRepository _sut;
    
    public LayoutStateRepositoryTests()
    {
        // Use temp directory for tests
        _testDirectory = Path.Combine(Path.GetTempPath(), "ShieldPromptTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
        
        _sut = new JsonLayoutStateRepository(_testDirectory);
    }
    
    [Fact]
    public async Task SaveLayoutStateAsync_WithValidState_PersistsToFile()
    {
        // Arrange
        var state = new LayoutState(
            FileTreeWidth: 350,
            PromptBuilderHeight: 0.6,
            IsFileTreeCollapsed: false,
            IsPromptBuilderCollapsed: false,
            IsPreviewCollapsed: false);
        
        // Act
        await _sut.SaveLayoutStateAsync(state);
        
        // Assert
        var loaded = await _sut.LoadLayoutStateAsync();
        loaded.Should().NotBeNull();
        loaded!.FileTreeWidth.Should().Be(350);
        loaded.PromptBuilderHeight.Should().Be(0.6);
    }
    
    [Fact]
    public async Task LoadLayoutStateAsync_WhenNoStateExists_ReturnsNull()
    {
        // Act
        var result = await _sut.LoadLayoutStateAsync();
        
        // Assert
        result.Should().BeNull();
    }
    
    [Fact]
    public async Task LoadLayoutStateAsync_AfterSave_ReturnsExactState()
    {
        // Arrange
        var state = new LayoutState(
            FileTreeWidth: 400,
            PromptBuilderHeight: 0.7,
            IsFileTreeCollapsed: true,
            IsPromptBuilderCollapsed: false,
            IsPreviewCollapsed: true);
        await _sut.SaveLayoutStateAsync(state);
        
        // Act
        var loaded = await _sut.LoadLayoutStateAsync();
        
        // Assert
        loaded.Should().Be(state);
    }
    
    [Fact]
    public async Task ResetToDefaultAsync_ClearsPersistedState()
    {
        // Arrange
        var state = new LayoutState(
            FileTreeWidth: 500,
            PromptBuilderHeight: 0.8,
            IsFileTreeCollapsed: false,
            IsPromptBuilderCollapsed: false,
            IsPreviewCollapsed: false);
        await _sut.SaveLayoutStateAsync(state);
        
        // Act
        await _sut.ResetToDefaultAsync();
        
        // Assert
        var loaded = await _sut.LoadLayoutStateAsync();
        loaded.Should().BeNull();
    }
    
    [Fact]
    public async Task SaveLayoutStateAsync_WithMinimumWidth_Persists()
    {
        // Arrange
        var state = new LayoutState(
            FileTreeWidth: 200, // Min width
            PromptBuilderHeight: 0.3,
            IsFileTreeCollapsed: false,
            IsPromptBuilderCollapsed: false,
            IsPreviewCollapsed: false);
        
        // Act
        await _sut.SaveLayoutStateAsync(state);
        var loaded = await _sut.LoadLayoutStateAsync();
        
        // Assert
        loaded!.FileTreeWidth.Should().Be(200);
    }
    
    [Fact]
    public async Task SaveLayoutStateAsync_WithCollapsedState_Persists()
    {
        // Arrange
        var state = new LayoutState(
            FileTreeWidth: 300,
            PromptBuilderHeight: 0.5,
            IsFileTreeCollapsed: true,
            IsPromptBuilderCollapsed: true,
            IsPreviewCollapsed: true);
        
        // Act
        await _sut.SaveLayoutStateAsync(state);
        var loaded = await _sut.LoadLayoutStateAsync();
        
        // Assert
        loaded!.IsFileTreeCollapsed.Should().BeTrue();
        loaded!.IsPromptBuilderCollapsed.Should().BeTrue();
        loaded!.IsPreviewCollapsed.Should().BeTrue();
    }
    
    [Fact]
    public async Task SaveLayoutStateAsync_MultipleTimes_OverwritesPrevious()
    {
        // Arrange
        var state1 = new LayoutState(300, 0.5, false, false, false);
        var state2 = new LayoutState(400, 0.7, true, false, true);
        
        // Act
        await _sut.SaveLayoutStateAsync(state1);
        await _sut.SaveLayoutStateAsync(state2);
        var loaded = await _sut.LoadLayoutStateAsync();
        
        // Assert
        loaded.Should().Be(state2);
    }
    
    [Fact]
    public void LayoutDefaults_ProvidesCorrectDefaultValues()
    {
        // Assert
        LayoutDefaults.FileTreeWidth.Should().Be(300);
        LayoutDefaults.PromptBuilderHeight.Should().Be(0.5);
        LayoutDefaults.IsFileTreeCollapsed.Should().BeFalse();
        LayoutDefaults.Default.FileTreeWidth.Should().Be(300);
    }
}

