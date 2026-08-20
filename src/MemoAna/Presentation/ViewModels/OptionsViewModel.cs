using MemoAna.Application.Abstract;

namespace MemoAna.Presentation.ViewModels;

public partial class OptionsViewModel(IRepository repository, ILocalizer localizer) : ObservableObject
{
    public string BackLabel { get => localizer["BackLabel"]; }
    public string CloudLabel { get => Settings.Options.CloudSaveEnabled ? localizer["CloudLabelOn"] : localizer["CloudLabelOff"]; }
    public string ConfirmOnExitLabel { get => Settings.Options.ConfirmOnExit ? localizer["ConfirmOnExitLabelOn"] : localizer["ConfirmOnExitLabelOff"]; }
    public string MusicLabel { get => Settings.Options.IsMusicEnabled ? localizer["MusicLabelOn"] : localizer["MusicLabelOff"]; }
    public string SettingsLabel { get => localizer["SettingsLabel"]; }
    public string SfxLabel { get => Settings.Options.IsSfxEnabled ? localizer["SfxLabelOn"] : localizer["SfxLabelOff"]; }

    [ObservableProperty]
    public partial GameSettingsDto Settings { get; set; } = new GameSettingsDto("", new(900, true, true, true, true, true, 0));
    
    [RelayCommand]
    private async Task Back()
    {
        await SaveOptions();
        await Shell.Current.GoToAsync("MainPage");
    }
    
    [RelayCommand]
    private void CloudOnOff() => Settings.Options.CloudSaveEnabled = !Settings.Options.CloudSaveEnabled;

    [RelayCommand]
    private void ConfirmOnExitOnOff() => Settings.Options.ConfirmOnExit = !Settings.Options.ConfirmOnExit;
    [RelayCommand]
    private void SoundOnOff() => Settings.Options.IsMusicEnabled = !Settings.Options.IsMusicEnabled;
    [RelayCommand]
    public void SfxOnOff() => Settings.Options.IsSfxEnabled = !Settings.Options.IsSfxEnabled;
    
    [RelayCommand]
    private void LoadOptions() => Settings = GameSettingsDto.FromEntity(repository.Query<GameSettingsEntity>().Single());
    [RelayCommand]
    private async Task SaveOptions()
    {
        GameSettingsEntity gse = repository.Query<GameSettingsEntity>().Where(gs => gs.Id == Settings.Id).Single();
        gse.Options = Settings.Options.ToEntity()!;
        await repository.UpdateAsync(gse);
        repository.SaveChanges();
    }
}
