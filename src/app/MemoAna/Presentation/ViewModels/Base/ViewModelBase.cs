using CommunityToolkit.Mvvm.ComponentModel;

namespace MemoAna.Presentation.ViewModels.Base;

public partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    public partial bool IsBusy { get; set; } = false;
}
