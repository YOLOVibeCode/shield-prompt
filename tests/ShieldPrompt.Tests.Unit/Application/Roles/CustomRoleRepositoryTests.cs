using FluentAssertions;
using ShieldPrompt.Domain.Records;
using ShieldPrompt.Infrastructure.Services;

namespace ShieldPrompt.Tests.Unit.Application.Roles;

/// <summary>
/// Tests for ICustomRoleRepository implementation.
/// TDD: Tests written BEFORE implementation.
/// </summary>
public class CustomRoleRepositoryTests
{
    private readonly YamlCustomRoleRepository _sut;
    private readonly string _testDirectory;
    
    public CustomRoleRepositoryTests()
    {
        // Use unique test directory for each test run
        _testDirectory = Path.Combine(Path.GetTempPath(), "ShieldPromptTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
        _sut = new YamlCustomRoleRepository(_testDirectory);
    }
    
    [Fact]
    public void GetCustomRoles_Initially_ReturnsEmptyList()
    {
        // Act
        var roles = _sut.GetCustomRoles();
        
        // Assert
        roles.Should().NotBeNull();
        roles.Should().BeEmpty("no custom roles exist initially");
    }
    
    [Fact]
    public void AddRole_WithValidRole_AddsSuccessfully()
    {
        // Arrange
        var customRole = new Role(
            Id: "custom-tester",
            Name: "Custom Tester",
            Icon: "🧪",
            Description: "A custom test role",
            SystemPrompt: "You are a custom test role for testing purposes.",
            Tone: "Professional",
            Style: "Detailed",
            Priorities: new[] { "Testing", "Quality" },
            Expertise: new[] { "Test Automation", "QA" })
        {
            IsBuiltIn = false
        };
        
        // Act
        _sut.AddRole(customRole);
        var roles = _sut.GetCustomRoles();
        
        // Assert
        roles.Should().ContainSingle();
        roles[0].Id.Should().Be("custom-tester");
        roles[0].Name.Should().Be("Custom Tester");
    }
    
    [Fact]
    public void AddRole_Multiple_AddsAll()
    {
        // Arrange
        var role1 = CreateTestRole("role1", "Role 1");
        var role2 = CreateTestRole("role2", "Role 2");
        
        // Act
        _sut.AddRole(role1);
        _sut.AddRole(role2);
        var roles = _sut.GetCustomRoles();
        
        // Assert
        roles.Should().HaveCount(2);
    }
    
    [Fact]
    public void UpdateRole_ExistingRole_UpdatesSuccessfully()
    {
        // Arrange
        var originalRole = CreateTestRole("test-role", "Original Name");
        _sut.AddRole(originalRole);
        
        var updatedRole = originalRole with { Name = "Updated Name" };
        
        // Act
        _sut.UpdateRole(updatedRole);
        var roles = _sut.GetCustomRoles();
        
        // Assert
        roles[0].Name.Should().Be("Updated Name");
    }
    
    [Fact]
    public void DeleteRole_ExistingRole_RemovesSuccessfully()
    {
        // Arrange
        var role = CreateTestRole("to-delete", "Delete Me");
        _sut.AddRole(role);
        
        // Act
        _sut.DeleteRole("to-delete");
        var roles = _sut.GetCustomRoles();
        
        // Assert
        roles.Should().BeEmpty();
    }
    
    [Fact]
    public void Exists_ExistingRole_ReturnsTrue()
    {
        // Arrange
        var role = CreateTestRole("exists", "Exists");
        _sut.AddRole(role);
        
        // Act
        var exists = _sut.Exists("exists");
        
        // Assert
        exists.Should().BeTrue();
    }
    
    [Fact]
    public void Exists_NonExistingRole_ReturnsFalse()
    {
        // Act
        var exists = _sut.Exists("non-existent");
        
        // Assert
        exists.Should().BeFalse();
    }
    
    [Fact]
    public void GetCustomRoles_AfterAddingAndDeleting_ReturnsCorrectCount()
    {
        // Arrange
        _sut.AddRole(CreateTestRole("role1", "Role 1"));
        _sut.AddRole(CreateTestRole("role2", "Role 2"));
        _sut.AddRole(CreateTestRole("role3", "Role 3"));
        
        // Act
        _sut.DeleteRole("role2");
        var roles = _sut.GetCustomRoles();
        
        // Assert
        roles.Should().HaveCount(2);
        roles.Should().NotContain(r => r.Id == "role2");
    }
    
    [Fact]
    public void AddRole_PersistsToFile()
    {
        // Arrange
        var role = CreateTestRole("persisted", "Persisted Role");
        _sut.AddRole(role);
        
        // Act - Create new repository instance to verify persistence
        var newRepo = new YamlCustomRoleRepository(_testDirectory);
        var roles = newRepo.GetCustomRoles();
        
        // Assert
        roles.Should().ContainSingle();
        roles[0].Id.Should().Be("persisted");
    }
    
    [Fact]
    public void UpdateRole_PersistsChanges()
    {
        // Arrange
        var role = CreateTestRole("update-test", "Original");
        _sut.AddRole(role);
        
        var updated = role with { Name = "Updated" };
        _sut.UpdateRole(updated);
        
        // Act - Create new repository instance
        var newRepo = new YamlCustomRoleRepository(_testDirectory);
        var roles = newRepo.GetCustomRoles();
        
        // Assert
        roles[0].Name.Should().Be("Updated");
    }
    
    [Fact]
    public void DeleteRole_PersistsRemoval()
    {
        // Arrange
        _sut.AddRole(CreateTestRole("delete-test", "To Delete"));
        _sut.DeleteRole("delete-test");
        
        // Act - Create new repository instance
        var newRepo = new YamlCustomRoleRepository(_testDirectory);
        var roles = newRepo.GetCustomRoles();
        
        // Assert
        roles.Should().BeEmpty();
    }
    
    private static Role CreateTestRole(string id, string name)
    {
        return new Role(
            Id: id,
            Name: name,
            Icon: "🧪",
            Description: "Test role description",
            SystemPrompt: "This is a test system prompt for testing purposes.",
            Tone: "Professional",
            Style: "Clear and concise",
            Priorities: new[] { "Testing" },
            Expertise: new[] { "Unit Testing" })
        {
            IsBuiltIn = false
        };
    }
}

