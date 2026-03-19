using System;
using System.Collections.Generic;
using System.Text;

namespace SportsLeague.Domain.Entities;

public abstract class AuditBase
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
}
