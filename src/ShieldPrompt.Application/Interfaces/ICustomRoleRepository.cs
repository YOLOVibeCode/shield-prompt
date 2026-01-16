using ShieldPrompt.Domain.Records;

namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Repository for managing custom user-created roles.
/// ISP-compliant: Separate from IRoleRepository (read-only built-in roles).
/// Handles CRUD operations for custom roles only.
/// </summary>
public interface ICustomRoleRepository
{
    /// <summary>
    /// Gets all custom roles created by the user.
    /// </summary>
    IReadOnlyList<Role> GetCustomRoles();
    
    /// <summary>
    /// Adds a new custom role.
    /// </summary>
    /// <param name="role">The role to add.</param>
    void AddRole(Role role);
    
    /// <summary>
    /// Updates an existing custom role.
    /// </summary>
    /// <param name="role">The role to update.</param>
    void UpdateRole(Role role);
    
    /// <summary>
    /// Deletes a custom role by ID.
    /// </summary>
    /// <param name="roleId">The ID of the role to delete.</param>
    void DeleteRole(string roleId);
    
    /// <summary>
    /// Checks if a role ID exists in custom roles.
    /// </summary>
    /// <param name="roleId">The role ID to check.</param>
    /// <returns>True if the role exists, false otherwise.</returns>
    bool Exists(string roleId);
}

