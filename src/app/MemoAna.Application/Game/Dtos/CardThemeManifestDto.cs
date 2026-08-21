using MemoAna.Domain.Game.Entities;

namespace MemoAna.Application.Game.Dtos;

public class CardThemeManifestDto
{
    public string ThemeName { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public string PreviewBase64Image { get; set; } = string.Empty;
    public string CardThemeId { get; set; } = string.Empty;
    public CardThemeDto? CardTheme { get; set; }
}

public sealed record CardThemeDto(ICollection<string> Base64Images,string ManifestId, CardThemeManifestDto? Manifest);