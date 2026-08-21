#pragma warning disable CA1416 // Validar a compatibilidade da plataforma
using MemoAna.Application.Common.Abstract.Repositories;

namespace MemoAna.Presentation.ViewModels;

public partial class GameSelectionViewModel(IThemeService service) : ViewModelBase
{
    [ObservableProperty]
    public partial ObservableCollection<CardThemeManifestDto> Themes { get; set; } = [];

    [ObservableProperty]
    public partial CardThemeManifestDto? SelectedTheme { get; set; } = null!;

    [RelayCommand]
    private async Task LoadThemesAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            IReadOnlyCollection<CardThemeManifestDto> list = await service.GetThemesAsync();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Themes.Clear();
                foreach (CardThemeManifestDto theme in list)
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
#pragma warning restore CA1416 // Validar a compatibilidade da plataforma