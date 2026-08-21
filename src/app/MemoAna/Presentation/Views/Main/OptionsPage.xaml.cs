using MemoAna.Presentation.ViewModels;

namespace MemoAna.Presentation.Views;

public partial class OptionsPage : ContentPage
{
	public OptionsPage(OptionsViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
		vm.LoadOptionsCommand.Execute(default!);
	}
    protected override void OnDisappearing()
    {
		if (BindingContext is OptionsViewModel vm)
			vm.SaveOptionsCommand.Execute(default!);
        base.OnDisappearing();
    }
}