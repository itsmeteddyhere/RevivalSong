using System;
using System.Collections.Generic;

namespace RevivalSong.Models;

public partial class Song
{
    public int Id { get; set; }

    public int? SongNumber { get; set; }

    public int? Gsn { get; set; }

    public string? Grk { get; set; }

    public string? DefaultKey { get; set; }

    public string? TimeSignature { get; set; }

    public int? Bpm { get; set; }

    public string? SongType { get; set; }

    public string? Author { get; set; }

    public string? CcliNumber { get; set; }

    public int? Year { get; set; }

    public string? Copyright { get; set; }

    public string? Assembly { get; set; }

    public string? Scripture { get; set; }

    public string? DefaultLanguageOrder { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

    public virtual ICollection<SongTranslation> SongTranslations { get; set; } = new List<SongTranslation>();

    public virtual ICollection<Stanza> Stanzas { get; set; } = new List<Stanza>();
}
