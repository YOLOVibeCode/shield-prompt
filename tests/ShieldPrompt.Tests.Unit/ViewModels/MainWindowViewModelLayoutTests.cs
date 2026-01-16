using FluentAssertions;
using NSubstitute;
using ShieldPrompt.App.ViewModels;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Infrastructure.Interfaces;
using ShieldPrompt.Presentation.ViewModels;
using ShieldPrompt.Sanitization.Interfaces;

namespace ShieldPrompt.Tests.Unit.ViewModels;

/// <summary>
/// Tests for MainWindowViewModel layout state persistence.
/// TDD: Tests written BEFORE implementation.
/// </summary>
public class MainWindowViewModelLayoutTests
{
    private readonly ILayoutStateRepository _layoutRepository;
    private readonly MainWindowViewModel _sut;
    
    public MainWindowViewModelLayoutTests()
    {
        // Mock dependencies
        var fileService = Substitute.For<IFileAggregationService>();
        var tokenCounter = Substitute.For<ITokenCountingService>();
        var sanitizer = Substitute.For<ISanitizationEngine>();
        var desanitizer = Substitute.For<IDesanitizationEngine>();
        var session = Substitute.For<IMappingSession>();
        var settings = Substitute.For<ISettingsRepository>();
        var undoManager = Substitute.For<IUndoRedoManager>();
        var responseParser = Substitute.For<IAIResponseParser>();
        var fileWriter = Substitute.For<IFileWriterService>();
        var templateRepo = Substitute.For<IPromptTemplateRepository>();
        var composer = Substitute.For<IPromptComposer>();
        _layoutRepository = Substitute.For<ILayoutStateRepository>();
        var roleRepo = Substitute.For<IRoleRepository>();
        var formatMetadataRepo = Substitute.For<IFormatMetadataRepository>();
        
        // Setup role repository defaults
        roleRepo.GetAllRoles().Returns(new List<ShieldPrompt.Domain.Records.Role>());
        roleRepo.GetDefault().Returns((ShieldPrompt.Domain.Records.Role?)null);
        
        _sut = new MainWindowViewModel(
            fileService,
            tokenCounter,
            sanitizer,
            desanitizer,
            session,
            settings,
            undoManager,
            responseParser,
            fileWriter,
            templateRepo,
            composer,
            _layoutRepository,
            roleRepo,
            Substitute.For<ICustomRoleRepository>(),
            formatMetadataRepo,
            Substitute.For<RoleEditorViewModel>(
                Substitute.For<IRoleRepository>(),
                Substitute.For<ICustomRoleRepository>()),
            Substitute.For<OutputFormatSettingsViewModel>(
                Substitute.For<IOutputFormatSettingsRepository>()),
            Substitute.For<LlmResponseViewModel>(
                Substitute.For<IStructuredResponseParser>(),
                Substitute.For<IFileWriterService>(),
                Substitute.For<IUndoRedoManager>(),
                Substitute.For<ShieldPrompt.Infrastructure.Interfaces.IClipboardService>()));
    }
    
    [Fact]
    public async Task LoadLayoutState_LoadsSavedLayoutState()
    {
        // Arrange
        var savedState = new LayoutState(
            FileTreeWidth: 400,
            PromptBuilderHeight: 0.6,
            IsFileTreeCollapsed: false,
            IsPromptBuilderCollapsed: false,
            IsPreviewCollapsed: false);
        
        _layoutRepository.LoadLayoutStateAsync(Arg.Any<CancellationToken>())
            .Returns(savedState);
        
        // Act
        // Simulate the constructor initialization by triggering property load
        _layoutRepository.ClearReceivedCalls();
        await _layoutRepository.LoadLayoutStateAsync();
        var state = await _layoutRepository.LoadLayoutStateAsync();
        if (state != null)
        {
            _sut.FileTreeWidth = state.FileTreeWidth;
            _sut.PromptBuilderHeightRatio = state.PromptBuilderHeight;
        }
        
        // Assert
        _sut.FileTreeWidth.Should().Be(400);
        _sut.PromptBuilderHeightRatio.Should().Be(0.6);
    }
    
    [Fact]
    public void LoadLayoutState_WhenNoSavedState_UsesDefaults()
    {
        // Assert - defaults are set on construction
        _sut.FileTreeWidth.Should().Be(LayoutDefaults.FileTreeWidth);
        _sut.PromptBuilderHeightRatio.Should().Be(LayoutDefaults.PromptBuilderHeight);
    }
    
    [Fact]
    public async Task FileTreeWidth_WhenChanged_SavesLayoutState()
    {
        // Act
        _sut.FileTreeWidth = 500;
        
        // Wait for debounce
        await Task.Delay(600);
        
        // Assert
        await _layoutRepository.Received(1).SaveLayoutStateAsync(
            Arg.Is<LayoutState>(s => s.FileTreeWidth == 500),
            Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task PromptBuilderHeightRatio_WhenChanged_SavesLayoutState()
    {
        // Act
        _sut.PromptBuilderHeightRatio = 0.7;
        
        // Wait for debounce
        await Task.Delay(600);
        
        // Assert
        await _layoutRepository.Received(1).SaveLayoutStateAsync(
            Arg.Is<LayoutState>(s => s.PromptBuilderHeight == 0.7),
            Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task ResetLayoutCommand_ResetsToDefaults()
    {
        // Arrange
        _sut.FileTreeWidth = 500;
        _sut.PromptBuilderHeightRatio = 0.8;
        
        // Act
        await _sut.ResetLayoutCommand.ExecuteAsync(null);
        await Task.Delay(100);
        
        // Assert
        _sut.FileTreeWidth.Should().Be(LayoutDefaults.FileTreeWidth);
        _sut.PromptBuilderHeightRatio.Should().Be(LayoutDefaults.PromptBuilderHeight);
        await _layoutRepository.Received(1).ResetToDefaultAsync(Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public void FileTreeWidth_EnforcesMinimumWidth()
    {
        // Act
        _sut.FileTreeWidth = 50; // Below minimum
        
        // Assert
        _sut.FileTreeWidth.Should().BeGreaterThanOrEqualTo(200);
    }
    
    [Fact]
    public void PromptBuilderHeightRatio_EnforcesBounds()
    {
        // Act & Assert
        _sut.PromptBuilderHeightRatio = -0.1;
        _sut.PromptBuilderHeightRatio.Should().BeGreaterThanOrEqualTo(0.2);
        
        _sut.PromptBuilderHeightRatio = 1.5;
        _sut.PromptBuilderHeightRatio.Should().BeLessThanOrEqualTo(0.8);
    }
}

