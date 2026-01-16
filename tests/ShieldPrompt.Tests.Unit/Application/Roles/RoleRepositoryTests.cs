using FluentAssertions;
using ShieldPrompt.Domain.Records;
using ShieldPrompt.Infrastructure.Services;

namespace ShieldPrompt.Tests.Unit.Application.Roles;

/// <summary>
/// Tests for IRoleRepository implementation.
/// TDD: Tests written BEFORE implementation.
/// </summary>
public class RoleRepositoryTests
{
    private readonly YamlRoleRepository _sut;
    
    public RoleRepositoryTests()
    {
        // Use real implementation for testing
        _sut = new YamlRoleRepository();
    }
    
    [Fact]
    public void GetAllRoles_ReturnsAllDefinedRoles()
    {
        // Act
        var roles = _sut.GetAllRoles();
        
        // Assert
        roles.Should().NotBeNull();
        roles.Should().NotBeEmpty();
    }
    
    [Fact]
    public void GetAllRoles_ReturnsAtLeast10Roles()
    {
        // Act
        var roles = _sut.GetAllRoles();
        
        // Assert
        roles.Count.Should().BeGreaterThanOrEqualTo(10, 
            "we should have at least 10 built-in roles");
    }
    
    [Fact]
    public void GetById_WithValidId_ReturnsRole()
    {
        // Arrange
        var allRoles = _sut.GetAllRoles();
        var firstRoleId = allRoles.First().Id;
        
        // Act
        var role = _sut.GetById(firstRoleId);
        
        // Assert
        role.Should().NotBeNull();
        role!.Id.Should().Be(firstRoleId);
    }
    
    [Fact]
    public void GetById_WithInvalidId_ReturnsNull()
    {
        // Act
        var role = _sut.GetById("non-existent-role-id");
        
        // Assert
        role.Should().BeNull();
    }
    
    [Fact]
    public void GetDefault_ReturnsGeneralReviewRole()
    {
        // Act
        var defaultRole = _sut.GetDefault();
        
        // Assert
        defaultRole.Should().NotBeNull();
        defaultRole.Id.Should().Be("general_review", 
            "General Review should be the default role");
    }
    
    [Fact]
    public void AllRoles_HaveRequiredProperties_NotNullOrEmpty()
    {
        // Act
        var roles = _sut.GetAllRoles();
        
        // Assert
        foreach (var role in roles)
        {
            role.Id.Should().NotBeNullOrWhiteSpace($"Role {role.Name} has no ID");
            role.Name.Should().NotBeNullOrWhiteSpace($"Role {role.Id} has no Name");
            role.Icon.Should().NotBeNullOrWhiteSpace($"Role {role.Id} has no Icon");
            role.Description.Should().NotBeNullOrWhiteSpace($"Role {role.Id} has no Description");
            role.SystemPrompt.Should().NotBeNullOrWhiteSpace($"Role {role.Id} has no SystemPrompt");
            role.Tone.Should().NotBeNullOrWhiteSpace($"Role {role.Id} has no Tone");
            role.Style.Should().NotBeNullOrWhiteSpace($"Role {role.Id} has no Style");
        }
    }
    
    [Fact]
    public void AllRoles_HaveUniqueIds()
    {
        // Act
        var roles = _sut.GetAllRoles();
        var ids = roles.Select(r => r.Id).ToList();
        
        // Assert
        ids.Should().OnlyHaveUniqueItems("each role must have a unique ID");
    }
    
    [Fact]
    public void AllRoles_HaveValidIcons_NotEmpty()
    {
        // Act
        var roles = _sut.GetAllRoles();
        
        // Assert
        foreach (var role in roles)
        {
            role.Icon.Should().NotBeNullOrWhiteSpace($"Role {role.Name} should have an icon");
            role.Icon.Length.Should().BeGreaterThanOrEqualTo(1, 
                $"Role {role.Name} icon should be at least 1 character (emoji)");
        }
    }
    
    [Fact]
    public void AllRoles_HaveSystemPrompts_AtLeast50Characters()
    {
        // Act
        var roles = _sut.GetAllRoles();
        
        // Assert
        foreach (var role in roles)
        {
            role.SystemPrompt.Length.Should().BeGreaterThanOrEqualTo(50, 
                $"Role {role.Name} should have a substantial system prompt (at least 50 characters)");
        }
    }
    
    [Fact]
    public void AllRoles_HavePriorities_AtLeastOne()
    {
        // Act
        var roles = _sut.GetAllRoles();
        
        // Assert
        foreach (var role in roles)
        {
            role.Priorities.Should().NotBeEmpty($"Role {role.Name} should have at least one priority");
        }
    }
    
    [Fact]
    public void GetById_WithGeneralReview_ReturnsExpectedProperties()
    {
        // Act
        var generalReview = _sut.GetById("general_review");
        
        // Assert
        generalReview.Should().NotBeNull();
        generalReview!.Name.Should().Contain("General");
        generalReview.Icon.Should().Be("🔧"); // Actual icon from roles.yaml
        generalReview.IsBuiltIn.Should().BeTrue();
        generalReview.Priorities.Should().NotBeEmpty();
    }
    
    [Fact]
    public void GetById_WithArchitectureReview_ReturnsExpectedProperties()
    {
        // Act
        var architect = _sut.GetById("architecture_review");
        
        // Assert
        architect.Should().NotBeNull();
        architect!.Name.Should().Contain("Architecture");
        architect.Icon.Should().Be("🏗️");
        architect.IsBuiltIn.Should().BeTrue();
    }
}

