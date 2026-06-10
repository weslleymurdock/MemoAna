using CommunityToolkit.Mvvm.ComponentModel;

namespace MemoAna.Presentation.ViewModels;

public partial class OptionsViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string SoundOnOff { get; set; } = string.Empty;


}
