using FluentAssertions;
using ShieldPrompt.Application.Formatters;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Application.Services;
using ShieldPrompt.Domain.Entities;
using ShieldPrompt.Domain.Records;
using ShieldPrompt.Infrastructure.Persistence;
using ShieldPrompt.Sanitization.Interfaces;
using ShieldPrompt.Sanitization.Patterns;
using ShieldPrompt.Sanitization.Services;
using Xunit;
using Xunit.Abstractions;

namespace ShieldPrompt.Tests.Unit.Integration;

/// <summary>
/// End-to-end integration test for the complete ShieldPrompt workflow.
/// Tests: Load → Select → Copy → AI Response → Paste → Apply → Undo
/// </summary>
public class CompleteWorkflowIntegrationTests : IDisposable
{
    private readonly IFileAggregationService _fileAggregation;
    private readonly ITokenCountingService _tokenCounting;
    private readonly ISanitizationEngine _sanitizer;
    private readonly IDesanitizationEngine _desanitizer;
    private readonly IAIResponseParser _aiParser;
    private readonly IFileWriterService _fileWriter;
    private readonly IMappingSession _session;
    private readonly ITestOutputHelper _output;
    private readonly string _testDirectory;

    public CompleteWorkflowIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
        
        // Setup all services (real implementations, no mocks!)
        _fileAggregation = new FileAggregationService();
        _tokenCounting = new TokenCountingService();
        
        var registry = new PatternRegistry();
        foreach (var pattern in BuiltInPatterns.GetAll())
        {
            registry.AddPattern(pattern);
        }
        
        var aliasGenerator = new AliasGenerator();
        _session = new MappingSession();
        _sanitizer = new SanitizationEngine(registry, _session, aliasGenerator);
        _desanitizer = new DesanitizationEngine();
        _aiParser = new AIResponseParser();
        _fileWriter = new FileWriterService();
        
