#pragma warning disable CA1416
using MemoAna.Application.Abstract.Repositories;
using MemoAna.Application.Abstract.Services;
using MemoAna.Application.Core;
using MemoAna.Domain.Entities;
using MemoAna.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel; 

namespace MemoAna.Application.Services;

internal sealed class GameService : IGameService
{
    private readonly IRepository repository;
    private readonly IDispatcherTimer _gameTimer;
    private MemoryCard? _firstSelectedCard;
    private MemoryCard? _secondSelectedCard;
    private bool _isProcessingTurn;
    private string _currentTheme = string.Empty;
    private GameDifficulty _currentDifficulty;
    private int _totalMoves;
    private int _successfulMoves;
    private int _mistakes;
    private int _currentStreak;
    private int _accumulatedScore;
    public int TotalMoves => _totalMoves;
    public int CurrentScore => _accumulatedScore;

    public ObservableCollection<MemoryCard> CurrentCards { get; } = [];
    public TimeSpan RemainingTime { get; private set; }
    public bool IsGameActive { get; private set; }
    
    public event EventHandler<GameStatisticsEntity>? GameFinished;
    public event EventHandler<GameTickEntity>? TimerTick;
    public GameService(IRepository cardRepository, IDispatcher dispatcher)
    {
        repository = cardRepository;

        _gameTimer = dispatcher.CreateTimer();
        _gameTimer.Interval = TimeSpan.FromSeconds(1);
        _gameTimer.Tick += OnTimerTick;
    }

    public async Task StartGameAsync(GameDifficulty difficulty, string theme)
    {
        _gameTimer.Stop();
        _firstSelectedCard = null;
        _secondSelectedCard = null;
        _isProcessingTurn = false;

        _currentTheme = theme;
        _currentDifficulty = difficulty;
        _totalMoves = 0;
        _successfulMoves = 0;
        _mistakes = 0;
        _currentStreak = 0;
        _accumulatedScore = 0;

        CurrentCards.Clear();

        var (pairCount, totalSeconds) = difficulty switch
        {
            GameDifficulty.Easy => (6, 75),
            GameDifficulty.Medium => (10, 100),
            GameDifficulty.Hard => (15, 120),
            _ => (6, 75)
        };


        var manifest = await repository.Query<CardThemeManifestEntity>()
                     .Where(m => m.ThemeName == theme)
                     .Include(m => m.CardTheme)
                     .SingleAsync();

        var random = new Random();

        var rawStrings = manifest?.CardTheme?.Base64Images.OrderBy(_ => random.Next()).Take(pairCount).ToList()
            ?? throw new KeyNotFoundException("Imagens do tema não encontradas");

        var gameCards = new List<MemoryCard>();
        int idFactory = 0;
        string pairIdFactory = Guid.Empty.ToString();
        
        foreach (var base64Str in rawStrings)
        {
            if (string.IsNullOrEmpty(base64Str)) continue;
            pairIdFactory = Guid.NewGuid().ToString();
            byte[] imageBytes = Convert.FromBase64String(base64Str.Split(',')[1]);

            ImageSource sourceCardA = ImageSource.FromStream(() => new MemoryStream(imageBytes));
            ImageSource sourceCardB = ImageSource.FromStream(() => new MemoryStream(imageBytes));

            gameCards.Add(new MemoryCard { Id = idFactory++, PairId = pairIdFactory, CardImage = sourceCardA });
            gameCards.Add(new MemoryCard { Id = idFactory++, PairId = pairIdFactory, CardImage = sourceCardB });
        }

        var shuffledCards = gameCards.OrderBy(_ => random.Next()).ToList();

        foreach (var card in shuffledCards)
            CurrentCards.Add(card);

        IsGameActive = true;
        RemainingTime = TimeSpan.FromSeconds(totalSeconds);
        _gameTimer.Start();
    }
 
    public async Task FlipCardAsync(MemoryCard selectedCard)
    {
        if (!IsGameActive || _isProcessingTurn || selectedCard.IsFaceUp || selectedCard.IsMatched)
            return;

        selectedCard.IsFaceUp = true;

        if (_firstSelectedCard == null)
        {
            _firstSelectedCard = selectedCard;
            return;
        }

        _secondSelectedCard = selectedCard;
        _isProcessingTurn = true;
        _totalMoves++;

        if (_firstSelectedCard.PairId.Equals(_secondSelectedCard.PairId))
        {
            _firstSelectedCard.IsMatched = true;
            _secondSelectedCard.IsMatched = true;

            _successfulMoves++;
            _currentStreak++;

            _accumulatedScore = (_accumulatedScore + 1) * _currentStreak;

            ResetTurn();
            CheckWinCondition();
        }
        else
        {
            _mistakes++;
            _currentStreak = 0;

            await Task.Delay(900);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                _firstSelectedCard?.IsFaceUp = false;
                _secondSelectedCard?.IsFaceUp = false;
                ResetTurn();
            });
        }
    }
    private void ResetTurn()
    {
        _firstSelectedCard = null;
        _secondSelectedCard = null;
        _isProcessingTurn = false;
    }
     
    private void CheckWinCondition()
    {
        if (CurrentCards.All(c => c.IsMatched))
            EndGame(won: true);
    }
    
    public void ForceStopTimer()
    {
        IsGameActive = false;
        _gameTimer?.Stop();
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (!IsGameActive) return;

        RemainingTime = RemainingTime.Subtract(TimeSpan.FromSeconds(1));

        TimerTick?.Invoke(this, new GameTickEntity((int)RemainingTime.TotalSeconds));

        if (RemainingTime.TotalSeconds <= 0)
        {
            EndGame(won: false);
        }
    }

    private async void EndGame(bool won)
    {
        _gameTimer.Stop();
        IsGameActive = false;

        int finalScoreCalculated = _accumulatedScore;
        int remainingSeconds = (int)RemainingTime.TotalSeconds;

        if (won)
        {
            finalScoreCalculated += remainingSeconds * 25;
        }
        else
        {
            int unmatchedCardsCount = CurrentCards.Count(c => !c.IsMatched);
            finalScoreCalculated -= unmatchedCardsCount * 50;
            if (finalScoreCalculated < 0)
                finalScoreCalculated = 0;
        }

        var stats = new GameStatisticsEntity
        {
            ThemeName = _currentTheme,
            Difficulty = _currentDifficulty,
            IsVictory = won,
            TotalMoves = _totalMoves,
            SuccessfulMoves = _successfulMoves,
            Mistakes = _mistakes,
            RemainingSeconds = remainingSeconds,
            FinalScore = finalScoreCalculated,
            PlayedAt = DateTime.Now 
        };

        try
        {
            await repository.AddAsync(stats);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Falha ao salvar Scoreboard: {ex.Message}");
        }
        finally
        {
            GameFinished?.Invoke(this, stats);
        }
    }
}
#pragma warning restore CA1416