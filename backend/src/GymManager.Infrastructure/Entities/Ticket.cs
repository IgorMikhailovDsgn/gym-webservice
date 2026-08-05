using System;
using System.Collections.Generic;

namespace GymManager.Infrastructure.Entities;

public partial class Ticket
{
    public Guid Id { get; set; }

    public Guid ClientId { get; set; }

    public Guid TicketTypeId { get; set; }

    public DateOnly DateStart { get; set; }

    public DateOnly DateEnd { get; set; }

    public int VisitsUsed { get; set; }

    public int? VisitsLimit { get; set; }

    public bool IsCancelled { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual TicketType TicketType { get; set; } = null!;

    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
