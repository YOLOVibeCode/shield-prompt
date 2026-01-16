using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShieldPrompt.Application.Interfaces;
using ShieldPrompt.Domain.Records;

namespace ShieldPrompt.App.ViewModels;

/// <summary>
/// ViewModel for the Role Editor tab.
/// Allows users to create, edit, and delete custom AI roles.
/// </summary>
public partial class RoleEditorViewModel : ViewModelBase
{
    private readonly IRoleRepository _builtInRoleRepository;
    private readonly ICustomRoleRepository _customRoleRepository;
    
    public ObservableCollection<RoleItemViewModel> AllRoles { get; } = new();
    
    [ObservableProperty]
    private RoleItemViewModel? _selectedRole;
    
    [ObservableProperty]
    private string _roleId = string.Empty;
    
    [ObservableProperty]
    private string _roleName = string.Empty;
    
    [ObservableProperty]
    private string _roleIcon = "🎭";
    
    [ObservableProperty]
    private string _roleDescription = string.Empty;
    
    [ObservableProperty]
    private string _systemPrompt = string.Empty;
    
    [ObservableProperty]
    private string _tone = string.Empty;
    
    [ObservableProperty]
    private string _style = string.Empty;
    
    [ObservableProperty]
    private string _priorities = string.Empty;
    
    [ObservableProperty]
    private string _expertise = string.Empty;
    
    [ObservableProperty]
    private bool _isEditMode;
    
    [ObservableProperty]
    private string _statusMessage = string.Empty;
    
    public RoleEditorViewModel(
        IRoleRepository builtInRoleRepository,
        ICustomRoleRepository customRoleRepository)
    {
        _builtInRoleRepository = builtInRoleRepository;
        _customRoleRepository = customRoleRepository;
        
        LoadAllRoles();
    }
    
    [RelayCommand]
    private void CreateNew()
    {
        ClearForm();
        IsEditMode = false;
        RoleId = $"custom-{Guid.NewGuid().ToString().Substring(0, 8)}";
        StatusMessage = "Creating new role...";
    }
    
    [RelayCommand]
    private void Save()
    {
        try
        {
            // Validate
            if (string.IsNullOrWhiteSpace(RoleName))
            {
                StatusMessage = "❌ Role name is required";
                return;
            }
            
            if (string.IsNullOrWhiteSpace(SystemPrompt))
            {
                StatusMessage = "❌ System prompt is required";
                return;
            }
            
            // Parse comma-separated values
            var priorities = Priorities.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToArray();
            var expertise = Expertise.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToArray();
            
            var role = new Role(
                Id: RoleId,
                Name: RoleName,
                Icon: string.IsNullOrWhiteSpace(RoleIcon) ? "🎭" : RoleIcon,
                Description: RoleDescription,
                SystemPrompt: SystemPrompt,
                Tone: string.IsNullOrWhiteSpace(Tone) ? "Professional" : Tone,
                Style: string.IsNullOrWhiteSpace(Style) ? "Clear and concise" : Style,
                Priorities: priorities,
                Expertise: expertise)
            {
                IsBuiltIn = false
            };
            
            if (IsEditMode)
            {
                _customRoleRepository.UpdateRole(role);
                StatusMessage = $"✅ Updated role '{RoleName}'";
            }
            else
            {
                _customRoleRepository.AddRole(role);
                StatusMessage = $"✅ Created role '{RoleName}'";
            }
            
            LoadAllRoles();
            ClearForm();
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Error: {ex.Message}";
        }
    }
    
    [RelayCommand]
    private void Delete()
    {
        if (SelectedRole == null || SelectedRole.IsBuiltIn)
        {
            StatusMessage = "❌ Cannot delete built-in roles";
            return;
        }
        
        try
        {
            _customRoleRepository.DeleteRole(SelectedRole.Role.Id);
            StatusMessage = $"✅ Deleted role '{SelectedRole.Role.Name}'";
            LoadAllRoles();
            ClearForm();
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Error: {ex.Message}";
        }
    }
    
    [RelayCommand]
    private void Cancel()
    {
        ClearForm();
        StatusMessage = "Cancelled";
    }
    
    partial void OnSelectedRoleChanged(RoleItemViewModel? value)
    {
        if (value == null) return;
        
        // Populate form
        RoleId = value.Role.Id;
        RoleName = value.Role.Name;
        RoleIcon = value.Role.Icon;
        RoleDescription = value.Role.Description;
        SystemPrompt = value.Role.SystemPrompt;
        Tone = value.Role.Tone;
        Style = value.Role.Style;
        Priorities = string.Join(", ", value.Role.Priorities);
        Expertise = string.Join(", ", value.Role.Expertise);
        IsEditMode = !value.IsBuiltIn;
        
        StatusMessage = value.IsBuiltIn 
            ? "ℹ️ Built-in roles are read-only" 
            : $"Editing '{value.Role.Name}'";
    }
    
    private void LoadAllRoles()
    {
        AllRoles.Clear();
        
        // Load built-in roles (read-only)
        foreach (var role in _builtInRoleRepository.GetAllRoles())
        {
            AllRoles.Add(new RoleItemViewModel(role, IsBuiltIn: true));
        }
        
        // Load custom roles (editable)
        foreach (var role in _customRoleRepository.GetCustomRoles())
        {
            AllRoles.Add(new RoleItemViewModel(role, IsBuiltIn: false));
        }
    }
    
    private void ClearForm()
    {
        SelectedRole = null;
        RoleId = string.Empty;
        RoleName = string.Empty;
        RoleIcon = "🎭";
        RoleDescription = string.Empty;
        SystemPrompt = string.Empty;
        Tone = string.Empty;
        Style = string.Empty;
        Priorities = string.Empty;
        Expertise = string.Empty;
        IsEditMode = false;
    }
}

/// <summary>
/// Wrapper for Role to display in list with built-in indicator.
/// </summary>
public record RoleItemViewModel(Role Role, bool IsBuiltIn)
{
    public string DisplayName => IsBuiltIn 
        ? $"{Role.Icon} {Role.Name} (Built-in)" 
        : $"{Role.Icon} {Role.Name}";
}

