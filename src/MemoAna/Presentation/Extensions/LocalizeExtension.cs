using MemoAna.Application.Abstract;

namespace MemoAna.Presentation.Extensions;

[ContentProperty(nameof(Key))]
public class LocalizeExtension : IMarkupExtension<string>
{
    public string Key { get; set; } = string.Empty;

    public string ProvideValue(IServiceProvider serviceProvider)
    {
        ILocalizer? localizer = App.Current?.Handler.MauiContext?.Services.GetService<ILocalizer>();

        if (localizer == null || string.IsNullOrEmpty(Key))
            return Key;

        return localizer[Key];
    }

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);
}