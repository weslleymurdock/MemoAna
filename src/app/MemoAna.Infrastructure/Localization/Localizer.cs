using MemoAna.Application.Common.Abstract.Localization;
using Microsoft.Extensions.Localization;

namespace MemoAna.Infrastructure.Localization;

public class Localizer(IStringLocalizerFactory factory) : ILocalizer
{
    private readonly IStringLocalizer _localizer = factory.Create(
            "Localization",
            typeof(Localizer).Assembly.FullName!
        );

    public string this[string key] => _localizer[key];
}
