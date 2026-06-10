using CommunityToolkit.Mvvm.ComponentModel;

namespace MemoAna.Presentation.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    public partial bool IsBusy { get; set; } = false;
}
