using System;
using System.Collections.Generic;

namespace RevivalSong.Models;

public partial class Songlist
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public DateTime? EventDate { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }
}
