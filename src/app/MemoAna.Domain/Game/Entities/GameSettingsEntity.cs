using MemoAna.Domain.Common.Entities;
using MemoAna.Domain.Game.Models;

namespace MemoAna.Domain.Game.Entities;

public class GameSettingsEntity : EntityBase
{
    public GameSettingsEntity() : base(Guid.CreateVersion7().ToString()) { }

    public GameSettingsEntity(string id) : base(id) { }

    public GameOptions Options { get; set; } = new();
}