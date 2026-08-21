using MemoAna.Presentation.ViewModels;

namespace MemoAna.Presentation.Views;

public partial class GameSelectionPage : ContentPage
{
	public GameSelectionPage(GameSelectionViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is GameSelectionViewModel vm)
        {
            await vm.LoadThemesCommand.ExecuteAsync(null);
        }

    }
}