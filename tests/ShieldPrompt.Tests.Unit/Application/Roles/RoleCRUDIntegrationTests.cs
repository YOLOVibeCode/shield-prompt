using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using ShieldPrompt.Domain.Records;
using ShieldPrompt.Infrastructure.Services;
using Xunit;

namespace ShieldPrompt.Tests.Unit.Application.Roles;

/// <summary>
/// Integration tests for Role CRUD operations end-to-end.
/// Tests the full workflow: Create → Read → Update → Delete → Merge with built-in roles.
/// TDD: Tests written BEFORE implementation (Phase 3.2), implementation in Phase 3.3.
/// </summary>
public class RoleCRUDIntegrationTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly YamlCustomRoleRepository _customRepo;
    private readonly YamlRoleRepository _builtInRepo;
    
    public RoleCRUDIntegrationTests()
    {
        // Create temporary directory for test
        _testDirectory = Path.Combine(Path.GetTempPath(), $"shieldprompt-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        
        _customRepo = new YamlCustomRoleRepository(_testDirectory);
        _builtInRepo = new YamlRoleRepository();
    }
    
    public void Dispose()
    {
        // Clean up test directory
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }
    
    [Fact]
    public void FullCRUDCycle_CreateReadUpdateDelete_WorksCorrectly()
    {
        // Arrange: Create a custom role
        var role = new Role(
            Id: "test-role",
            Name: "Test Engineer",
            Icon: "🧪",
            Description: "Specialized in testing",
            SystemPrompt: "You are a test engineer focused on quality.",
            Tone: "Analytical",
            Style: "Precise and thorough",
            Priorities: new[] { "Quality", "Coverage" },
            Expertise: new[] { "Testing", "QA" });
        
        // Act 1: Create
        _customRepo.AddRole(role);
        
        // Assert 1: Read (verify created)
        var retrieved = _customRepo.GetCustomRoles();
        retrieved.Should().ContainSingle();
        retrieved.First().Id.Should().Be("test-role");
        retrieved.First().Name.Should().Be("Test Engineer");
        retrieved.First().IsBuiltIn.Should().BeFalse();
        
        // Act 2: Update
        var updatedRole = role with { Name = "Updated Test Engineer", Icon = "🔬" };
        _customRepo.UpdateRole(updatedRole);
        
        // Assert 2: Verify update
        var afterUpdate = _customRepo.GetCustomRoles();
        afterUpdate.Should().ContainSingle();
        afterUpdate.First().Name.Should().Be("Updated Test Engineer");
        afterUpdate.First().Icon.Should().Be("🔬");
        
        // Act 3: Delete
        _customRepo.DeleteRole("test-role");
        
        // Assert 3: Verify deletion
        var afterDelete = _customRepo.GetCustomRoles();
        afterDelete.Should().BeEmpty();
    }
    
    [Fact]
    public void MergedRoles_BuiltInAndCustom_ContainsBothSets()
    {
        // Arrange: Add custom roles
        _customRepo.AddRole(new Role(
            Id: "custom-1",
            Name: "Custom Role 1",
            Icon: "🎨",
            Description: "First custom role",
            SystemPrompt: "Custom prompt 1",
            Tone: "Creative",
            Style: "Innovative",
            Priorities: new[] { "Creativity" },
            Expertise: new[] { "Design" }));
        
        _customRepo.AddRole(new Role(
            Id: "custom-2",
            Name: "Custom Role 2",
            Icon: "🎭",
            Description: "Second custom role",
            SystemPrompt: "Custom prompt 2",
            Tone: "Theatrical",
            Style: "Dramatic",
            Priorities: new[] { "Expression" },
            Expertise: new[] { "Performance" }));
        
        // Act: Merge built-in and custom roles
        var builtInRoles = _builtInRepo.GetAllRoles();
        var customRoles = _customRepo.GetCustomRoles();
        var allRoles = builtInRoles.Concat(customRoles).ToList();
        
        // Assert
        allRoles.Should().HaveCountGreaterThan(10, "should have at least 12 built-in + 2 custom");
        allRoles.Count(r => r.IsBuiltIn).Should().BeGreaterThan(10);
        allRoles.Count(r => !r.IsBuiltIn).Should().Be(2);
        allRoles.Should().Contain(r => r.Id == "custom-1");
        allRoles.Should().Contain(r => r.Id == "custom-2");
        allRoles.Should().Contain(r => r.Id == "general_review"); // Updated to new default role ID
    }
    
    [Fact]
    public void CustomRoles_PersistAcrossInstances_LoadedCorrectly()
    {
        // Arrange: Add role in first instance
        _customRepo.AddRole(new Role(
            Id: "persistent-role",
            Name: "Persistent Role",
            Icon: "💾",
            Description: "Should persist",
            SystemPrompt: "I persist across instances",
            Tone: "Reliable",
            Style: "Consistent",
            Priorities: new[] { "Stability" },
            Expertise: new[] { "Persistence" }));
        
        // Act: Create new repository instance (simulates app restart)
        var newRepo = new YamlCustomRoleRepository(_testDirectory);
        var loaded = newRepo.GetCustomRoles();
        
        // Assert
        loaded.Should().ContainSingle();
        loaded.First().Id.Should().Be("persistent-role");
        loaded.First().Name.Should().Be("Persistent Role");
        loaded.First().SystemPrompt.Should().Be("I persist across instances");
    }
    
    [Fact]
    public void AddRole_DuplicateId_ThrowsException()
    {
        // Arrange
        var role = new Role(
            Id: "duplicate-id",
            Name: "First Role",
            Icon: "1️⃣",
            Description: "First role",
            SystemPrompt: "First",
            Tone: "First",
            Style: "First",
            Priorities: new[] { "First" },
            Expertise: new[] { "First" });
        
        _customRepo.AddRole(role);
        
        // Act & Assert
        var duplicate = role with { Name = "Second Role" };
        _customRepo.Invoking(r => r.AddRole(duplicate))
            .Should().Throw<InvalidOperationException>()
            .WithMessage("*already exists*");
    }
    
    [Fact]
    public void UpdateRole_NonExistentRole_ThrowsException()
    {
        // Arrange
        var role = new Role(
            Id: "non-existent",
            Name: "Non-existent",
            Icon: "❓",
            Description: "Does not exist",
            SystemPrompt: "N/A",
            Tone: "N/A",
            Style: "N/A",
            Priorities: new[] { "N/A" },
            Expertise: new[] { "N/A" });
        
        // Act & Assert
        _customRepo.Invoking(r => r.UpdateRole(role))
            .Should().Throw<InvalidOperationException>()
            .WithMessage("*not found*");
    }
    
    [Fact]
    public void DeleteRole_NonExistentRole_DoesNotThrow()
    {
        // Act & Assert (should silently ignore)
        _customRepo.Invoking(r => r.DeleteRole("non-existent"))
            .Should().NotThrow();
    }
    
    [Fact]
    public void Exists_WithExistingRole_ReturnsTrue()
    {
        // Arrange
        _customRepo.AddRole(new Role(
            Id: "exists-test",
            Name: "Exists Test",
            Icon: "✅",
            Description: "Test",
            SystemPrompt: "Test",
            Tone: "Test",
            Style: "Test",
            Priorities: new[] { "Test" },
            Expertise: new[] { "Test" }));
        
        // Act & Assert
        _customRepo.Exists("exists-test").Should().BeTrue();
        _customRepo.Exists("does-not-exist").Should().BeFalse();
    }
    
    [Fact]
    public void CustomRoles_AlwaysHaveIsBuiltInFalse_EvenIfExplicitlySet()
    {
        // Arrange: Try to create a custom role with IsBuiltIn = true (should be overridden)
        var role = new Role(
            Id: "custom-forced",
            Name: "Custom Forced",
            Icon: "🔒",
            Description: "Test",
            SystemPrompt: "Test",
            Tone: "Test",
            Style: "Test",
            Priorities: new[] { "Test" },
            Expertise: new[] { "Test" })
        {
            IsBuiltIn = true // This should be overridden
        };
        
        // Act
        _customRepo.AddRole(role);
        var retrieved = _customRepo.GetCustomRoles().First();
        
        // Assert
        retrieved.IsBuiltIn.Should().BeFalse("custom roles are never built-in");
    }
}

