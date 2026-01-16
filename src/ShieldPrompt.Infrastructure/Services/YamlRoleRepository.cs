using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Domain.Records;

namespace ShieldPrompt.Infrastructure.Services;

/// <summary>
/// YAML-based implementation of role repository.
/// Loads roles from config/roles.yaml file.
/// </summary>
public class YamlRoleRepository : IRoleRepository
{
    private readonly Lazy<List<Role>> _rolesCache;
    private readonly string _yamlFilePath;
    
    public YamlRoleRepository()
    {
        // Look for roles.yaml in config directory
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        _yamlFilePath = Path.Combine(baseDir, "..", "..", "..", "..", "..", "config", "roles.yaml");
        _yamlFilePath = Path.GetFullPath(_yamlFilePath);
        
        _rolesCache = new Lazy<List<Role>>(() => LoadRolesFromYaml());
    }
    
    public IReadOnlyList<Role> GetAllRoles()
    {
        return _rolesCache.Value.AsReadOnly();
    }
    
    public Role? GetById(string roleId)
    {
        return _rolesCache.Value.FirstOrDefault(r => 
            r.Id.Equals(roleId, StringComparison.OrdinalIgnoreCase));
    }
    
    public Role GetDefault()
    {
        // Try to get "General Code Review" as the default (most common use case)
        var defaultRole = GetById("general_review");
        if (defaultRole == null)
        {
            // Fallback to the first role if general_review doesn't exist
            defaultRole = _rolesCache.Value.FirstOrDefault();
        }
        
        if (defaultRole == null)
        {
            throw new InvalidOperationException("No roles found in repository");
        }
        
        return defaultRole;
    }
    
    private List<Role> LoadRolesFromYaml()
    {
        try
        {
            if (!File.Exists(_yamlFilePath))
            {
                Console.WriteLine($"Warning: roles.yaml not found at {_yamlFilePath}. Using fallback roles.");
                return GetFallbackRoles();
            }
            
            var yaml = File.ReadAllText(_yamlFilePath);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();
            
            var yamlData = deserializer.Deserialize<RolesYaml>(yaml);
            
            if (yamlData?.Roles == null || yamlData.Roles.Count == 0)
            {
                Console.WriteLine("Warning: No roles found in YAML. Using fallback.");
                return GetFallbackRoles();
            }
            
            return yamlData.Roles.Select(r => new Role(
                Id: r.Id ?? "unknown",
                Name: r.Name ?? "Unknown",
                Icon: r.Icon ?? "❓",
                Description: r.Description ?? "",
                SystemPrompt: r.SystemPrompt ?? "",
                Tone: r.Tone ?? "Professional",
                Style: r.Style ?? "Clear and concise",
                Priorities: r.Priorities?.ToList() ?? new List<string>(),
                Expertise: r.Expertise?.ToList() ?? new List<string>())
            {
                IsBuiltIn = true
            }).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading roles from YAML: {ex.Message}. Using fallback roles.");
            return GetFallbackRoles();
        }
    }
    
    private static List<Role> GetFallbackRoles()
    {
        // Minimal fallback if YAML file is missing
        return new List<Role>
        {
            new Role(
                Id: "engineer",
                Name: "Software Engineer",
                Icon: "🔧",
                Description: "Expert software developer focused on code quality and best practices",
                SystemPrompt: "You are an expert software engineer. Focus on code quality, testing, and maintainability.",
                Tone: "Technical, precise",
                Style: "Code-first with explanations",
                Priorities: new[] { "Code Quality", "Testing", "Maintainability" },
                Expertise: new[] { "Clean Architecture", "TDD", "SOLID" })
            {
                IsBuiltIn = true
            }
        };
    }
    
    // YAML deserialization classes
    private class RolesYaml
    {
        public List<RoleYaml>? Roles { get; set; }
    }
    
    private class RoleYaml
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Icon { get; set; }
        public string? Description { get; set; }
        public string? SystemPrompt { get; set; }
        public string? Tone { get; set; }
        public string? Style { get; set; }
        public List<string>? Priorities { get; set; }
        public List<string>? Expertise { get; set; }
    }
}

