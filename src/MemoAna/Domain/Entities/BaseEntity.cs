namespace MemoAna.Domain.Entities;

public class BaseEntity(string Id)
{
    public string Id { get; set; } = Id ?? Guid.NewGuid().ToString();
}
