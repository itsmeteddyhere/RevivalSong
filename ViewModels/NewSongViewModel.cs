using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using RevivalSong.Models;

namespace RevivalSong.ViewModels;

public partial class StanzaEditModel : ViewModelBase
{
    public List<string> SectionOptions { get; } = new() { "verse", "chorus", "bridge", "pre_chorus", "ending", "custom" };

    [ObservableProperty] private string _sectionType = "verse";
    [ObservableProperty] private int _sectionNumber = 1;
    [ObservableProperty] private string _label = "";
    [ObservableProperty] private string _keyChange = "";
    [ObservableProperty] private bool _splitParenthesis = false;
    [ObservableProperty] private string _lyrics = "";

    public Action<StanzaEditModel>? OnRemove { get; set; }

    [RelayCommand]
    private void Remove() => OnRemove?.Invoke(this);
}

public partial class LanguageEditModel : ViewModelBase
{
    [ObservableProperty] private string _langCode = "";
    [ObservableProperty] private string _title = "";
    [ObservableProperty] private string _altTitle = "";
    [ObservableProperty] private string _translator = "";
    [ObservableProperty] private string _stanzaOrder = "";

    [ObservableProperty] private ObservableCollection<StanzaEditModel> _stanzas = new();

    [RelayCommand]
    private void AddStanza()
    {
        var newStanza = new StanzaEditModel();
        newStanza.OnRemove = (s) => Stanzas.Remove(s);
        Stanzas.Add(newStanza);
    }
}

public partial class NewSongViewModel : ViewModelBase
{
    private readonly Window _window;
    private readonly int? _songId;


    [ObservableProperty] private int? _songNumber;
    [ObservableProperty] private string _songType = "worship";
    [ObservableProperty] private string _langOrder = "CS, EN";
    [ObservableProperty] private int? _gsn;
    [ObservableProperty] private string _grk = "";
    [ObservableProperty] private string _defaultKey = "";
    [ObservableProperty] private string _timeSignature = "";
    [ObservableProperty] private int? _bpm;
    [ObservableProperty] private string _author = "";
    [ObservableProperty] private string _ccli = "";
    [ObservableProperty] private int? _year;
    [ObservableProperty] private string _assembly = "";
    [ObservableProperty] private string _scripture = "";
    [ObservableProperty] private string _copyright = "© ";
    [ObservableProperty] private string _createdBy = "";

    [ObservableProperty] private ObservableCollection<LanguageEditModel> _languageTabs = new();

    public NewSongViewModel(Window window, int? songId = null)
    {
        _window = window;
        _songId = songId;

        if (_songId.HasValue)
        {
            _window.Title = "Edit Song";
            LoadSong(_songId.Value);
        }
        else
        {
            _window.Title = "Add Song";
            UpdateTabs(); // just generate blank ones
        }
    }

    private void LoadSong(int id)
    {
        using var db = new AppDbContext();

        // grab the song and all its children in one shot
        var song = db.Songs
            .Include(s => s.SongTranslations)
            .Include(s => s.Stanzas)
            .FirstOrDefault(s => s.Id == id);

        if (song == null) return;

        // map the metadata
        SongNumber = song.SongNumber;
        SongType = song.SongType ?? "worship";
        LangOrder = song.DefaultLanguageOrder ?? "CS, EN";
        Gsn = song.Gsn;
        Grk = song.Grk;
        DefaultKey = song.DefaultKey;
        TimeSignature = song.TimeSignature;
        Bpm = song.Bpm;
        Author = song.Author;
        Ccli = song.CcliNumber;
        Year = song.Year;
        Assembly = song.Assembly;
        Scripture = song.Scripture;
        Copyright = song.Copyright;
        CreatedBy = song.CreatedBy;

        // map the language tabs and stanzas
        foreach (var trans in song.SongTranslations)
        {
            var tab = new LanguageEditModel
            {
                LangCode = trans.Language?.ToUpper() ?? "",
                Title = trans.Title ?? "",
                AltTitle = trans.AltTitle ?? "",
                Translator = trans.Translator ?? "",
                StanzaOrder = trans.StanzaOrder ?? ""
            };

            var stanzas = song.Stanzas
                .Where(s => s.Language.Equals(trans.Language, StringComparison.OrdinalIgnoreCase))
                .OrderBy(s => s.SortOrder);

            foreach (var st in stanzas)
            {
                var sm = new StanzaEditModel
                {
                    SectionType = st.SectionType ?? "verse",
                    SectionNumber = st.SectionNumber ?? 1,
                    Label = st.Label ?? "",
                    KeyChange = st.KeyChange ?? "",
                    SplitParenthesis = st.SplitParenthesis == 1,
                    Lyrics = st.Lyrics ?? ""
                };
                sm.OnRemove = (s) => tab.Stanzas.Remove(s);
                tab.Stanzas.Add(sm);
            }

            LanguageTabs.Add(tab);
        }
    }

