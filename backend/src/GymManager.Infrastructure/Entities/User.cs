using System;
using System.Collections.Generic;

namespace GymManager.Infrastructure.Entities;

/// <summary>
/// Сотрудники, оформляющие посещения
/// </summary>
public partial class User
{
    public Guid Id { get; set; }

    public string Username { get; set; } = null!;

    /// <summary>
    /// BCrypt-хеш. Пароль в открытом виде не хранится
    /// </summary>
    public string PasswordHash { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? MiddleName { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
