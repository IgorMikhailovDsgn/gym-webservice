using System;
using System.Collections.Generic;

namespace GymManager.Infrastructure.Entities;

public partial class TicketType
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public int? DurationDays { get; set; }

    /// <summary>
    /// NULL = абонемент без ограничения по количеству посещений
    /// </summary>
    public int? DefaultVisits { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
