using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using RevivalSong.Models;
using RevivalSong.Views;
using RevivalSong.Services;
using System.Threading;
using System.Text.RegularExpressions;

namespace RevivalSong.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private ProjectorWindow? _projectorWindow;
    private ProjectorViewModel _projectorVm = new();
    private List<ProjectorWindow> _projectorWindows = new();

    public BibleProjectorViewModel BibleProjectorVm { get; } = new();
    private List<BibleProjectorWindow> _bibleProjectorWindows = new();

    [ObservableProperty] private bool _isProjectionActive;
    private int _currentTabIndex = 0;

    // ── Timer ──────────────────────────────────────────────────────────────
    public TimerViewModel TimerVm { get; } = new();

    [ObservableProperty] private DisplayItemModel? _selectedTimerDisplay;

    private TimerProjectorWindow? _timerProjectorWindow;

    public void OpenTimerProjector(Window mainWindow)
    {
        CloseTimerProjector();
        if (SelectedTimerDisplay == null) return;

        var screens = mainWindow.Screens.All;
        if (SelectedTimerDisplay.ScreenIndex >= screens.Count) return;

        _timerProjectorWindow = new TimerProjectorWindow { DataContext = TimerVm };
        _timerProjectorWindow.Position = screens[SelectedTimerDisplay.ScreenIndex].WorkingArea.TopLeft;
        _timerProjectorWindow.Show();
    }

    public void CloseTimerProjector()
    {
        _timerProjectorWindow?.Close();
        _timerProjectorWindow = null;
    }
    // ───────────────────────────────────────────────────────────────────────

    // ── Bible ──────────────────────────────────────────────────────────────
    
    [ObservableProperty] private ObservableCollection<BibleTranslation> _availableTranslations = new();
    [ObservableProperty] private BibleTranslation? _selectedTranslation;
    [ObservableProperty] private BibleTranslation? _currentBibleData;
    
    [ObservableProperty] private ObservableCollection<BibleBook> _books = new();
    [ObservableProperty] private BibleBook? _selectedBook;
    
    [ObservableProperty] private ObservableCollection<BibleChapter> _chapters = new();
    [ObservableProperty] private BibleChapter? _selectedChapter;
    
    [ObservableProperty] private ObservableCollection<BibleVerse> _verses = new();
    [ObservableProperty] private BibleVerse? _selectedVerse;

    [ObservableProperty] private ObservableCollection<ScriptureListItem> _scriptureList = new();
    [ObservableProperty] private ScriptureListItem? _selectedScripture;
    
    [ObservableProperty] private string _bibleSearchQuery = "";
    private CancellationTokenSource? _bibleSearchCts;

    partial void OnBibleSearchQueryChanged(string value)
    {
        _bibleSearchCts?.Cancel();
        _bibleSearchCts = new CancellationTokenSource();
        var token = _bibleSearchCts.Token;

        if (token.IsCancellationRequested) return;

        ExecuteBibleSearch(value);
    }

    private void ExecuteBibleSearch(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return;
        if (CurrentBibleData == null) return;

        // Try to match [Book] [Chapter]:[Verse] or similar formats
        // e.g. "Genesis 1:3", "1 John 2:4", "Píseň písní 7:14"
        var match = Regex.Match(query.Trim(), @"^(.*?)\s+(\d+)\s*[:\s]\s*(\d+)$");
        if (!match.Success) return;

        string bookName = match.Groups[1].Value.Trim();
        if (!int.TryParse(match.Groups[2].Value, out int chapterNum)) return;
        if (!int.TryParse(match.Groups[3].Value, out int verseNum)) return;

        // Find Book (case insensitive)
        var book = Books.FirstOrDefault(b => b.Name.Equals(bookName, StringComparison.OrdinalIgnoreCase));
        if (book == null) return;

        SelectedBook = book;

        // Find Chapter
        var chapter = Chapters.FirstOrDefault(c => c.Number == chapterNum);
        if (chapter == null) return;

        SelectedChapter = chapter;

        // Find Verse
        var verse = Verses.FirstOrDefault(v => v.Number == verseNum);
        if (verse == null) return;

        SelectedVerse = verse;
    }

    // DB Context placeholder (commented out per user request)
    // private void SaveScriptureListToDb() {
    //     using var db = new AppDbContext();
    //     // Save logic here...
    //     // db.SaveChanges();
    // }

    public void LoadBibleTranslations()
    {
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        // In dev, the executable is deep in bin/Debug/..., so we need to find the project root or use a relative path
        // A safer bet is checking relative to the current working directory or known path
        string biblesDir = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "bibles");
        if (!System.IO.Directory.Exists(biblesDir)) {
             // Fallback for development if run from IDE
             biblesDir = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "..", "..", "..", "bibles");
        }

        var translations = BibleService.LoadAvailableTranslations(biblesDir);
        AvailableTranslations = new ObservableCollection<BibleTranslation>(translations);
    }

    partial void OnSelectedTranslationChanged(BibleTranslation? value)
    {
        if (value == null) return;
        
        CurrentBibleData = BibleService.LoadTranslationData(value);
        if (CurrentBibleData != null)
        {
            Books = new ObservableCollection<BibleBook>(CurrentBibleData.Books);
        }
        else
        {
            Books.Clear();
        }
        Chapters.Clear();
        Verses.Clear();
    }

    partial void OnSelectedBookChanged(BibleBook? value)
    {
        if (value != null)
        {
            Chapters = new ObservableCollection<BibleChapter>(value.Chapters);
        }
        else
        {
            Chapters.Clear();
        }
        Verses.Clear();
    }

    partial void OnSelectedChapterChanged(BibleChapter? value)
    {
        if (value != null)
        {
            Verses = new ObservableCollection<BibleVerse>(value.Verses);
        }
        else
        {
            Verses.Clear();
        }
    }

    partial void OnSelectedVerseChanged(BibleVerse? value)
    {
        if (value != null && SelectedBook != null && SelectedChapter != null)
        {
            BibleProjectorVm.Reference = $"{SelectedBook.Name} {SelectedChapter.Number}:{value.Number}";
        }
    }

    [RelayCommand]
    private void AddScriptureToPresentation()
    {
        if (SelectedVerse != null && SelectedBook != null && SelectedChapter != null)
        {
            var newItem = new ScriptureListItem
            {
                Reference = $"{SelectedBook.Name} {SelectedChapter.Number}:{SelectedVerse.Number}",
                Text = SelectedVerse.Text
            };
            ScriptureList.Add(newItem);
            
            // SaveScriptureListToDb(); // Uncomment when DB is ready
        }
    }

    [RelayCommand]
    private void RemoveScripture()
    {
        if (SelectedScripture != null)
        {
            ScriptureList.Remove(SelectedScripture);
            // SaveScriptureListToDb(); // Uncomment when DB is ready
        }
    }

    [RelayCommand]
    private void MoveScriptureUp()
    {
        if (SelectedScripture != null)
        {
            int index = ScriptureList.IndexOf(SelectedScripture);
            if (index > 0)
            {
                ScriptureList.Move(index, index - 1);
                // SaveScriptureListToDb(); // Uncomment when DB is ready
            }
        }
    }

    [RelayCommand]
    private void MoveScriptureDown()
    {
        if (SelectedScripture != null)
        {
            int index = ScriptureList.IndexOf(SelectedScripture);
            if (index < ScriptureList.Count - 1 && index >= 0)
            {
                ScriptureList.Move(index, index + 1);
                // SaveScriptureListToDb(); // Uncomment when DB is ready
            }
        }
    }

    partial void OnSelectedScriptureChanged(ScriptureListItem? value)
    {
        if (value != null)
        {
            // Project the scripture reference
            BibleProjectorVm.Reference = value.Reference;
        }
    }

    // ───────────────────────────────────────────────────────────────────────

    [ObservableProperty] private ObservableCollection<Song> _searchResults = new();

    [ObservableProperty] private string _searchQuery = "";

    [ObservableProperty] private Song? _searchSelectedSong;
    [ObservableProperty] private Song? _songlistSelectedSong;

    [ObservableProperty] private ObservableCollection<Stanza> _currentStanzas = new();

    [ObservableProperty] private ObservableCollection<Songlist> _songlists;

    [ObservableProperty] private Songlist? _selectedSonglist = new();
    [ObservableProperty] private ObservableCollection<Song>? _selectedSonglistSongs = new();
    [ObservableProperty] private string _selectedSonglistTitle = "";

    [ObservableProperty] private Stanza? _selectedStanza;
    [ObservableProperty] private string _stanzaLangFgColor = "#60a5fa";
    [ObservableProperty] private string _stanzaNameFgColor = "#aaa";

    [ObservableProperty] private string _currentStanzaText;
    [ObservableProperty] private string _previousStanzaText;
    [ObservableProperty] private string _nextStanzaText;

    [ObservableProperty] private bool _endingSkipped = true;
    [ObservableProperty] private string _endingSkippedBtnTxt = "Ending skipped";
    [ObservableProperty] private string _endingSkippedBtnClr = "#60a5fa";

    [ObservableProperty]
    private ObservableCollection<DisplayItemModel> _availableDisplays = new();

    public void LoadDisplays(Window window)
    {
        AvailableDisplays.Clear();
        var screens = window.Screens.All;

        for (int i = 0; i < screens.Count; i++)
        {
            var screen = screens[i];
            AvailableDisplays.Add(new DisplayItemModel
            {
                ScreenIndex = i,
                Name = !string.IsNullOrEmpty(screen.DisplayName) ? screen.DisplayName : $"Display {i + 1}",
                IsPrimary = screen.IsPrimary,
                Resolution = $"{screen.Bounds.Width}x{screen.Bounds.Height}",
                IsEnabled = true
            });
        }
    }

    [RelayCommand]
    private async Task EditSong(Song? targetSong)
    {
        if (targetSong == null) return;

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            && desktop.MainWindow is not null)
        {

            var dialog = new NewSongWindow(targetSong.Id);

            var result = await dialog.ShowDialog<bool>(desktop.MainWindow);
            if (result)
            {
                OnSearchQueryChanged(SearchQuery);
            }
        }
    }

    [RelayCommand]
    private async Task PrintSonglist()
    {
        if (SelectedSonglist == null ||
            Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.MainWindow is null) return;

        var dialog = new PrintCopiesDialog();
        var copies = await dialog.ShowDialog<int>(desktop.MainWindow);

        if (copies < 1 || copies > 4) return;

        using var db = new AppDbContext();

        var listItems = db.SonglistItems
            .Where(i => i.SonglistId == SelectedSonglist.Id)
            .OrderBy(i => i.ItemOrder)
            .ToList();

        var rowsHtml = new System.Text.StringBuilder();

        foreach (var item in listItems)
        {
            if (item.ItemType == "section")
            {
                rowsHtml.AppendLine($"<div class='section-title'>{item.SectionTitle?.ToUpper()}</div>");
            }
            else
            {
                var song = db.Songs.Include(s => s.SongTranslations).FirstOrDefault(s => s.Id == item.SongId);
                if (song != null)
                {
                    var langOrder = item.CustomLanguageOrder ?? song.DefaultLanguageOrder ?? "CS";
                    var firstLang = langOrder.Split(new[] { ',', ' ' }, System.StringSplitOptions.RemoveEmptyEntries)
                        .FirstOrDefault() ?? "CS";

                    var trans = song.SongTranslations.FirstOrDefault(t =>
                                    t.Language.Equals(firstLang, System.StringComparison.OrdinalIgnoreCase))
                                ?? song.SongTranslations.FirstOrDefault();

                    string num = song.SongNumber?.ToString() ?? "";
                    string title = trans?.Title ?? "Untitled";
                    string key = song.DefaultKey ?? "";

                    rowsHtml.AppendLine($@"
                    <div class='song-row'>
                        <span class='num'>{num}</span>
                        <span class='name'>{title}</span>
                        <span class='key'>{key}</span>
                    </div>");
                }
            }
        }

        var quadsHtml = new System.Text.StringBuilder();

        for (int i = 1; i <= 4; i++)
        {
            string borderStyle = "";
            if (i == 1) borderStyle = "border-right: 1px dashed #999; border-bottom: 1px dashed #999;";
            if (i == 2) borderStyle = "border-bottom: 1px dashed #999;";
            if (i == 3) borderStyle = "border-right: 1px dashed #999;";

            quadsHtml.AppendLine($"<div class='quad' style='{borderStyle}'>");

            if (i <= copies)
            {
                quadsHtml.AppendLine($"<div class='title'>{SelectedSonglist.Title}</div>");
                quadsHtml.AppendLine(rowsHtml.ToString());
            }

            quadsHtml.AppendLine("</div>");
        }

        string fullHtml = $@"<!DOCTYPE html>
    <html>
    <head>
    <meta charset='utf-8'>
    <style>
        @page {{ size: A4; margin: 0; }}
        body {{ margin: 0; padding: 0; width: 210mm; height: 297mm; font-family: 'Segoe UI', sans-serif; display: flex; flex-wrap: wrap; }}
        .quad {{ width: 50%; height: 50%; box-sizing: border-box; padding: 15mm; overflow: hidden; }}
        .title {{ font-weight: bold; font-size: 16pt; margin-bottom: 15pt; text-align: center; }}
        .section-title {{ font-weight: bold; font-size: 10pt; margin-top: 10pt; margin-bottom: 4pt; color: #555; border-bottom: 1px solid #ccc; }}
        .song-row {{ display: flex; font-size: 11pt; margin-bottom: 5pt; }}
        .num {{ font-weight: bold; width: 35px; }}
        .name {{ flex-grow: 1; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; padding-right: 10px; }}
        .key {{ font-weight: bold; color: #777; width: 30px; text-align: right; }}
    </style>
    </head>
    <body>
    {quadsHtml}
    <script>
        // instantly pop open the print dialog the second the browser loads the file
        window.onload = function() {{ window.print(); }}
    </script>
    </body>
    </html>";

        string tempFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "RevivalSong_Print.html");
        System.IO.File.WriteAllText(tempFile, fullHtml);

        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(tempFile)
                { UseShellExecute = true });
        }
        catch
        {
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform
                    .Linux))
            {
                System.Diagnostics.Process.Start("xdg-open", tempFile);
            }
        }
    }

    [RelayCommand]
    private async Task OpenNewSongDialog()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            && desktop.MainWindow is not null)
        {
            var dialog = new NewSongWindow();

            var result = await dialog.ShowDialog<bool>(desktop.MainWindow);
            if (result)
            {
                OnSearchQueryChanged(SearchQuery);
            }
        }
    }

    public void UpdateTabProjection(int tabIndex, Window mainWindow)
    {
        _currentTabIndex = tabIndex;

        // Timer is independent of main projection
        if (tabIndex == 2)
        {
            OpenTimerProjector(mainWindow);
        }
        else
        {
            CloseTimerProjector();
        }

        // Main Projection
        if (!IsProjectionActive)
        {
            CloseSongsProjector();
            CloseBibleProjector();
            return;
        }

        if (tabIndex == 1) // Bible
        {
            CloseSongsProjector();
            OpenBibleProjector(mainWindow);
        }
        else // Default to Songs for other tabs (Songs, Settings, etc.)
        {
            CloseBibleProjector();
            OpenSongsProjector(mainWindow);
        }
    }

    [RelayCommand]
    private void ToggleProjector()
    {
        IsProjectionActive = !IsProjectionActive;

        var desktop = App.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        if (desktop?.MainWindow == null) return;

        UpdateTabProjection(_currentTabIndex, desktop.MainWindow);
    }

    private void OpenSongsProjector(Window mainWindow)
    {
        if (_projectorWindows.Any()) return;

        var screens = mainWindow.Screens.All;
        var activeDisplays = AvailableDisplays.Where(d => d.IsEnabled).ToList();

        foreach (var display in activeDisplays)
        {
            if (screens.Count > display.ScreenIndex)
            {
                var window = new ProjectorWindow { DataContext = _projectorVm };
                window.Position = screens[display.ScreenIndex].WorkingArea.TopLeft;
                window.Show();
                _projectorWindows.Add(window);
            }
        }
    }

    private void CloseSongsProjector()
    {
        foreach (var window in _projectorWindows)
        {
            window.Close();
        }
        _projectorWindows.Clear();
    }

    private void OpenBibleProjector(Window mainWindow)
    {
        if (_bibleProjectorWindows.Any()) return;

        var screens = mainWindow.Screens.All;
        var activeDisplays = AvailableDisplays.Where(d => d.IsEnabled).ToList();

        foreach (var display in activeDisplays)
        {
            if (screens.Count > display.ScreenIndex)
            {
                var window = new BibleProjectorWindow { DataContext = BibleProjectorVm };
                window.Position = screens[display.ScreenIndex].WorkingArea.TopLeft;
                window.Show();
                _bibleProjectorWindows.Add(window);
            }
        }
    }

    private void CloseBibleProjector()
    {
        foreach (var window in _bibleProjectorWindows)
        {
            window.Close();
        }
        _bibleProjectorWindows.Clear();
    }

    [RelayCommand] private void EndingSkippedBtn()
    {
        int previousIndex = SelectedStanza != null ? CurrentStanzas.IndexOf(SelectedStanza) : -1;

        if (EndingSkipped)
        {
            EndingSkipped = false;
            EndingSkippedBtnTxt = "Skip ending";
            EndingSkippedBtnClr = "#333";
        } else if (!EndingSkipped)
        {
            EndingSkipped = true;
            EndingSkippedBtnTxt = "Ending skipped";
            EndingSkippedBtnClr = "#60a5fa";
        }

        if (SearchSelectedSong != null)
        {
            OnSearchSelectedSongChanged(SearchSelectedSong);
        }
        else if (SonglistSelectedSong != null)
        {
            OnSonglistSelectedSongChanged(SonglistSelectedSong);
        }

        if (previousIndex >= 0 && previousIndex < CurrentStanzas.Count)
        {
            SelectedStanza = CurrentStanzas[previousIndex];
        }
    }

    [RelayCommand]
    private async Task MakeNewSonglist()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            && desktop.MainWindow is not null)
        {
            var dialog = new NewSonglistWindow();

            string? songlistName = await dialog.ShowDialog<string?>(desktop.MainWindow);

            if (!string.IsNullOrWhiteSpace(songlistName))
            {
                using var db = new AppDbContext();

                var songlist = new Songlist();
                songlist.Title = songlistName;

                db.Songlists.Add(songlist);
                await db.SaveChangesAsync();

                Songlists.Add(songlist);
                SelectedSonglist = songlist;
            }
        }
    }

    [RelayCommand]
    private void LoadDB()
    {
        using var db = new AppDbContext();

        var songsFromDb = db.Songs.Take(50).ToList();

        SearchResults = new ObservableCollection<Song>(songsFromDb);
    }

    [RelayCommand]
    private void PreviousStanza()
    {
        if (PreviousStanzaText != "")
        {
            var index = CurrentStanzas.IndexOf(SelectedStanza);

            SelectedStanza = CurrentStanzas[index - 1].Disabled == "True" ?
                CurrentStanzas[index - 2] :
                CurrentStanzas[index - 1];
        }
    }
    
    [RelayCommand]
    private void NextStanza()
    {
        if (NextStanzaText != "")
        {
            var index = CurrentStanzas.IndexOf(SelectedStanza);

            SelectedStanza = CurrentStanzas[index + 1].Disabled == "True" ?
                CurrentStanzas[index + 2] :
                CurrentStanzas[index + 1];
        }
    }

    [RelayCommand]
        private void NextSong()
        {
            if (SelectedSonglistSongs[SelectedSonglistSongs.IndexOf(SonglistSelectedSong) + 1] != null)
            {
                SonglistSelectedSong = SelectedSonglistSongs[SelectedSonglistSongs.IndexOf(SonglistSelectedSong) + 1];
            }
        }

    [RelayCommand]
    public void SaveSonglistOrder()
    {
        if (SelectedSonglist == null || SelectedSonglistSongs == null) return;

        using var db = new AppDbContext();


        var oldDbItems = db.SonglistItems.Where(i => i.SonglistId == SelectedSonglist.Id);
        db.SonglistItems.RemoveRange(oldDbItems);


        for (int i = 0; i < SelectedSonglistSongs.Count; i++)
        {
            var newItem = new SonglistItem
            {
                SonglistId = SelectedSonglist.Id,
                SongId = SelectedSonglistSongs[i].Id,
                ItemOrder = i
            };
            db.SonglistItems.Add(newItem);
        }

        db.SaveChanges();
    }

    [RelayCommand]
    private void PreviousSong() {
        if (SelectedSonglistSongs[SelectedSonglistSongs.IndexOf(SonglistSelectedSong) - 1] != null)
        {
            SonglistSelectedSong = SelectedSonglistSongs[SelectedSonglistSongs.IndexOf(SonglistSelectedSong) - 1];
        }
    }

    partial void OnSearchQueryChanged(string val)
    {
        using var db = new AppDbContext();

        if (string.IsNullOrWhiteSpace(val))
        {
            var allSongs = db.Songs.Include(s => s.SongTranslations).OrderBy(s => s.SongNumber).ToList();
            SearchResults = new ObservableCollection<Song>(allSongs);
            return;
        }

        var filteredSongs = db.Songs.Include(s => s.SongTranslations).Where(s => s.SongNumber.ToString().Contains(val)
        || s.SongTranslations.Any(t => t.Title.Contains(val))).ToList().OrderBy(s => s.SongNumber);

        SearchResults = new ObservableCollection<Song>(filteredSongs);
    }

    partial void OnSearchSelectedSongChanged(Song? val)
    {
        using var db = new AppDbContext();

        if (val == null) return;

        var languageOrderString = val.DefaultLanguageOrder;
        var languageTags = languageOrderString.Split(new[] { ' ', ',' }, System.StringSplitOptions.RemoveEmptyEntries);
        var arrangedLanguages = new List<SongTranslation>();

        foreach (var tag in languageTags)
        {
            var matchingTranslation = db.SongTranslations.Where(s => s.SongId == val.Id && s.Language.ToLower() == tag.ToLower()).FirstOrDefault();

            if (matchingTranslation != null)
            {
                arrangedLanguages.Add(matchingTranslation);
            }
        }

        if (!arrangedLanguages.Any() && val.SongTranslations != null)
        {
            arrangedLanguages.AddRange(val.SongTranslations);
        }

        var rawStanzas = db.Stanzas.Where(s => s.SongId == val.Id).ToList();
        var allArrangedStanzas = new List<Stanza>();

        foreach (var lang in arrangedLanguages)
        {
            if (string.IsNullOrWhiteSpace(lang.StanzaOrder))
            {
                var defaultStanzas = rawStanzas
                    .Where(s => s.Language.Equals(lang.Language, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(s => s.SortOrder);
                foreach (var st in defaultStanzas)
                {
                    allArrangedStanzas.Add(st.Clone());
                }
                continue;
            }

            var orderTags = lang.StanzaOrder.Split(new[] { ' ', ',' }, System.StringSplitOptions.RemoveEmptyEntries);

            foreach (var tag in orderTags)
            {
                if (tag.Length < 1) continue;

                char typeLetter = char.ToLower(tag[0]);
                int sectionNumber = 1;
                
                if (tag.Length > 1)
                {
                    int.TryParse(tag.Substring(1), out sectionNumber);
                }

                string targetType = typeLetter switch
                {
                    'v' => "verse",
                    'c' => "chorus",
                    'b' => "bridge",
                    'e' => "ending",
                    _ => ""
                };

                var matchingStanza = rawStanzas.FirstOrDefault(s =>
                    s.SectionType.Equals(targetType, StringComparison.OrdinalIgnoreCase) &&
                    (s.SectionNumber == sectionNumber || s.SectionNumber == null) &&
                    s.Language.Equals(lang.Language, StringComparison.OrdinalIgnoreCase));

                if (matchingStanza != null)
                {
                    var clone = matchingStanza.Clone();

                    if (clone.SectionType.Equals("ending", StringComparison.OrdinalIgnoreCase) &&
                        arrangedLanguages.IndexOf(lang) != arrangedLanguages.Count - 1 &&
                        EndingSkipped)
                    {
                        clone.BgColor = "#555";
                        clone.LangFgColor = "#888";
                        clone.NameFgColor = "#888";
                        clone.Disabled = "True";
                    }

                    allArrangedStanzas.Add(clone);
                }
            }
        }

        CurrentStanzas = new ObservableCollection<Stanza>(allArrangedStanzas);
    }
    partial void OnSonglistSelectedSongChanged(Song? val)
    {
        SearchSelectedSong = null;
        using var db = new AppDbContext();

        if (val == null) return;

        var languageOrderString = val.DefaultLanguageOrder;
        var languageTags = languageOrderString.Split(new[] { ' ', ',' }, System.StringSplitOptions.RemoveEmptyEntries);
        var arrangedLanguages = new List<SongTranslation>();

        foreach (var tag in languageTags)
        {
            var matchingTranslation = db.SongTranslations.Where(s => s.SongId == val.Id && s.Language.ToLower() == tag.ToLower()).FirstOrDefault();

            if (matchingTranslation != null)
            {
                arrangedLanguages.Add(matchingTranslation);
            }
        }

        if (!arrangedLanguages.Any() && val.SongTranslations != null)
        {
            arrangedLanguages.AddRange(val.SongTranslations);
        }

        var rawStanzas = db.Stanzas.Where(s => s.SongId == val.Id).ToList();
        var allArrangedStanzas = new List<Stanza>();

        foreach (var lang in arrangedLanguages)
        {
            if (string.IsNullOrWhiteSpace(lang.StanzaOrder))
            {
                var defaultStanzas = rawStanzas
                    .Where(s => s.Language.Equals(lang.Language, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(s => s.SortOrder);
                foreach (var st in defaultStanzas)
                {
                    allArrangedStanzas.Add(st.Clone());
                }
                continue;
            }

            var orderTags = lang.StanzaOrder.Split(new[] { ' ', ',' }, System.StringSplitOptions.RemoveEmptyEntries);

            foreach (var tag in orderTags)
            {
                if (tag.Length < 1) continue;

                char typeLetter = char.ToLower(tag[0]);
                int sectionNumber = 1;

                if (tag.Length > 1)
                {
                    int.TryParse(tag.Substring(1), out sectionNumber);
                }

                string targetType = typeLetter switch
                {
                    'v' => "verse",
                    'c' => "chorus",
                    'b' => "bridge",
                    'e' => "ending",
                    _ => ""
                };

                var matchingStanza = rawStanzas.FirstOrDefault(s =>
                    s.SectionType.Equals(targetType, StringComparison.OrdinalIgnoreCase) &&
                    (s.SectionNumber == sectionNumber || s.SectionNumber == null) &&
                    s.Language.Equals(lang.Language, StringComparison.OrdinalIgnoreCase));


                if (matchingStanza != null)
                {
                    var clone = matchingStanza.Clone();

                    if (clone.SectionType.Equals("ending", StringComparison.OrdinalIgnoreCase) &&
                        arrangedLanguages.IndexOf(lang) != arrangedLanguages.Count - 1 &&
                        EndingSkipped)
                    {
                        clone.BgColor = "#555";
                        clone.LangFgColor = "#888";
                        clone.NameFgColor = "#888";
                        clone.Disabled = "True";
                    }

                    allArrangedStanzas.Add(clone);
                }
            }
        }

        CurrentStanzas = new ObservableCollection<Stanza>(allArrangedStanzas);
    }

    partial void OnSelectedStanzaChanged(Stanza? value)
    {
        CurrentStanzaText = value?.Lyrics ?? "";

        int index = value != null ? CurrentStanzas.IndexOf(value) : -1;

        if (index > 0)
        {
            PreviousStanzaText = CurrentStanzas[index - 1].Disabled == "True" ?
                CurrentStanzas[index - 2].Lyrics :
                CurrentStanzas[index - 1].Lyrics;
        } 
        else 
        { 
            PreviousStanzaText = ""; 
        }
        
        if (index >= 0 && index < CurrentStanzas.Count - 1)
        {
            NextStanzaText = CurrentStanzas[index + 1].Disabled == "True" ?
                CurrentStanzas[index + 2].Lyrics :
                CurrentStanzas[index + 1].Lyrics;
        } 
        else 
        { 
            NextStanzaText = ""; 
        }

        _projectorVm.Lyrics = value?.Lyrics ?? "";

        var activeSong = SearchSelectedSong ?? SonglistSelectedSong;
        _projectorVm.Title = activeSong?.PrimaryTitle ?? "";
        _projectorVm.SongNumber = activeSong?.SongNumber.ToString() ?? "";
        _projectorVm.KeyText = activeSong?.DefaultKey ?? "";

        if (value != null)
        {
            int currentPosition = 0;
            int languageTotal = 0;
            bool foundTarget = false;

            foreach (var s in CurrentStanzas)
            {
                if (s.Language == value.Language)
                {
                    if (s.Disabled != "True")
                    {
                        languageTotal++;

                        if (!foundTarget) currentPosition++;
                    }

                    if (s == value) foundTarget = true;
                }
            }

            _projectorVm.CounterText = value.Disabled == "True" ? "" : $"{currentPosition}/{languageTotal}";
        }
        else
        {
            _projectorVm.CounterText = "";
        }
    }

    partial void OnSelectedSonglistChanged(Songlist? value)
    {
        using var db = new AppDbContext();

        SelectedSonglistTitle = value.Title;

        var selSonglistItems = new List<SonglistItem>();

        foreach (var item in db.SonglistItems)
        {
            if (item.SonglistId == value.Id) selSonglistItems.Add(item);
        }

        var sortedSonglistItems = selSonglistItems.OrderBy(s => s.ItemOrder);

        var songlistSongs = new List<Song>();

        foreach (var item in sortedSonglistItems)
        {
            songlistSongs.Add(db.Songs.Include(s => s.SongTranslations).Where(s => s.Id == item.SongId).FirstOrDefault().Clone());
        }

        SelectedSonglistSongs = new ObservableCollection<Song>(songlistSongs);
    }

    [RelayCommand]
    private void AddSongToSonglist()
    {
        if (SearchSelectedSong == null || SelectedSonglist == null) return;

        using var db = new AppDbContext();

        if (SonglistSelectedSong != null)
        {
            SelectedSonglistSongs?.Insert(SelectedSonglistSongs.IndexOf(SonglistSelectedSong) + 1, SearchSelectedSong);
        }
        else
        {
            SelectedSonglistSongs?.Add(SearchSelectedSong.Clone());
        }

        var oldDbItems = db.SonglistItems.Where(i => i.SonglistId == SelectedSonglist.Id);
        db.SonglistItems.RemoveRange(oldDbItems);

        for (int i = 0; i < SelectedSonglistSongs?.Count; i++)
        {
            var newItem = new SonglistItem
            {
                SonglistId = SelectedSonglist.Id,
                SongId = SelectedSonglistSongs[i].Id,
                ItemOrder = i
            };
            db.SonglistItems.Add(newItem);
        }

        db.SaveChanges();
    }

    public MainViewModel()
    {
        using var db = new AppDbContext();
        OnSearchQueryChanged("");
        Songlists = new ObservableCollection<Songlist>(db.Songlists);
        
        LoadBibleTranslations();
    }
}