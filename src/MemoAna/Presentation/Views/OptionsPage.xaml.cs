using MemoAna.Presentation.ViewModels;

namespace MemoAna.Presentation.Views;

public partial class OptionsPage : ContentPage
{
	public OptionsPage(OptionsViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}