    [RelayCommand]
    private void UpdateTabs()
    {
        var langs = LangOrder.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

        var toRemove = LanguageTabs.Where(t => !langs.Contains(t.LangCode, StringComparer.OrdinalIgnoreCase)).ToList();
        foreach (var t in toRemove) LanguageTabs.Remove(t);

        foreach (var lang in langs)
        {
            if (!LanguageTabs.Any(t => t.LangCode.Equals(lang, StringComparison.OrdinalIgnoreCase)))
            {
                var newTab = new LanguageEditModel { LangCode = lang.ToUpper() };
                newTab.AddStanzaCommand.Execute(null);
                LanguageTabs.Add(newTab);
            }
        }
    }

    [RelayCommand]
    private void SaveSong()
    {
        using var db = new AppDbContext();

        Song targetSong;

        if (_songId.HasValue)
        {
            // EDIT MODE: grab the existing song and nuke its old children
            targetSong = db.Songs
                .Include(s => s.SongTranslations)
                .Include(s => s.Stanzas)
                .First(s => s.Id == _songId.Value);

            db.SongTranslations.RemoveRange(targetSong.SongTranslations);
            db.Stanzas.RemoveRange(targetSong.Stanzas);

            targetSong.SongTranslations.Clear();
            targetSong.Stanzas.Clear();
        }
        else
        {
            // ADD MODE: create a fresh song
            targetSong = new Song
            {
                SongTranslations = new List<SongTranslation>(),
                Stanzas = new List<Stanza>()
            };
            db.Songs.Add(targetSong);
        }

        // map the UI data back into the target song
        targetSong.SongNumber = SongNumber;
        targetSong.SongType = SongType;
        targetSong.DefaultLanguageOrder = LangOrder;
        targetSong.Gsn = Gsn;
        targetSong.Grk = Grk;
        targetSong.DefaultKey = DefaultKey;
        targetSong.TimeSignature = TimeSignature;
        targetSong.Bpm = Bpm;
        targetSong.Author = Author;
        targetSong.CcliNumber = Ccli;
        targetSong.Year = Year;
        targetSong.Assembly = Assembly;
        targetSong.Scripture = Scripture;
        targetSong.Copyright = Copyright;

        if (!_songId.HasValue) targetSong.CreatedBy = CreatedBy;
        targetSong.UpdatedBy = CreatedBy; // update the last modified author
        targetSong.UpdatedAt = DateTime.Now;

        // rebuild all translations and stanzas from the UI tabs
        foreach (var tab in LanguageTabs)
        {
            targetSong.SongTranslations.Add(new SongTranslation
            {
                Language = tab.LangCode,
                Title = tab.Title,
                AltTitle = tab.AltTitle,
                Translator = tab.Translator,
                StanzaOrder = tab.StanzaOrder,
                TitleClean = tab.Title.ToLower()
            });

            for (int i = 0; i < tab.Stanzas.Count; i++)
            {
                var s = tab.Stanzas[i];
                targetSong.Stanzas.Add(new Stanza
                {
                    Language = tab.LangCode,
                    SectionType = s.SectionType,
                    SectionNumber = s.SectionNumber,
                    Label = s.Label,
                    KeyChange = s.KeyChange,
                    SplitParenthesis = s.SplitParenthesis ? 1 : 0,
                    Lyrics = s.Lyrics,
                    SortOrder = i + 1
                });
            }
        }

        db.SaveChanges();
        _window.Close(true);
    }
}