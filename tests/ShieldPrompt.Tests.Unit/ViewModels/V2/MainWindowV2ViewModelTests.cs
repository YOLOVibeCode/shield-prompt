using FluentAssertions;
using NSubstitute;
using ShieldPrompt.App.ViewModels.V2;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Application.Interfaces.Actions;
using ShieldPrompt.Domain.Entities;
using ShieldPrompt.Domain.Enums;
using ShieldPrompt.Domain.Records;
using ShieldPrompt.Infrastructure.Interfaces;
using ShieldPrompt.Sanitization.Interfaces;
using Xunit;
using ModelProfile = ShieldPrompt.Domain.Entities.ModelProfile;

namespace ShieldPrompt.Tests.Unit.ViewModels.V2;

/// <summary>
/// TDD tests for MainWindowV2ViewModel.
/// These tests verify that the v2 ViewModel correctly reuses existing business logic.
/// </summary>
public class MainWindowV2ViewModelTests
{
    private readonly ISanitizationEngine _sanitizationEngine;
    private readonly IDesanitizationEngine _desanitizationEngine;
    private readonly ITokenCountingService _tokenService;
    private readonly IPromptComposer _promptComposer;
    private readonly IFileAggregationService _fileService;
    private readonly IClipboardService _clipboardService;
    private readonly IRoleRepository _roleRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IPromptTemplateRepository _templateRepository;
    private readonly IMappingSession _mappingSession;
    private readonly IProgressReporter _progressReporter;
    private readonly IStatusBarService _statusBarService;
    private readonly IStructuredResponseParser _responseParser;
    private readonly IFileApplyService _fileApplyService;
    private readonly IDiffService _diffService;
    private readonly IStatusMessageReporter _statusMessageReporter;
    private readonly StatusBarViewModel _statusBar;
    private readonly ApplyDashboardViewModel _applyDashboard;
    private readonly MainWindowV2ViewModel _sut;

    public MainWindowV2ViewModelTests()
    {
        _sanitizationEngine = Substitute.For<ISanitizationEngine>();
        _desanitizationEngine = Substitute.For<IDesanitizationEngine>();
        _tokenService = Substitute.For<ITokenCountingService>();
        _promptComposer = Substitute.For<IPromptComposer>();
        _fileService = Substitute.For<IFileAggregationService>();
        _clipboardService = Substitute.For<IClipboardService>();
        _roleRepository = Substitute.For<IRoleRepository>();
        _workspaceRepository = Substitute.For<IWorkspaceRepository>();
        _templateRepository = Substitute.For<IPromptTemplateRepository>();
        _mappingSession = Substitute.For<IMappingSession>();
        _progressReporter = Substitute.For<IProgressReporter>();
        _statusBarService = Substitute.For<IStatusBarService>();
        _responseParser = Substitute.For<IStructuredResponseParser>();
        _fileApplyService = Substitute.For<IFileApplyService>();
        _diffService = Substitute.For<IDiffService>();
        _statusMessageReporter = Substitute.For<IStatusMessageReporter>();

        // Create StatusBarViewModel for tests
        _statusBarService.GetSessionInfo().Returns(new SessionSummary(TimeSpan.Zero, DateTime.UtcNow));
        _statusBar = new StatusBarViewModel(_progressReporter, _statusBarService);

        // Create ApplyDashboardViewModel for tests
        var actionFactory = Substitute.For<IFileActionFactory>();
        var undoRedoManager = Substitute.For<IUndoRedoManager>();
        _applyDashboard = new ApplyDashboardViewModel(_responseParser, actionFactory, undoRedoManager, _diffService, _statusMessageReporter);

        // Setup default roles
        var defaultRole = new Role(
            Id: "general_review",
            Name: "General Review",
            Icon: "🔧",
            Description: "General code review",
            SystemPrompt: "You are a helpful assistant.",
            Tone: "professional",
            Style: "detailed",
            Priorities: Array.Empty<string>(),
            Expertise: Array.Empty<string>());
        _roleRepository.GetAllRoles().Returns(new[] { defaultRole });
        _roleRepository.GetDefault().Returns(defaultRole);

        // Setup default template
        var defaultTemplate = new PromptTemplate(
            Id: "general",
            Name: "General Review",
            Icon: "📝",
            Description: "General code review",
            SystemPrompt: "Analyze the code.",
            FocusOptions: Array.Empty<string>(),
            RequiresCustomInput: false);
        _templateRepository.GetAllTemplates().Returns(new[] { defaultTemplate });

        // Setup empty workspaces
        _workspaceRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<Workspace>>(Array.Empty<Workspace>()));

