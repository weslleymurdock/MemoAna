using MemoAna.Application.Game.Dtos;

namespace MemoAna.Application.Game.Abstract.Services;

public interface IThemeService
{
    Task<IReadOnlyCollection<CardThemeManifestDto>> GetThemesAsync();
}
