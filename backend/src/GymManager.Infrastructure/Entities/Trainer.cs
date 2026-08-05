using System;
using System.Collections.Generic;

namespace GymManager.Infrastructure.Entities;

public partial class Trainer
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? MiddleName { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