        _sut = new MainWindowV2ViewModel(
            _sanitizationEngine,
            _desanitizationEngine,
            _tokenService,
            _promptComposer,
            _fileService,
            _clipboardService,
            _roleRepository,
            _workspaceRepository,
            _templateRepository,
            _mappingSession,
            _statusBar,
            _applyDashboard);
    }

    [Fact]
    public void Constructor_WithValidServices_InitializesCorrectly()
    {
        // Assert
        _sut.Should().NotBeNull();
        _sut.StatusText.Should().Be("Ready");
        _sut.SanitizationEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task LoadRoles_OnInitialization_LoadsFromRepository()
    {
        // Allow async initialization to complete
        await Task.Delay(100);

        // Assert
        _sut.AvailableRoles.Should().NotBeEmpty();
        _sut.SelectedRole.Should().NotBeNull();
        _sut.SelectedRole!.Id.Should().Be("general_review");
    }

    [Fact]
    public async Task OpenFolder_WithValidPath_LoadsFileTree()
    {
        // Arrange
        var testPath = "/test/path";
        var rootNode = new FileNode(testPath, "path", true);
        var childFile = new FileNode("/test/path/file.cs", "file.cs", false);
        rootNode.AddChild(childFile);
        
        _fileService.LoadDirectoryAsync(testPath, Arg.Any<CancellationToken>())
            .Returns(rootNode);
        _workspaceRepository.GetByPathAsync(testPath, Arg.Any<CancellationToken>())
            .Returns((Workspace?)null);

        // Act
        await _sut.LoadFolderAsync(testPath);

        // Assert
        _sut.FileTree.Should().NotBeEmpty();
        await _fileService.Received(1).LoadDirectoryAsync(testPath, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void SelectedFiles_WhenFilesSelected_UpdatesTokenCount()
    {
        // Arrange
        var file1 = new FileNode("/test/file1.cs", "file1.cs", false);
        var file2 = new FileNode("/test/file2.cs", "file2.cs", false);
        var vm1 = new FileNodeViewModel(file1);
        var vm2 = new FileNodeViewModel(file2);
        vm1.IsChecked = true;
        vm2.IsChecked = true;
        _tokenService.CountTokens(Arg.Any<string>()).Returns(100);

        // Act
        _sut.FileTree.Add(vm1);
        _sut.FileTree.Add(vm2);
        _sut.UpdateTokenCounts();

        // Assert
        _sut.SelectedFilesText.Should().Contain("2");
    }

    [Fact]
    public async Task CopyToClipboard_WithSelectedFiles_UsesSanitizationEngine()
    {
        // Arrange
        await Task.Delay(100); // Wait for initialization

        var file = new FileNode("/test/file.cs", "file.cs", false)
        {
            Content = "var db = ProductionDB;"
        };
        var fileVm = new FileNodeViewModel(file);
        fileVm.IsChecked = true;
        _sut.FileTree.Add(fileVm);
        _sut.SanitizationEnabled = true;

        var sanitizationResult = new SanitizationResult(
            SanitizedContent: "var db = DATABASE_0;",
            WasSanitized: true,
            Matches: new[] { new SanitizationMatch("ProductionDB", "DATABASE_0", PatternCategory.Database, "DatabasePattern", 0, 12) });
        _sanitizationEngine.Sanitize(Arg.Any<string>(), Arg.Any<SanitizationOptions>())
            .Returns(sanitizationResult);

        var composedPrompt = new ComposedPrompt(
            SystemPrompt: "system",
            UserContent: "user",
            FullPrompt: "full prompt",
            EstimatedTokens: 100,
            Warnings: new List<string>());
        _promptComposer.Compose(Arg.Any<PromptTemplate>(), Arg.Any<IEnumerable<FileNode>>(), Arg.Any<PromptOptions>())
            .Returns(composedPrompt);

        // Act
        await _sut.CopyToClipboardCommand.ExecuteAsync(null);

        // Assert
        _sanitizationEngine.Received(1).Sanitize(Arg.Any<string>(), Arg.Any<SanitizationOptions>());
        await _clipboardService.Received(1).SetTextAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task CopyToClipboard_WithSanitizationDisabled_SkipsSanitization()
    {
        // Arrange
        await Task.Delay(100); // Wait for initialization

        var file = new FileNode("/test/file.cs", "file.cs", false)
        {
            Content = "var db = ProductionDB;"
        };
        var fileVm = new FileNodeViewModel(file);
        fileVm.IsChecked = true;
        _sut.FileTree.Add(fileVm);
        _sut.SanitizationEnabled = false;

        var composedPrompt = new ComposedPrompt(
            SystemPrompt: "system",
            UserContent: "user",
            FullPrompt: "composed prompt",
            EstimatedTokens: 100,
            Warnings: new List<string>());
        _promptComposer.Compose(Arg.Any<PromptTemplate>(), Arg.Any<IEnumerable<FileNode>>(), Arg.Any<PromptOptions>())
            .Returns(composedPrompt);

        // Act
        await _sut.CopyToClipboardCommand.ExecuteAsync(null);

        // Assert
        _sanitizationEngine.DidNotReceive().Sanitize(Arg.Any<string>(), Arg.Any<SanitizationOptions>());
        await _clipboardService.Received(1).SetTextAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task LivePreview_WhenFilesSelected_UpdatesInRealTime()
    {
        // Arrange
        await Task.Delay(100); // Wait for initialization

        var file = new FileNode("/test/file.cs", "file.cs", false)
        {
            Content = "public class Test { }"
        };
        var fileVm = new FileNodeViewModel(file);
        fileVm.IsChecked = true;

        var composedPrompt = new ComposedPrompt(
            SystemPrompt: "system",
            UserContent: "user",
            FullPrompt: "# Preview\n```csharp\npublic class Test { }\n```",
            EstimatedTokens: 50,
            Warnings: new List<string>());
        _promptComposer.Compose(Arg.Any<PromptTemplate>(), Arg.Any<IEnumerable<FileNode>>(), Arg.Any<PromptOptions>())
            .Returns(composedPrompt);

        // Act
        _sut.FileTree.Add(fileVm);
        _sut.UpdateLivePreview();

        // Assert
        _sut.LivePreviewContent.Should().NotBeNullOrEmpty();
        _promptComposer.Received().Compose(Arg.Any<PromptTemplate>(), Arg.Any<IEnumerable<FileNode>>(), Arg.Any<PromptOptions>());
    }

    [Fact]
    public void ContextUsagePercent_WithSelectedFiles_CalculatesCorrectly()
    {
        // Arrange
        _sut.SelectedModel = new ModelProfile("gpt-4o", "GPT-4o", 128_000, "cl100k_base", 0.25);
        
        var file = new FileNode("/test/file.cs", "file.cs", false) 
        { 
            Content = "test content"
        };
        var fileVm = new FileNodeViewModel(file);
        fileVm.IsChecked = true;
        _tokenService.CountTokens(Arg.Any<string>()).Returns(12_800); // 10% of 128k
        _sut.FileTree.Add(fileVm);

        // Act
        _sut.UpdateTokenCounts();

        // Assert
        _sut.ContextUsagePercent.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task PasteResponse_WithClipboardContent_OpensResponseDashboard()
    {
        // Arrange
        var llmResponse = "<code_changes><changed_file><file_path>test.cs</file_path><file_summary>test</file_summary><file_operation>UPDATE</file_operation><file_code><![CDATA[code]]></file_code></changed_file></code_changes>";
        _clipboardService.GetTextAsync().Returns(llmResponse);

        // Act
        await _sut.PasteResponseCommand.ExecuteAsync(null);

        // Assert
        await _clipboardService.Received(1).GetTextAsync();
        _sut.ResponseDashboardVisible.Should().BeTrue();
    }

    [Fact]
    public void SanitizedCountText_WhenSanitizationPerformed_ShowsCount()
    {
        // Arrange
        var sanitizationResult = new SanitizationResult(
            SanitizedContent: "sanitized",
            WasSanitized: true,
            Matches: new[]
            {
                new SanitizationMatch("ProductionDB", "DATABASE_0", PatternCategory.Database, "DatabasePattern", 0, 12),
                new SanitizationMatch("192.168.1.1", "IP_ADDRESS_0", PatternCategory.IPAddress, "IPPattern", 20, 11)
            });

        // Act
        _sut.UpdateSanitizationStatus(sanitizationResult);

        // Assert
        _sut.HasSanitizedValues.Should().BeTrue();
        _sut.SanitizedCountText.Should().Contain("2");
    }

    [Fact]
    public async Task Refresh_WithCurrentWorkspace_ReloadsFileTree()
    {
        // Arrange
        var workspace = new Workspace
        {
            Id = "test-ws",
            Name = "Test",
            RootPath = "/test/path"
        };
        _sut.CurrentWorkspace = workspace;

        var rootNode = new FileNode("/test/path", "path", true);
        _fileService.LoadDirectoryAsync(workspace.RootPath, Arg.Any<CancellationToken>())
            .Returns(rootNode);
        _workspaceRepository.GetByPathAsync(workspace.RootPath, Arg.Any<CancellationToken>())
            .Returns(workspace);

        // Act
        await _sut.RefreshCommand.ExecuteAsync(null);

        // Assert
        await _fileService.Received(1).LoadDirectoryAsync(workspace.RootPath, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void FileSearchQuery_WhenSet_FiltersFileTree()
    {
        // Arrange
        var file1 = new FileNode("/test/App.cs", "App.cs", false);
        var file2 = new FileNode("/test/User.cs", "User.cs", false);
        var file3 = new FileNode("/test/README.md", "README.md", false);
        _sut.FileTree.Add(new FileNodeViewModel(file1));
        _sut.FileTree.Add(new FileNodeViewModel(file2));
        _sut.FileTree.Add(new FileNodeViewModel(file3));

        // Act
        _sut.FileSearchQuery = ".cs";

        // Assert
        _sut.FilteredFileTree.Should().HaveCount(2);
        _sut.FilteredFileTree.Should().Contain(f => f.Name == "App.cs");
        _sut.FilteredFileTree.Should().Contain(f => f.Name == "User.cs");
    }
}
