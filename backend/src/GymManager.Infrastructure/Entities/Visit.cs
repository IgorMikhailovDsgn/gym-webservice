using System;
using System.Collections.Generic;

namespace GymManager.Infrastructure.Entities;

public partial class Visit
{
    public Guid Id { get; set; }

    public Guid TicketId { get; set; }

    public DateTime VisitedAt { get; set; }

    public Guid? TrainerId { get; set; }

    public Guid UserId { get; set; }

    public virtual Ticket Ticket { get; set; } = null!;

    public virtual Trainer? Trainer { get; set; }

    public virtual User User { get; set; } = null!;
}
