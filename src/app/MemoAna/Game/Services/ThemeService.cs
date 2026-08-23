using MemoAna.Common.Abstract.Repositories;
using MemoAna.Game.Abstract.Services;
using MemoAna.Game.Dtos;
using MemoAna.Game.Entities;

namespace MemoAna.Game.Services;

public sealed class ThemeService(IRepository<CardThemeManifestEntity> repository) : IThemeService
{
    public async Task<IReadOnlyCollection<CardThemeManifestDto>> GetThemesAsync()
        => [.. (await repository.ListAsync(x => true, null!, CancellationToken.None))
            .Select(theme => new CardThemeManifestDto(theme) )];
}
