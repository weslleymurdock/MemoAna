using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemoAna.Application.Abstract.Services;

namespace MemoAna.Presentation.ViewModels;

public partial class MainViewModel(IAudioService audioService) : ObservableObject
{
    [ObservableProperty]
    public partial string Version { get; set; } = "v1.0.0-dev";

    [RelayCommand]
    private async Task StartMenuMusicAsync()
    {
        try
        {
            await audioService.PlayMainTitleAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao tocar música do menu: {ex.Message}");
        }
    }

    [RelayCommand]
    private static async Task Play()
    {
        await Shell.Current.GoToAsync("GameSelectionPage");
    }
    
    [RelayCommand]
    private static async Task Options()
    {
        await Shell.Current.GoToAsync("OptionsPage");
    }

    [RelayCommand]
    private static async Task About()
    {
        await Shell.Current.GoToAsync("AboutPage");
    }
}
