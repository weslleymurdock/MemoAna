using MemoAna.Backend.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace MemoAna.Backend.Infrastructure.Identity.Models;

/// <summary>
/// Represents an MemoAna.Backend application role.
/// </summary>
public class Role
    : IdentityRole<string>,
      IEntityBase,
      ISoftDeletable
{
    protected Role() : base() => Id = Guid.CreateVersion7().ToString();
    
    public Role(string? name = null) : base(name ?? "user") => Id = Guid.CreateVersion7().ToString();
    /// <summary>
    /// Gets or sets whether the role is deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the deletion timestamp.
    /// </summary>
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } =
        DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the creator identifier.
    /// </summary>
    public string CreatedBy { get; set; } =
        string.Empty;

    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; } =
        DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the last updater identifier.
    /// </summary>
    public string UpdatedBy { get; set; } =
        string.Empty;
}
