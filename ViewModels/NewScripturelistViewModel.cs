using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RevivalSong.Models;
using RevivalSong.Views;

namespace RevivalSong.ViewModels;

public partial class NewScripturelistViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OkClickCommand))]
    private string _scripturelistName = "";

    public Action<string?>? CloseAction { get; set; }

    private bool CanExecuteOk => !string.IsNullOrWhiteSpace(ScripturelistName);

    [RelayCommand(CanExecute = nameof(CanExecuteOk))]
    private void OkClick()
    {
        CloseAction?.Invoke(ScripturelistName.Trim());
    }

    [RelayCommand]
    private void CancelClick()
    {
        CloseAction?.Invoke(null);
    }
}