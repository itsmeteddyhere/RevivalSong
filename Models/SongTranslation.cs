using System;
using System.Collections.Generic;

namespace RevivalSong.Models;

public partial class SongTranslation
{
    public int Id { get; set; }

    public int SongId { get; set; }

    public string Language { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? AltTitle { get; set; }

    public string? TitleClean { get; set; }

    public string? Translator { get; set; }

    public string? StanzaOrder { get; set; }

    public virtual Song Song { get; set; } = null!;
}
