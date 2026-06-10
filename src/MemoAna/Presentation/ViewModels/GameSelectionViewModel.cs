using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemoAna.Application.Abstract.Repositories;
using MemoAna.Domain.Entities;
using System.Collections.ObjectModel;

namespace MemoAna.Presentation.ViewModels;

public partial class GameSelectionViewModel(IRepository repository) : BaseViewModel
{
    [ObservableProperty]
    public partial ObservableCollection<CardThemeManifestEntity> Themes { get; set; } = [];

    [ObservableProperty]
    public partial CardThemeManifestEntity? SelectedTheme { get; set; } = null!;

    [RelayCommand]
    private async Task LoadThemesAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            var list = await repository.GetAsync<CardThemeManifestEntity>();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Themes.Clear();
                foreach (var theme in list)
                {
                    Themes.Add(theme);
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao carregar temas: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task Play(string difficultyParam)
    {
        if (SelectedTheme == null) return;

        var navigationParameter = new Dictionary<string, object>
        {
            { "Theme", SelectedTheme.ThemeName },
            { "Difficulty", difficultyParam }
        };

        await Shell.Current.GoToAsync("GamePage", true, [.. navigationParameter]);
    }
}