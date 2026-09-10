using System;
using System.Collections.Generic;

namespace RevivalSong.Models;

public partial class SonglistItem
{
    public int Id { get; set; }

    public int SonglistId { get; set; }

    public int? SongId { get; set; }

    public int ItemOrder { get; set; }

    public string? CustomKey { get; set; }

    public string? CustomLanguageOrder { get; set; }

    public string? CustomStanzaOrder { get; set; }

    public string? Notes { get; set; }

    public string? ItemType { get; set; }

    public string? SectionTitle { get; set; }
}
