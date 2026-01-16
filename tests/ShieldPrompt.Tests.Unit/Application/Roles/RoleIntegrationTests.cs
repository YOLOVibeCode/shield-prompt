using FluentAssertions;
using ShieldPrompt.Application.Services;
using ShieldPrompt.Domain.Entities;
using ShieldPrompt.Domain.Records;
using ShieldPrompt.Infrastructure.Services;

namespace ShieldPrompt.Tests.Unit.Application.Roles;

/// <summary>
/// End-to-end integration tests for the role system.
/// Tests that roles are properly injected into prompts.
/// </summary>
public class RoleIntegrationTests
{
    private readonly YamlRoleRepository _roleRepository;
    private readonly YamlPromptTemplateRepository _templateRepository;
    private readonly TokenCountingService _tokenService;
    private readonly PromptComposer _composer;
    
    public RoleIntegrationTests()
    {
        _roleRepository = new YamlRoleRepository();
        _templateRepository = new YamlPromptTemplateRepository();
        _tokenService = new TokenCountingService();
        var xmlBuilder = new XmlPromptBuilder();
        _composer = new PromptComposer(_tokenService, xmlBuilder);
    }
    
    [Fact]
    public void Compose_WithRole_IncludesRoleSystemPrompt()
    {
        // Arrange
        var role = _roleRepository.GetById("general_review"); // Updated to new role ID
        var template = _templateRepository.GetById("code_review");
        
        var testFile = new FileNode("test.cs", "test.cs", false);
        testFile.Content = "public class Test { }";
        
        var files = new List<FileNode> { testFile };
        
        var options = new PromptOptions(
            SelectedRole: role);
        
        // Act
        var result = _composer.Compose(template!, files, options);
        
        // Assert
        result.FullPrompt.Should().Contain(role!.Name, "role name should be in prompt");
        result.FullPrompt.Should().Contain(role.SystemPrompt, "role system prompt should be in full prompt");
    }
    
    [Fact]
    public void Compose_WithRoleAndTemplate_IncludesBothSystemPrompts()
    {
        // Arrange
        var role = _roleRepository.GetById("security_audit"); // Updated to new role ID
        var template = _templateRepository.GetById("code_review");
        
        var authFile = new FileNode("auth.cs", "auth.cs", false);
        authFile.Content = "public class Auth { public string Password { get; set; } }";
        
        var files = new List<FileNode> { authFile };
        
        var options = new PromptOptions(
            SelectedRole: role);
        
        // Act
        var result = _composer.Compose(template!, files, options);
        
        // Assert
        result.FullPrompt.Should().Contain("Security", "security role should be mentioned");
        result.FullPrompt.Should().Contain(role!.SystemPrompt, "role system prompt should be in prompt");
        result.FullPrompt.Should().Contain("code review", Exactly.Once(), "template system prompt contains 'code review'");
    }
    
    [Fact]
    public void Compose_WithDifferentRoles_ProducesDifferentPrompts()
    {
        // Arrange
        var generalReviewRole = _roleRepository.GetById("general_review"); // Updated to new role ID
        var architectureRole = _roleRepository.GetById("architecture_review"); // Updated to new role ID
        var template = _templateRepository.GetById("code_review");
        
        var serviceFile = new FileNode("service.cs", "service.cs", false);
        serviceFile.Content = "public class Service { }";
        
        var files = new List<FileNode> { serviceFile };
        
        var generalOptions = new PromptOptions(SelectedRole: generalReviewRole);
        var architectureOptions = new PromptOptions(SelectedRole: architectureRole);
        
        // Act
        var generalResult = _composer.Compose(template!, files, generalOptions);
        var architectureResult = _composer.Compose(template!, files, architectureOptions);
        
        // Assert
        generalResult.FullPrompt.Should().Contain("General");
        architectureResult.FullPrompt.Should().Contain("Architecture");
        
        generalResult.FullPrompt.Should().NotBe(architectureResult.FullPrompt, 
            "different roles should produce different prompts");
    }
    
    [Fact]
    public void Compose_WithoutRole_DoesNotIncludeRoleSection()
    {
        // Arrange
        var template = _templateRepository.GetById("code_review");
        
        var testFile = new FileNode("test.cs", "test.cs", false);
        testFile.Content = "public class Test { }";
        
        var files = new List<FileNode> { testFile };
        
        var options = new PromptOptions(SelectedRole: null);
        
        // Act
        var result = _composer.Compose(template!, files, options);
        
        // Assert
        result.FullPrompt.Should().NotContain("# AI Role", "role section should not be included when no role is selected");
    }
    
    [Fact]
    public void Compose_WithRoleAndFocusAreas_IncludesBoth()
    {
        // Arrange
        var role = _roleRepository.GetById("security_audit"); // Updated to new role ID
        var template = _templateRepository.GetById("code_review");
        
        var authFile = new FileNode("auth.cs", "auth.cs", false);
        authFile.Content = "public class Auth { }";
        
        var files = new List<FileNode> { authFile };
        
        var options = new PromptOptions(
            SelectedRole: role,
            SelectedFocusAreas: new List<string> { "Security", "Performance" });
        
        // Act
        var result = _composer.Compose(template!, files, options);
        
        // Assert
        result.FullPrompt.Should().Contain("Security");
        result.FullPrompt.Should().Contain("Focus Areas:");
        result.FullPrompt.Should().Contain("- Security");
        result.FullPrompt.Should().Contain("- Performance");
    }
    
    [Fact]
    public void Compose_WithAllRoles_GeneratesUniquePrompts()
    {
        // Arrange
        var allRoles = _roleRepository.GetAllRoles();
        var template = _templateRepository.GetById("code_review");
        
        var exampleFile = new FileNode("example.cs", "example.cs", false);
        exampleFile.Content = "public class Example { }";
        
        var files = new List<FileNode> { exampleFile };
        
        var prompts = new Dictionary<string, string>();
        
        // Act
        foreach (var role in allRoles)
        {
            var options = new PromptOptions(SelectedRole: role);
            var result = _composer.Compose(template!, files, options);
            prompts[role.Id] = result.FullPrompt;
        }
        
        // Assert
        prompts.Should().HaveCount(allRoles.Count, "each role should generate a prompt");
        prompts.Values.Distinct().Should().HaveCount(prompts.Count, 
            "all prompts should be unique");
    }
}

