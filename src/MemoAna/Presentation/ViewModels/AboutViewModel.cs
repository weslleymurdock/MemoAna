using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MemoAna.Presentation.ViewModels;

public partial class AboutViewModel : ObservableObject
{
    [RelayCommand]
    private static async Task Back()
    {
        await AppShell.Current.GoToAsync("MainPage");
    }
}