        // Create test directory
        _testDirectory = Path.Combine(Path.GetTempPath(), $"ShieldPrompt-E2E-{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            try
            {
                Directory.Delete(_testDirectory, recursive: true);
            }
            catch
            {
                // Best effort cleanup
            }
        }
    }

    [Fact]
    public async Task CompleteWorkflow_CopyPasteApply_ShouldUpdateFilesWithRestoredValues()
    {
        _output.WriteLine("═══════════════════════════════════════════");
        _output.WriteLine("COMPLETE WORKFLOW - END-TO-END TEST");
        _output.WriteLine("═══════════════════════════════════════════");
        
        // ═══════════════════════════════════════════════════════════
        // STEP 1: Setup - Create source files with secrets
        // ═══════════════════════════════════════════════════════════
        _output.WriteLine("\n📁 STEP 1: Creating source files with secrets...");
        
        var programPath = Path.Combine(_testDirectory, "Program.cs");
        var databasePath = Path.Combine(_testDirectory, "Database.cs");
        
        var programContent = @"
public class Program
{
    private string _db = ""ProductionDB"";
    private string _ip = ""192.168.1.50"";
    
    public void Connect()
    {
        Console.WriteLine($""Connecting to {_db} at {_ip}"");
    }
}";
        
        var databaseContent = @"
public class Database
{
    private string _connectionString = ""Server=ProductionDB;Password=Secret123"";
}";
        
        File.WriteAllText(programPath, programContent);
        File.WriteAllText(databasePath, databaseContent);
        
        _output.WriteLine($"  ✅ Created Program.cs with secrets");
        _output.WriteLine($"  ✅ Created Database.cs with connection string");
        
        // ═══════════════════════════════════════════════════════════
        // STEP 2: Load files (simulates user opening folder)
        // ═══════════════════════════════════════════════════════════
        _output.WriteLine("\n📂 STEP 2: Loading files...");
        
        var rootNode = await _fileAggregation.LoadDirectoryAsync(_testDirectory);
        var files = GetAllFiles(rootNode).Where(f => !f.IsDirectory).ToList();
        
        _output.WriteLine($"  ✅ Loaded {files.Count} files");
        
        // ═══════════════════════════════════════════════════════════
        // STEP 3: Aggregate and sanitize (simulates copy)
        // ═══════════════════════════════════════════════════════════
        _output.WriteLine("\n📋 STEP 3: Copy with sanitization...");
        
        var aggregated = await _fileAggregation.AggregateContentsAsync(files);
        var formatter = new MarkdownFormatter();
        var formatted = formatter.Format(files, aggregated);
        
        var sanitized = _sanitizer.Sanitize(formatted, new SanitizationOptions());
        
        _output.WriteLine($"  ✅ Aggregated content: {aggregated.Length:N0} chars");
        _output.WriteLine($"  ✅ Formatted (Markdown): {formatted.Length:N0} chars");
        _output.WriteLine($"  ✅ Sanitized: {sanitized.Matches.Count} secrets masked");
        _output.WriteLine($"  ✅ ProductionDB → {sanitized.Matches.First(m => m.Original == "ProductionDB").Alias}");
        _output.WriteLine($"  ✅ 192.168.1.50 → {sanitized.Matches.First(m => m.Original == "192.168.1.50").Alias}");
        
        // Verify secrets are masked
        sanitized.SanitizedContent.Should().NotContain("ProductionDB");
        sanitized.SanitizedContent.Should().NotContain("192.168.1.50");
        sanitized.SanitizedContent.Should().NotContain("Secret123");
        
        // ═══════════════════════════════════════════════════════════
        // STEP 4: Simulate AI improving the code (with aliases)
        // ═══════════════════════════════════════════════════════════
        _output.WriteLine("\n🤖 STEP 4: Simulating ChatGPT response...");
        
        var aiResponse = @"
I'll refactor your code to use dependency injection:

**Program.cs**
```csharp
public class Program
{
    private readonly IConfiguration _config;
    
    public Program(IConfiguration config)
    {
        _config = config;
    }
    
    public void Connect()
    {
        var db = _config[""Database""] ?? ""DATABASE_0"";
        var ip = _config[""ServerIP""] ?? ""IP_ADDRESS_0"";
        Console.WriteLine($""Connecting to {db} at {ip}"");
    }
}
```

**Database.cs**
```csharp
public class Database
{
    private readonly string _connectionString;
    
    public Database(IConfiguration config)
    {
        _connectionString = config.GetConnectionString(""Default"") ?? ""Server=DATABASE_0;Password=PASSWORD_0"";
    }
}
```

This approach uses configuration instead of hardcoded values.
";
        
        _output.WriteLine("  ✅ AI returned improved code with DATABASE_0, IP_ADDRESS_0 aliases");
        
        // ═══════════════════════════════════════════════════════════
        // STEP 5: Desanitize AI response (simulates paste & restore)
        // ═══════════════════════════════════════════════════════════
        _output.WriteLine("\n🔓 STEP 5: Desanitizing AI response...");
        
        var desanitized = _desanitizer.Desanitize(aiResponse, _session);
        
        _output.WriteLine("  ✅ Desanitized content");
        _output.WriteLine($"  ✅ DATABASE_0 → ProductionDB");
        _output.WriteLine($"  ✅ IP_ADDRESS_0 → 192.168.1.50");
        
        // Verify restoration
        desanitized.Should().Contain("ProductionDB");
        desanitized.Should().Contain("192.168.1.50");
        desanitized.Should().Contain("Secret123");
        desanitized.Should().NotContain("DATABASE_0");
        desanitized.Should().NotContain("IP_ADDRESS_0");
        desanitized.Should().NotContain("PASSWORD_0");
        
        // ═══════════════════════════════════════════════════════════
        // STEP 6: Parse AI response to extract file operations
        // ═══════════════════════════════════════════════════════════
        _output.WriteLine("\n📄 STEP 6: Parsing AI response for files...");
        
        var parsed = _aiParser.Parse(desanitized, files);
        
        _output.WriteLine($"  ✅ Detected {parsed.Updates.Count} file operations");
        foreach (var update in parsed.Updates)
        {
            _output.WriteLine($"    - {update.Type}: {update.FilePath} ({update.EstimatedLinesChanged} lines)");
        }
        
        parsed.Updates.Should().HaveCount(2, "AI returned 2 files");
        parsed.Updates.Should().Contain(u => u.FilePath.Contains("Program.cs"));
        parsed.Updates.Should().Contain(u => u.FilePath.Contains("Database.cs"));
        
        // ═══════════════════════════════════════════════════════════
        // STEP 7: Apply to files (THE MAGIC MOMENT!)
        // ═══════════════════════════════════════════════════════════
        _output.WriteLine("\n💾 STEP 7: Applying AI changes to files...");
        
        var result = await _fileWriter.ApplyUpdatesAsync(
            parsed.Updates,
            _testDirectory,
            new FileWriteOptions { CreateBackup = true });
        
        _output.WriteLine($"  ✅ Files updated: {result.FilesUpdated}");
        _output.WriteLine($"  ✅ Files created: {result.FilesCreated}");
        _output.WriteLine($"  ✅ Backup ID: {result.BackupId}");
        _output.WriteLine($"  ✅ Errors: {result.Errors.Count}");
        
        result.FilesUpdated.Should().BeGreaterThan(0, "files should be updated");
        result.Errors.Should().BeEmpty("no errors should occur");
        result.BackupId.Should().NotBeNullOrEmpty("backup should be created");
        
        // ═══════════════════════════════════════════════════════════
        // STEP 8: Verify files have AI improvements AND real values
        // ═══════════════════════════════════════════════════════════
        _output.WriteLine("\n✅ STEP 8: Verifying updated files...");
        
        var updatedProgramContent = File.ReadAllText(programPath);
        var updatedDatabaseContent = File.ReadAllText(databasePath);
        
        _output.WriteLine("\n  Program.cs now contains:");
        _output.WriteLine("  " + string.Join("\n  ", updatedProgramContent.Split('\n').Take(10)));
        
        // Verify AI improvements are present
        updatedProgramContent.Should().Contain("IConfiguration", "AI added dependency injection");
        updatedDatabaseContent.Should().Contain("IConfiguration", "AI added configuration");
        
        // Verify REAL values are restored (not aliases!)
        updatedProgramContent.Should().Contain("ProductionDB", "real database name restored");
        updatedProgramContent.Should().Contain("192.168.1.50", "real IP restored");
        updatedDatabaseContent.Should().Contain("ProductionDB", "real database in connection string");
        updatedDatabaseContent.Should().Contain("Secret123", "real password restored");
        
        // Verify aliases are GONE
        updatedProgramContent.Should().NotContain("DATABASE_0", "aliases should be replaced");
        updatedProgramContent.Should().NotContain("IP_ADDRESS_0", "aliases should be replaced");
        
        _output.WriteLine("\n  ✅ AI improvements applied");
        _output.WriteLine("  ✅ Real values restored");
        _output.WriteLine("  ✅ Aliases removed");
        
        // ═══════════════════════════════════════════════════════════
        // STEP 9: Test Undo (restore from backup)
        // ═══════════════════════════════════════════════════════════
        _output.WriteLine("\n↶ STEP 9: Testing undo (restore from backup)...");
        
        await _fileWriter.RestoreBackupAsync(result.BackupId);
        
        var restoredProgramContent = File.ReadAllText(programPath);
        var restoredDatabaseContent = File.ReadAllText(databasePath);
        
        // Verify back to original (content-wise, whitespace may vary)
        restoredProgramContent.Should().Contain("private string _db = \"ProductionDB\"");
        restoredProgramContent.Should().Contain("private string _ip = \"192.168.1.50\"");
        restoredProgramContent.Should().NotContain("IConfiguration", "should restore original without DI");
        
        restoredDatabaseContent.Should().Contain("Server=ProductionDB;Password=Secret123");
        restoredDatabaseContent.Should().NotContain("IConfiguration", "should restore original");
        
        _output.WriteLine("  ✅ Files restored to original state");
        _output.WriteLine("  ✅ Undo works perfectly!");
        
        // ═══════════════════════════════════════════════════════════
        // SUCCESS!
        // ═══════════════════════════════════════════════════════════
        _output.WriteLine("\n═══════════════════════════════════════════");
        _output.WriteLine("🎊 COMPLETE WORKFLOW TEST - SUCCESS! 🎊");
        _output.WriteLine("═══════════════════════════════════════════");
        _output.WriteLine("✅ Load files");
        _output.WriteLine("✅ Sanitize (copy)");
        _output.WriteLine("✅ AI processes (with aliases)");
        _output.WriteLine("✅ Desanitize (paste)");
        _output.WriteLine("✅ Apply to files (auto-update)");
        _output.WriteLine("✅ Undo (restore from backup)");
        _output.WriteLine("\nThe complete automation loop works perfectly!");
    }

    [Fact]
    public async Task CompleteWorkflow_WithNewFileCreation_ShouldWork()
    {
        _output.WriteLine("\n🎯 Testing workflow with NEW file creation...\n");
        
        // Step 1: Create initial file
        var originalPath = Path.Combine(_testDirectory, "Original.cs");
        File.WriteAllText(originalPath, "public class Original { }");
        
        // Step 2: Load and sanitize
        var rootNode = await _fileAggregation.LoadDirectoryAsync(_testDirectory);
        var files = GetAllFiles(rootNode).Where(f => !f.IsDirectory).ToList();
        var content = await _fileAggregation.AggregateContentsAsync(files);
        
        var sanitized = _sanitizer.Sanitize(content, new SanitizationOptions());
        
        // Step 3: Simulate AI suggesting a NEW file
        var aiResponse = @"
I suggest adding a configuration file:

**Config.cs** (new file)
```csharp
public class Config
{
    public string Database => ""DATABASE_0"";
}
```
";
        
        // Step 4: Desanitize
        var desanitized = _desanitizer.Desanitize(aiResponse, _session);
        
        // Step 5: Parse (should detect new file)
        var parsed = _aiParser.Parse(desanitized, files);
        
        parsed.Updates.Should().NotBeEmpty("should extract file from AI response");
        var configUpdate = parsed.Updates.FirstOrDefault(u => u.FilePath.Contains("Config.cs"));
        configUpdate.Should().NotBeNull("should find Config.cs in response");
        configUpdate!.Type.Should().Be(FileUpdateType.Create, "should detect new file");
        
        // Step 6: Apply
        var result = await _fileWriter.ApplyUpdatesAsync(
            parsed.Updates,
            _testDirectory,
            new FileWriteOptions());
        
        result.FilesCreated.Should().Be(1);
        
        // Verify new file created
        var configPath = Path.Combine(_testDirectory, "Config.cs");
        File.Exists(configPath).Should().BeTrue();
        var configContent = File.ReadAllText(configPath);
        configContent.Should().Contain("public class Config");
        
        // Note: If there were secrets in the AI response, they would be restored here
        // In this case, the AI suggested DATABASE_0 as a placeholder, not a secret alias
        
        _output.WriteLine("✅ New file creation workflow works!");
    }

    [Fact]
    public async Task CompleteWorkflow_MultipleFilesWithBackup_ShouldSupportFullUndo()
    {
        _output.WriteLine("\n🔄 Testing complete undo workflow...\n");
        
        // Step 1: Create multiple files
        var file1Path = Path.Combine(_testDirectory, "Service1.cs");
        var file2Path = Path.Combine(_testDirectory, "Service2.cs");
        var file3Path = Path.Combine(_testDirectory, "Service3.cs");
        
        File.WriteAllText(file1Path, "class Service1 { }");
        File.WriteAllText(file2Path, "class Service2 { }");
        File.WriteAllText(file3Path, "class Service3 { }");
        
        var originalContent1 = File.ReadAllText(file1Path);
        var originalContent2 = File.ReadAllText(file2Path);
        var originalContent3 = File.ReadAllText(file3Path);
        
        // Step 2: Simulate updates
        var updates = new[]
        {
            new FileUpdate("Service1.cs", "class Service1 { /* improved */ }", FileUpdateType.Update, 1),
            new FileUpdate("Service2.cs", "class Service2 { /* improved */ }", FileUpdateType.Update, 1),
            new FileUpdate("Service3.cs", "class Service3 { /* improved */ }", FileUpdateType.Update, 1)
        };
        
        // Step 3: Apply
        var result = await _fileWriter.ApplyUpdatesAsync(
            updates,
            _testDirectory,
            new FileWriteOptions { CreateBackup = true });
        
        result.FilesUpdated.Should().Be(3);
        
        // Verify changes applied
        File.ReadAllText(file1Path).Should().Contain("improved");
        File.ReadAllText(file2Path).Should().Contain("improved");
        File.ReadAllText(file3Path).Should().Contain("improved");
        
        _output.WriteLine("  ✅ All 3 files updated");
        
        // Step 4: Undo (Ctrl+Z simulation)
        await _fileWriter.RestoreBackupAsync(result.BackupId);
        
        // Verify complete restoration
        File.ReadAllText(file1Path).Should().Be(originalContent1);
        File.ReadAllText(file2Path).Should().Be(originalContent2);
        File.ReadAllText(file3Path).Should().Be(originalContent3);
        
        _output.WriteLine("  ✅ Undo restored all 3 files");
        _output.WriteLine("  ✅ Backup/restore works perfectly!");
    }

    private IEnumerable<FileNode> GetAllFiles(FileNode node)
    {
        yield return node;
        foreach (var child in node.Children)
        {
            foreach (var descendant in GetAllFiles(child))
            {
                yield return descendant;
            }
        }
    }
}

