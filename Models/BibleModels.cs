using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RevivalSong.Models;

public class BibleTranslation
{
    public string Name { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public List<BibleBook> Books { get; set; } = new();
}

public class BibleBook
{
    public string Name { get; set; } = string.Empty;
    public List<BibleChapter> Chapters { get; set; } = new();
}

public class BibleChapter
{
    public int Number { get; set; }
    public List<BibleVerse> Verses { get; set; } = new();
}

public class BibleVerse
{
    public int Number { get; set; }
    public string Text { get; set; } = string.Empty;
}

public partial class ScriptureListItem : ObservableObject
{
    [ObservableProperty]
    private string _reference = string.Empty; // e.g., "Genesis 1:1"

    [ObservableProperty]
    private string _text = string.Empty;

    // Database entity placeholders (commented out per user request)
    // public int Id { get; set; }
    // public int ScriptureListId { get; set; }
    // public int ItemOrder { get; set; }
}

// Database entity placeholder for the list itself (commented out per user request)
/*
public class ScriptureList
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<ScriptureListItem> Items { get; set; } = new();
}
*/
