using System;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace RevivalSong.ViewModels;

public partial class ProjectorViewModel : ViewModelBase
{
    [ObservableProperty] private string _songNumber = "";
    [ObservableProperty] private string _title = "";
    [ObservableProperty] private string _keyText = "";
    [ObservableProperty] private string _lyrics = "";
    [ObservableProperty] private string _counterText = "";
}

public partial class BibleProjectorViewModel : ObservableObject
{
    [ObservableProperty] private string _reference = "";
}

public partial class TimerViewModel : ObservableObject
{
    private readonly DispatcherTimer _timer;
    private TimeSpan _remainingTime;
    private TimeSpan _configuredTime;

    [ObservableProperty] private string _timeDisplay = "05:00";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PlayPauseBtnText))]
    private bool _isRunning;

    [ObservableProperty] private decimal _setMinutes = 5;
    [ObservableProperty] private decimal _setSeconds = 0;

    /// Button label flips between Play and Pause
    public string PlayPauseBtnText => IsRunning ? "⏸  Pause" : "▶  Play";

    partial void OnSetMinutesChanged(decimal value) => SyncConfiguredTime();
    partial void OnSetSecondsChanged(decimal value) => SyncConfiguredTime();

    private void SyncConfiguredTime()
    {
        if (IsRunning) return;
        _configuredTime = TimeSpan.FromSeconds((int)SetMinutes * 60 + (int)SetSeconds);
        _remainingTime  = _configuredTime;
        TimeDisplay     = FormatTime(_remainingTime);
    }

    private static string FormatTime(TimeSpan t)
        => $"{(int)t.TotalMinutes:D2}:{t.Seconds:D2}";

    public TimerViewModel()
    {
        _configuredTime = TimeSpan.FromMinutes(5);
        _remainingTime  = _configuredTime;
        TimeDisplay     = FormatTime(_remainingTime);

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (_, _) =>
        {
            if (_remainingTime > TimeSpan.Zero)
            {
                _remainingTime = _remainingTime.Subtract(TimeSpan.FromSeconds(1));
                TimeDisplay    = FormatTime(_remainingTime);
            }
            else
            {
                _timer.Stop();
                IsRunning = false;
            }
        };
    }

    [RelayCommand]
    private void TogglePlayPause()
    {
        if (IsRunning)
        {
            _timer.Stop();
            IsRunning = false;
        }
        else
        {
            if (_remainingTime <= TimeSpan.Zero)
            {
                _remainingTime = _configuredTime;
                TimeDisplay    = FormatTime(_remainingTime);
            }
            _timer.Start();
            IsRunning = true;
        }
    }

    [RelayCommand]
    private void Reset()
    {
        _timer.Stop();
        IsRunning      = false;
        _remainingTime = _configuredTime;
        TimeDisplay    = FormatTime(_remainingTime);
    }
}