namespace MemoAna.Domain.Game.Enums;

/// <summary>
/// Game Difficulty levels with corresponding number of cards
/// </summary>
public enum GameDifficulty  : int
{
    /// <summary>
    /// 12 cards (6 pairs)
    /// </summary>
    Easy = 12,   
    /// <summary>
    /// 18 cards (9 pairs)
    /// </summary>
    Medium = 18, 
    /// <summary>
    /// 30 cards (15 pairs)
    /// </summary>
    Hard = 30    
}