namespace MemoAna.Presentation.Localization;

internal class AppLocalizer<T> where T : class
{
    public IStringLocalizer<T> Localizer { get; }

    public AppLocalizer(IStringLocalizer<T> localizer)
    {
        Localizer = localizer;
    }
}