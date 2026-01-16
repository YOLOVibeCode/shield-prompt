using ShieldPrompt.Domain.Records;

namespace ShieldPrompt.Application.Interfaces;

/// <summary>
/// Repository for retrieving AI role definitions.
/// ISP-compliant: Only read operations, no write operations.
/// Follows Single Responsibility Principle.
/// </summary>
public interface IRoleRepository
{
    /// <summary>
    /// Gets all available roles (built-in + custom).
    /// </summary>
    IReadOnlyList<Role> GetAllRoles();
    
    /// <summary>
    /// Gets a specific role by ID.
    /// </summary>
    /// <param name="roleId">The unique role identifier.</param>
    /// <returns>The role if found, null otherwise.</returns>
    Role? GetById(string roleId);
    
    /// <summary>
    /// Gets the default role (Software Engineer).
    /// </summary>
    Role GetDefault();
}

