using System;
using System.Collections.Generic;

namespace RevivalSong.Models;

public partial class Stanza
{
    public int Id { get; set; }

    public int SongId { get; set; }

    public string Language { get; set; } = null!;

    public string SectionType { get; set; } = null!;

    public int? SectionNumber { get; set; }

    public string? Label { get; set; }

    public string Lyrics { get; set; } = null!;

    public string? KeyChange { get; set; }

    public int SortOrder { get; set; }

    public int? SplitParenthesis { get; set; }

    public virtual Song Song { get; set; } = null!;
}
