using FluentAssertions;
using NSubstitute;
using ShieldPrompt.App.ViewModels;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Domain.Entities;
using ShieldPrompt.Domain.Records;
using ShieldPrompt.Infrastructure.Interfaces;
using ShieldPrompt.Sanitization.Interfaces;
using TextCopy;

namespace ShieldPrompt.Tests.Unit.ViewModels;

/// <summary>
/// Tests for click-to-copy functionality in MainWindowViewModel.
/// </summary>
public class MainWindowViewModelCopyTests
{
    private readonly IFileAggregationService _fileService;
    private readonly ITokenCountingService _tokenService;
    private readonly ISanitizationEngine _sanitizationEngine;
    private readonly IDesanitizationEngine _desanitizationEngine;
    private readonly IMappingSession _session;
    private readonly ISettingsRepository _settingsRepository;
    private readonly IUndoRedoManager _undoRedoManager;
    private readonly IAIResponseParser _aiParser;
    private readonly IFileWriterService _fileWriter;
    private readonly IPromptTemplateRepository _templateRepository;
    private readonly IPromptComposer _promptComposer;
    private readonly ILayoutStateRepository _layoutRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ICustomRoleRepository _customRoleRepository;
    private readonly IFormatMetadataRepository _formatMetadataRepository;
    private readonly RoleEditorViewModel _roleEditorViewModel;
    private readonly MainWindowViewModel _sut;

    public MainWindowViewModelCopyTests()
    {
        _fileService = Substitute.For<IFileAggregationService>();
        _tokenService = Substitute.For<ITokenCountingService>();
        _sanitizationEngine = Substitute.For<ISanitizationEngine>();
        _desanitizationEngine = Substitute.For<IDesanitizationEngine>();
        _session = Substitute.For<IMappingSession>();
        _settingsRepository = Substitute.For<ISettingsRepository>();
        _undoRedoManager = Substitute.For<IUndoRedoManager>();
        _aiParser = Substitute.For<IAIResponseParser>();
        _fileWriter = Substitute.For<IFileWriterService>();
        _templateRepository = Substitute.For<IPromptTemplateRepository>();
        _promptComposer = Substitute.For<IPromptComposer>();
        _layoutRepository = Substitute.For<ILayoutStateRepository>();
        _roleRepository = Substitute.For<IRoleRepository>();
        _customRoleRepository = Substitute.For<ICustomRoleRepository>();
        _formatMetadataRepository = Substitute.For<IFormatMetadataRepository>();

        // Setup default role
        var defaultRole = new Role(
            Id: "general_review",
            Name: "General Code Review",
            Icon: "🔍",
            Description: "Comprehensive code review",
            SystemPrompt: "You are a code reviewer.",
            Tone: "Professional",
            Style: "Detailed",
            Priorities: new[] { "Quality" },
            Expertise: new[] { "Code Quality" }
        ) { IsBuiltIn = true };
        _roleRepository.GetDefault().Returns(defaultRole);
        _roleRepository.GetAllRoles().Returns(new[] { defaultRole });
        _customRoleRepository.GetCustomRoles().Returns(Array.Empty<Role>());

        // Create real RoleEditorViewModel with mocked dependencies
        _roleEditorViewModel = new RoleEditorViewModel(_roleRepository, _customRoleRepository);

        _settingsRepository.LoadAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(new AppSettings()));
        _layoutRepository.LoadLayoutStateAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<LayoutState?>(null));

        _sut = new MainWindowViewModel(
            _fileService,
            _tokenService,
            _sanitizationEngine,
            _desanitizationEngine,
            _session,
            _settingsRepository,
            _undoRedoManager,
            _aiParser,
            _fileWriter,
            _templateRepository,
            _promptComposer,
            _layoutRepository,
            _roleRepository,
            _customRoleRepository,
            _formatMetadataRepository,
            _roleEditorViewModel,
            Substitute.For<OutputFormatSettingsViewModel>(
                Substitute.For<IOutputFormatSettingsRepository>()),
            Substitute.For<LlmResponseViewModel>(
                Substitute.For<IStructuredResponseParser>(),
                Substitute.For<IFileWriterService>(),
                Substitute.For<IUndoRedoManager>(),
                Substitute.For<ShieldPrompt.Infrastructure.Interfaces.IClipboardService>()));
    }

    [Fact]
    public async Task CopyLivePreview_WithContent_CopiesEntirePreviewToClipboard()
    {
        // Arrange
        var testContent = "This is the live preview content\nWith multiple lines\nFor testing";
        _sut.GetType().GetProperty("LivePreview")!.SetValue(_sut, testContent);

        // Act
        await _sut.CopyLivePreviewCommand.ExecuteAsync(null);

        // Assert
        var clipboardContent = await ClipboardService.GetTextAsync();
        clipboardContent.Should().Be(testContent);
    }

    [Fact]
    public async Task CopyLivePreview_WhenCalled_UpdatesStatusMessage()
    {
        // Arrange
        _sut.GetType().GetProperty("LivePreview")!.SetValue(_sut, "Test content");

        // Act
        await _sut.CopyLivePreviewCommand.ExecuteAsync(null);

        // Assert
        _sut.StatusText.Should().Contain("Copied");
        _sut.StatusText.Should().Contain("clipboard");
    }

    [Fact]
    public async Task CopyLivePreview_WhenCalled_SetsIsCopyFlashActive()
    {
        // Arrange
        _sut.GetType().GetProperty("LivePreview")!.SetValue(_sut, "Test content");

        // Act
        await _sut.CopyLivePreviewCommand.ExecuteAsync(null);

        // Assert
        _sut.IsCopyFlashActive.Should().BeTrue();
    }

    [Fact]
    public async Task CopyLivePreview_WithEmptyContent_DoesNotCopy()
    {
        // Arrange
        _sut.GetType().GetProperty("LivePreview")!.SetValue(_sut, string.Empty);

        // Act
        await _sut.CopyLivePreviewCommand.ExecuteAsync(null);

        // Assert
        _sut.StatusText.Should().Contain("No content");
    }

    [Fact]
    public async Task CopyLivePreview_WithLargeContent_CopiesSuccessfully()
    {
        // Arrange
        var largeContent = string.Join("\n", Enumerable.Range(1, 1000).Select(i => $"Line {i}"));
        _sut.GetType().GetProperty("LivePreview")!.SetValue(_sut, largeContent);

        // Act
        await _sut.CopyLivePreviewCommand.ExecuteAsync(null);

        // Assert
        var clipboardContent = await ClipboardService.GetTextAsync();
        clipboardContent.Should().Be(largeContent);
    }

    [Fact]
    public async Task CopyLivePreview_FlashDeactivates_AfterDelay()
    {
        // Arrange
        _sut.GetType().GetProperty("LivePreview")!.SetValue(_sut, "Test content");

        // Act
        await _sut.CopyLivePreviewCommand.ExecuteAsync(null);
        _sut.IsCopyFlashActive.Should().BeTrue();

        await Task.Delay(2500); // Wait for flash to complete (2 seconds)

        // Assert
        _sut.IsCopyFlashActive.Should().BeFalse();
    }
}

