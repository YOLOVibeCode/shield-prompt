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
/// YAML-based implementation of custom role repository.
/// Stores custom roles in user's AppData folder: ~/.shieldprompt/custom-roles.yaml
/// ISP: Separate from YamlRoleRepository (built-in roles are read-only).
/// </summary>
public class YamlCustomRoleRepository : ICustomRoleRepository
{
    private readonly string _yamlFilePath;
    private readonly object _lock = new();
    
    public YamlCustomRoleRepository()
        : this(GetDefaultDirectory())
    {
    }
    
    public YamlCustomRoleRepository(string directory)
    {
        Directory.CreateDirectory(directory);
        _yamlFilePath = Path.Combine(directory, "custom-roles.yaml");
    }
    
    public IReadOnlyList<Role> GetCustomRoles()
    {
        lock (_lock)
        {
            if (!File.Exists(_yamlFilePath))
            {
                return Array.Empty<Role>();
            }
            
            try
            {
                var yaml = File.ReadAllText(_yamlFilePath);
                if (string.IsNullOrWhiteSpace(yaml))
                {
                    return Array.Empty<Role>();
                }
                
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(UnderscoredNamingConvention.Instance)
                    .Build();
                
                var rolesData = deserializer.Deserialize<RolesYaml>(yaml);
                
                if (rolesData?.Roles == null || rolesData.Roles.Count == 0)
                {
                    return Array.Empty<Role>();
                }
                
                return rolesData.Roles.Select(r => new Role(
                    Id: r.Id ?? "unknown",
                    Name: r.Name ?? "Unknown",
                    Icon: r.Icon ?? "❓",
                    Description: r.Description ?? "",
                    SystemPrompt: r.SystemPrompt ?? "",
                    Tone: r.Tone ?? "Professional",
                    Style: r.Style ?? "Clear",
                    Priorities: r.Priorities?.ToList() ?? new List<string>(),
                    Expertise: r.Expertise?.ToList() ?? new List<string>())
                {
                    IsBuiltIn = false
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading custom roles: {ex.Message}");
                return Array.Empty<Role>();
            }
        }
    }
    
    public void AddRole(Role role)
    {
        lock (_lock)
        {
            var roles = GetCustomRoles().ToList();
            
            // Check for duplicate ID
            if (roles.Any(r => r.Id == role.Id))
            {
                throw new InvalidOperationException($"Role with ID '{role.Id}' already exists.");
            }
            
            // Ensure IsBuiltIn is false for custom roles
            var customRole = role with { IsBuiltIn = false };
            roles.Add(customRole);
            
            SaveRoles(roles);
        }
    }
    
    public void UpdateRole(Role role)
    {
        lock (_lock)
        {
            var roles = GetCustomRoles().ToList();
            var index = roles.FindIndex(r => r.Id == role.Id);
            
            if (index == -1)
            {
                throw new InvalidOperationException($"Role with ID '{role.Id}' not found.");
            }
            
            // Ensure IsBuiltIn is false for custom roles
            var customRole = role with { IsBuiltIn = false };
            roles[index] = customRole;
            
            SaveRoles(roles);
        }
    }
    
    public void DeleteRole(string roleId)
    {
        lock (_lock)
        {
            var roles = GetCustomRoles().ToList();
            var index = roles.FindIndex(r => r.Id == roleId);
            
            if (index == -1)
            {
                return; // Silently ignore if role doesn't exist
            }
            
            roles.RemoveAt(index);
            SaveRoles(roles);
        }
    }
    
    public bool Exists(string roleId)
    {
        return GetCustomRoles().Any(r => r.Id == roleId);
    }
    
    private void SaveRoles(List<Role> roles)
    {
        try
        {
            var rolesYaml = new RolesYaml
            {
                Roles = roles.Select(r => new RoleYaml
                {
                    Id = r.Id,
                    Name = r.Name,
                    Icon = r.Icon,
                    Description = r.Description,
                    SystemPrompt = r.SystemPrompt,
                    Tone = r.Tone,
                    Style = r.Style,
                    Priorities = r.Priorities.ToList(),
                    Expertise = r.Expertise.ToList()
                }).ToList()
            };
            
            var serializer = new SerializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();
            
            var yaml = serializer.Serialize(rolesYaml);
            File.WriteAllText(_yamlFilePath, yaml);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save custom roles: {ex.Message}", ex);
        }
    }
    
    private static string GetDefaultDirectory()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(appDataPath, "ShieldPrompt");
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

