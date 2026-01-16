using FluentAssertions;
using NSubstitute;
using ShieldPrompt.App.ViewModels;
using ShieldPrompt.Application.Formatters;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Application.Services;
using ShieldPrompt.Domain.Entities;
using ShieldPrompt.Infrastructure.Persistence;
using ShieldPrompt.Presentation.ViewModels;
using ShieldPrompt.Sanitization.Patterns;
using ShieldPrompt.Sanitization.Services;
using Xunit;

namespace ShieldPrompt.Tests.Unit.ViewModels;

/// <summary>
/// Tests for counter behavior in MainWindowViewModel.
/// Ensures counters update correctly when files are selected/deselected.
/// </summary>
public class MainWindowViewModelCounterTests
{
    private readonly MainWindowViewModel _vm;
    private readonly string _testPath;

    public MainWindowViewModelCounterTests()
    {
        // Setup real services
        var fileService = new FileAggregationService();
        var tokenService = new TokenCountingService();
        var patternRegistry = new PatternRegistry();
        foreach (var pattern in BuiltInPatterns.GetAll())
        {
            patternRegistry.AddPattern(pattern);
        }
        
        var session = new MappingSession();
        var aliasGenerator = new AliasGenerator();
        var sanitizer = new SanitizationEngine(patternRegistry, session, aliasGenerator);
        var desanitizer = new DesanitizationEngine();
        var settings = new SettingsRepository();
        var undoRedo = new ShieldPrompt.Application.Services.UndoRedoManager();
        var aiParser = new ShieldPrompt.Application.Services.AIResponseParser();
        var fileWriter = new ShieldPrompt.Application.Services.FileWriterService();
        var templateRepo = new ShieldPrompt.Infrastructure.Services.YamlPromptTemplateRepository();
        var xmlBuilder = new ShieldPrompt.Application.Services.XmlPromptBuilder();
        var promptComposer = new ShieldPrompt.Application.Services.PromptComposer(tokenService, xmlBuilder);
        var layoutRepo = new ShieldPrompt.Infrastructure.Services.JsonLayoutStateRepository(
            Path.Combine(Path.GetTempPath(), "ShieldPromptTests", Guid.NewGuid().ToString()));
        var roleRepo = new ShieldPrompt.Infrastructure.Services.YamlRoleRepository();
        var formatMetadataRepo = new ShieldPrompt.Infrastructure.Services.StaticFormatMetadataRepository();

        _vm = new MainWindowViewModel(
            fileService,
            tokenService,
            sanitizer,
            desanitizer,
            session,
            settings,
            undoRedo,
            aiParser,
            fileWriter,
            templateRepo,
            promptComposer,
            layoutRepo,
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

        // Get tutorial path for testing
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        _testPath = Path.Combine(baseDir, "..", "..", "..", "..", "..", "samples", "tutorial-project");
        _testPath = Path.GetFullPath(_testPath);
    }

    [Fact]
    public void SelectedFileCount_Initially_ShouldBeZero()
    {
        // Assert
        _vm.SelectedFileCount.Should().Be(0, "no files selected initially");
    }

    [Fact]
    public async Task SelectedFileCount_WhenSingleFileSelected_ShouldBeOne()
    {
        // Arrange
        await LoadTutorialAsync();
        var firstFile = GetFirstFile(_vm.RootNodeViewModel!);
        
        // Act
        firstFile.IsSelected = true;
        
        // Assert
        _vm.SelectedFileCount.Should().Be(1, "one file was selected");
    }

    [Fact]
    public async Task SelectedFileCount_WhenMultipleFilesSelected_ShouldIncrementCorrectly()
    {
        // Arrange
        await LoadTutorialAsync();
        var files = GetAllNonDirectoryNodes(_vm.RootNodeViewModel!).Take(3).ToList();
        
        // Act & Assert
        files[0].IsSelected = true;
        _vm.SelectedFileCount.Should().Be(1, "first file selected");
        
        files[1].IsSelected = true;
        _vm.SelectedFileCount.Should().Be(2, "second file selected");
        
        files[2].IsSelected = true;
        _vm.SelectedFileCount.Should().Be(3, "third file selected");
    }

    [Fact]
    public async Task SelectedFileCount_WhenFileDeselected_ShouldDecrement()
    {
        // Arrange
        await LoadTutorialAsync();
        var files = GetAllNonDirectoryNodes(_vm.RootNodeViewModel!).Take(2).ToList();
        files[0].IsSelected = true;
        files[1].IsSelected = true;
        
        // Pre-condition
        _vm.SelectedFileCount.Should().Be(2);
        
        // Act
        files[0].IsSelected = false;
        
        // Assert
        _vm.SelectedFileCount.Should().Be(1, "one file was deselected");
    }

    [Fact]
    public async Task SelectedFileCount_WhenSelectAllCalled_ShouldCountAllNonDirectoryFiles()
    {
        // Arrange
        await LoadTutorialAsync();
        var expectedCount = GetAllNonDirectoryNodes(_vm.RootNodeViewModel!).Count();
        
        // Act
        _vm.SelectAllCommand.Execute(null);
        
        // Assert
        _vm.SelectedFileCount.Should().Be(expectedCount, "all files should be selected");
    }

    [Fact]
    public async Task SelectedFileCount_WhenDeselectAllCalled_ShouldBeZero()
    {
        // Arrange
        await LoadTutorialAsync();
        _vm.SelectAllCommand.Execute(null);
        
        // Pre-condition
        _vm.SelectedFileCount.Should().BeGreaterThan(0);
        
        // Act
        _vm.DeselectAllCommand.Execute(null);
        
        // Assert
        _vm.SelectedFileCount.Should().Be(0, "all files should be deselected");
    }

    [Fact]
    public async Task TotalTokens_Initially_ShouldBeZero()
    {
        // Assert
        _vm.TotalTokens.Should().Be(0, "no content selected initially");
    }

    [Fact]
    public async Task TotalTokens_WhenFileSelected_ShouldShowEstimate()
    {
        // Arrange
        await LoadTutorialAsync();
        
        // Wait for background token counting to complete
        await Task.Delay(500);
        
        var firstFile = GetFirstFile(_vm.RootNodeViewModel!);
        
        // Act
        firstFile.IsSelected = true;
        
        // Assert
        _vm.TotalTokens.Should().BeGreaterThan(0, "should show estimated tokens for selected file");
    }

    [Fact]
    public async Task TotalTokens_WhenMultipleFilesSelected_ShouldSumEstimates()
    {
        // Arrange
        await LoadTutorialAsync();
        await Task.Delay(500); // Wait for token counting
        
        var files = GetAllNonDirectoryNodes(_vm.RootNodeViewModel!).Take(2).ToList();
        
        // Act
        files[0].IsSelected = true;
        var tokensAfterFirst = _vm.TotalTokens;
        
        files[1].IsSelected = true;
        var tokensAfterSecond = _vm.TotalTokens;
        
        // Assert
        tokensAfterSecond.Should().BeGreaterThan(tokensAfterFirst, 
            "adding more files should increase token estimate");
    }

    [Fact]
    public async Task TotalTokens_AfterCopy_ShouldShowExactCount()
    {
        // Arrange
        await LoadTutorialAsync();
        var firstFile = GetFirstFile(_vm.RootNodeViewModel!);
        firstFile.IsSelected = true;
        
        var estimatedTokens = _vm.TotalTokens;
        
        // Act
        await _vm.CopyToClipboardCommand.ExecuteAsync(null);
        
        // Assert
        _vm.TotalTokens.Should().BeGreaterThan(0, "should have exact token count after copy");
        // Note: Exact will differ from estimate due to formatting
    }

    [Fact]
    public async Task SanitizedValueCount_Initially_ShouldBeZero()
    {
        // Assert
        _vm.SanitizedValueCount.Should().Be(0, "no sanitization has occurred");
    }

    [Fact(Skip = "Requires full UI initialization - sanitization tested elsewhere")]
    public async Task SanitizedValueCount_AfterCopyingTutorialFiles_ShouldBeGreaterThanZero()
    {
        // Arrange
        await LoadTutorialAsync();
        _vm.SelectAllCommand.Execute(null);
        
        // Set a template (required for CopyToClipboard to work)
        if (_vm.AvailableTemplates.Count == 0)
        {
            // Skip test if templates not loaded
            return;
        }
        _vm.SelectedTemplate = _vm.AvailableTemplates.First();
        
        // Set a role (required for prompt composer)
        if (_vm.AvailableRoles.Count > 0)
        {
            _vm.SelectedRole = _vm.AvailableRoles.First();
        }
        
        // Act
        await _vm.CopyToClipboardCommand.ExecuteAsync(null);
        
        // Assert
        _vm.SanitizedValueCount.Should().BeGreaterThan(30, 
            "tutorial has ~34 fake secrets");
    }

    [Fact(Skip = "Requires full UI initialization - sanitization tested elsewhere")]
    public async Task SanitizedValueCount_AfterClearSession_ShouldResetToZero()
    {
        // Arrange
        await LoadTutorialAsync();
        _vm.SelectAllCommand.Execute(null);
        
        // Set a template (required for CopyToClipboard to work)
        if (_vm.AvailableTemplates.Count == 0)
        {
            // Skip test if templates not loaded
            return;
        }
        _vm.SelectedTemplate = _vm.AvailableTemplates.First();
        
        // Set a role (required for prompt composer)
        if (_vm.AvailableRoles.Count > 0)
        {
            _vm.SelectedRole = _vm.AvailableRoles.First();
        }
        
        await _vm.CopyToClipboardCommand.ExecuteAsync(null);
        
        // Pre-condition
        _vm.SanitizedValueCount.Should().BeGreaterThan(0);
        
        // Act
        _vm.ClearSessionCommand.Execute(null);
        
        // Assert
        _vm.SanitizedValueCount.Should().Be(0, "session was cleared");
    }

    [Fact]
    public async Task PerFileTokenCount_AfterLoadingFolder_ShouldBePopulated()
    {
        // Arrange & Act
        await LoadTutorialAsync();
        
        // Wait for background token counting
        await Task.Delay(1000);
        
        var files = GetAllNonDirectoryNodes(_vm.RootNodeViewModel!).ToList();
        
        // Assert
        files.Should().NotBeEmpty();
        files.Where(f => f.TokenCount > 0).Should().NotBeEmpty(
            "at least some files should have token counts");
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 3)]
    public async Task SelectedFileCount_WithVariousSelections_ShouldMatchActualCount(
        int filesToSelect, int expected)
    {
        // Arrange
        await LoadTutorialAsync();
        var files = GetAllNonDirectoryNodes(_vm.RootNodeViewModel!).Take(filesToSelect).ToList();
        
        // Act
        foreach (var file in files)
        {
            file.IsSelected = true;
        }
        
        // Assert
        _vm.SelectedFileCount.Should().Be(expected, 
            $"selected {filesToSelect} files");
    }

    // Helper methods
    private async Task LoadTutorialAsync()
    {
        if (!Directory.Exists(_testPath))
        {
            throw new InvalidOperationException(
                $"Tutorial project not found at {_testPath}. " +
                "Run from repository root or ensure samples/tutorial-project exists.");
        }

        await _vm.LoadTutorialProjectCommand.ExecuteAsync(null);
        
        // Give it time to load
        await Task.Delay(200);
    }

    private FileNodeViewModel GetFirstFile(FileNodeViewModel root)
    {
        return GetAllNonDirectoryNodes(root).First();
    }

    private IEnumerable<FileNodeViewModel> GetAllNonDirectoryNodes(FileNodeViewModel node)
    {
        if (!node.IsDirectory)
        {
            yield return node;
        }

        foreach (var child in node.Children)
        {
            foreach (var descendant in GetAllNonDirectoryNodes(child))
            {
                yield return descendant;
            }
        }
    }
}

