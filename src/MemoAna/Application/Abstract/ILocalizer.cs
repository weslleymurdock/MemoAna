namespace MemoAna.Application.Abstract;

public interface ILocalizer
{
    string this[string key] { get; }
}
