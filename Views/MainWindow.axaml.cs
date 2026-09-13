using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using RevivalSong.Models;
using RevivalSong.ViewModels;

namespace RevivalSong.Views;

public partial class MainWindow : Window
{
    // The song the user pressed down on — candidate for dragging
    private Song? _pressedSong;
    // The pointer args captured at press time, needed for DoDragDropAsync
    private PointerPressedEventArgs? _pressedArgs;
    // The song actively being dragged (set once drag threshold is exceeded)
    private Song? _draggedSong;
    // Whether a drag is currently in flight
    private bool _dragging;

    // Minimum pixels of movement before we start a drag
    private const double DragThreshold = 5.0;

    // Timer tab is the 3rd tab (0-indexed)
    private const int TimerTabIndex = 2;

    public MainWindow()
    {
        InitializeComponent();

        // Load display list once the window is on screen
        Opened += (_, _) =>
        {
            if (DataContext is not MainViewModel vm) return;
            vm.LoadDisplays(this);

            // Watch for display-selector changes while on the Timer tab
            vm.PropertyChanged += OnVmPropertyChanged;
        };

        // Clean up timer projector when main window closes
        Closing += (_, _) =>
        {
            if (DataContext is MainViewModel vm)
                vm.CloseTimerProjector();
        };

        // Hook tab switching
        var tabs = this.FindControl<TabControl>("MainTabControl")!;
        tabs.SelectionChanged += OnTabSelectionChanged;

        // Drag-and-drop on songlist
        var box = this.FindControl<ListBox>("SonglistBox")!;
        box.AddHandler(PointerPressedEvent,  OnSongPointerPressed,  RoutingStrategies.Tunnel, handledEventsToo: true);
        box.AddHandler(PointerMovedEvent,    OnSongPointerMoved,    RoutingStrategies.Tunnel, handledEventsToo: true);
        box.AddHandler(PointerReleasedEvent, OnSongPointerReleased, RoutingStrategies.Tunnel, handledEventsToo: true);
        DragDrop.AddDragOverHandler(box, OnSongDragOver);
        DragDrop.AddDropHandler(box,     OnSongDrop);
    }

    // ── Timer tab ──────────────────────────────────────────────────────────

    private void OnTabSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not MainViewModel vm) return;
        if (sender is not TabControl tabs) return;

        vm.UpdateTabProjection(tabs.SelectedIndex, this);
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(MainViewModel.SelectedTimerDisplay)) return;
        if (DataContext is not MainViewModel vm) return;

        var tabs = this.FindControl<TabControl>("MainTabControl");
        if (tabs?.SelectedIndex == TimerTabIndex)
            vm.OpenTimerProjector(this);
    }

    // ── Drag initiation ────────────────────────────────────────────────────

    private void OnSongPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;

        _pressedSong = FindSongAtSource(e.Source as Visual);
        _pressedArgs = _pressedSong != null ? e : null;
        _dragging    = false;
    }

    private async void OnSongPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_pressedSong == null || _pressedArgs == null || _dragging) return;

        // Only begin once the pointer has moved far enough
        var origin  = _pressedArgs.GetPosition(this);
        var current = e.GetPosition(this);
        var delta   = current - origin;
        if (Math.Abs(delta.X) < DragThreshold && Math.Abs(delta.Y) < DragThreshold) return;

        _dragging     = true;
        _draggedSong  = _pressedSong;

        var dragData = new DataTransfer();
        dragData.Add(DataTransferItem.CreateText("internal_song_drag"));

        await DragDrop.DoDragDropAsync(_pressedArgs, dragData, DragDropEffects.Move);

        // Reset after drag ends (success or cancel)
        _pressedSong  = null;
        _pressedArgs  = null;
        _draggedSong  = null;
        _dragging     = false;
    }

    private void OnSongPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        // Cancel pending drag if the user releases without moving enough
        _pressedSong = null;
        _pressedArgs = null;
        _dragging    = false;
    }

    // ── Drop handling ──────────────────────────────────────────────────────

    private void OnSongDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = _pressedSong != null || _dragging
            ? DragDropEffects.Move
            : DragDropEffects.None;
        e.Handled = true;
    }

    private void OnSongDrop(object? sender, DragEventArgs e)
    {
        e.Handled = true;

        if (DataContext is not MainViewModel vm) return;

        var draggedSong = _draggedSong;
        if (draggedSong == null || vm.SelectedSonglistSongs == null) return;

        var targetSong = FindSongAtSource(e.Source as Visual);

        if (targetSong != null && !ReferenceEquals(draggedSong, targetSong))
        {
            int oldIndex = vm.SelectedSonglistSongs.IndexOf(draggedSong);
            int newIndex = vm.SelectedSonglistSongs.IndexOf(targetSong);

            if (oldIndex != -1 && newIndex != -1)
            {
                vm.SelectedSonglistSongs.Move(oldIndex, newIndex);
                vm.SaveSonglistOrder();
            }
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    /// Walk up the visual tree from <paramref name="source"/> until we find
    /// a Control whose DataContext is a Song (i.e. a ListBoxItem).
    private static Song? FindSongAtSource(Visual? source)
    {
        var current = source;
        while (current != null)
        {
            if (current is Control { DataContext: Song song })
                return song;
            current = current.GetVisualParent();
        }
        return null;
    }
}