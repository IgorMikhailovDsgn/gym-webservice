using System;
using System.Collections.Generic;

namespace GymManager.Infrastructure.Entities;

public partial class VTicket
{
    public Guid? Id { get; set; }

    public Guid? ClientId { get; set; }

    public Guid? TicketTypeId { get; set; }

    public string? TicketTypeCode { get; set; }

    public string? TicketTypeName { get; set; }

    public DateOnly? DateStart { get; set; }

    public DateOnly? DateEnd { get; set; }

    public int? VisitsLimit { get; set; }

    public int? VisitsUsed { get; set; }

    public int? VisitsRemaining { get; set; }

    public bool? IsCancelled { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }
}
