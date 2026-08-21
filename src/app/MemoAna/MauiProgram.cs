using CommunityToolkit.Maui;
using MemoAna.Composition.Extensions;
using MemoAna.Presentation.ViewModels;

namespace MemoAna;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp() => 
        MauiApp.CreateBuilder().RunMauiApp<App>(builder =>
        {
            builder.Services.AddTransientWithShellRoute<MainPage, MainViewModel>(nameof(MainPage));
            builder.Services.AddTransientWithShellRoute<OptionsPage, OptionsViewModel>(nameof(OptionsPage));
            builder.Services.AddTransientWithShellRoute<AboutPage, MainViewModel>(nameof(AboutPage));
            builder.Services.AddTransientWithShellRoute<GameSelectionPage, GameSelectionViewModel>(nameof(GameSelectionPage));
            builder.Services.AddTransientWithShellRoute<GamePage, GameViewModel>(nameof(GamePage));
        });
}
