using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using RevivalSong.Models;

namespace RevivalSong.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] private ObservableCollection<Song> _searchResults = new();

    [ObservableProperty] private string _searchQuery = "";

    [ObservableProperty] private Song? _selectedSearchSong;

    [ObservableProperty] private ObservableCollection<Stanza> _currentStanzas = new();

    [ObservableProperty] private Stanza? _selectedStanza;
    
    [ObservableProperty] private string _currentStanzaText;
    [ObservableProperty] private string _previousStanzaText;
    [ObservableProperty] private string _nextStanzaText;
    
    [ObservableProperty] private string _endingState = "Ending skipped";

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
            SelectedStanza = CurrentStanzas[CurrentStanzas.IndexOf(SelectedStanza) - 1];
        }
    }
    
    [RelayCommand]
    private void NextStanza()
    {
        if (NextStanzaText != "")
        {
            SelectedStanza = CurrentStanzas[CurrentStanzas.IndexOf(SelectedStanza) + 1];
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

    partial void OnSelectedSearchSongChanged(Song? val)
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
            string stanzaOrderString = lang.StanzaOrder ?? "";
            var orderTags = stanzaOrderString.Split(new[] { ' ', ',' }, System.StringSplitOptions.RemoveEmptyEntries);

            foreach (var tag in orderTags)
            {
                if (tag.Length < 2) continue;

                char typeLetter = char.ToLower(tag[0]);
                if (!int.TryParse(tag.Substring(1), out int sectionNumber)) continue;

                string targetType = typeLetter switch
                {
                    'v' => "verse",
                    'c' => "chorus",
                    'b' => "bridge",
                    'e' => "ending",
                    _ => ""
                };

                var matchingStanza = rawStanzas.Where(s =>
                    s.SectionType.Equals(targetType, StringComparison.OrdinalIgnoreCase) &&
                    s.SectionNumber == sectionNumber && s.Language.Equals(lang.Language, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                if (matchingStanza != null)
                {
                    allArrangedStanzas.Add(matchingStanza.Clone());
                }
            }
        }



        CurrentStanzas = new ObservableCollection<Stanza>(allArrangedStanzas);
    }

    partial void OnSelectedStanzaChanged(Stanza? value)
    {
        CurrentStanzaText = value?.Lyrics;

        if (CurrentStanzas.IndexOf(value) != 0)
        {
            PreviousStanzaText = CurrentStanzas[CurrentStanzas.IndexOf(value) - 1].Lyrics;
        } else { PreviousStanzaText = ""; }
        
        if (CurrentStanzas.IndexOf(value) != CurrentStanzas.Count - 1)
        {
            NextStanzaText = CurrentStanzas[CurrentStanzas.IndexOf(value) + 1].Lyrics;
        } else { NextStanzaText = ""; }
    }

    public MainViewModel()
    {
        OnSearchQueryChanged("");
    }
}