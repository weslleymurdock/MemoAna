using MemoAna.Presentation.ViewModels;

namespace MemoAna.Presentation.Views;

public partial class AboutPage : ContentPage
{
	public AboutPage(AboutViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}