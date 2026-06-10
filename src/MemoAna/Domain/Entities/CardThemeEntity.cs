using System.ComponentModel.DataAnnotations.Schema;

namespace MemoAna.Domain.Entities;

public sealed class CardThemeEntity(string id) : BaseEntity(id)
{
    public List<string> Base64Images { get; set; } = [];
    public string ManifestId { get; set; } = string.Empty;
    public CardThemeManifestEntity? Manifest { get; set; }
     
}