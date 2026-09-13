using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RevivalSong.Models;
using RevivalSong.Views;

namespace RevivalSong.ViewModels;

public partial class NewSonglistViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OkClickCommand))]
    private string _songlistName = "";

    public Action<string?>? CloseAction { get; set; }

    private bool CanExecuteOk => !string.IsNullOrWhiteSpace(SonglistName);

    [RelayCommand(CanExecute = nameof(CanExecuteOk))]
    private void OkClick()
    {
        CloseAction?.Invoke(SonglistName.Trim());
    }

    [RelayCommand]
    private void CancelClick()
    {
        CloseAction?.Invoke(null);
    }
}