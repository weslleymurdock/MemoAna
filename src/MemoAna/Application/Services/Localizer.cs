using MemoAna.Application.Abstract;
using MemoAna.Presentation.Localization;

namespace MemoAna.Application.Services;

public class Localizer(IStringLocalizerFactory factory) : ILocalizer
{
    private readonly IStringLocalizer _localizer = factory.Create(
            "Localization",
            typeof(Localizer).Assembly.FullName!
        );

    public string this[string key] => _localizer[key];
}
