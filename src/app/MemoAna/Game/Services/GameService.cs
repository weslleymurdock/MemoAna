#pragma warning disable CA1416
using MemoAna.Game.Core;
using MemoAna.Common.Abstract.Repositories;
using MemoAna.Game.Abstract.Services;
using MemoAna.Game.Dtos;
using MemoAna.Game.Entities;
using MemoAna.Game.Enums;
using MemoAna.Game.EventArgs;

namespace MemoAna.Game.Services;

public sealed class GameService : IGameService
{
    private readonly IRepository<CardThemeManifestEntity> manifestRepository;
    private readonly IRepository<GameSettingsEntity> settingsRepository;
    private readonly IRepository<GameStatisticsEntity> statisticsRepository;
    private readonly IRepository<CardThemeEntity> themeRepository;
    private readonly IDispatcherTimer _gameTimer;
    private MemoryCard? _firstSelectedCard;
    private MemoryCard? _secondSelectedCard;
    private bool _isProcessingTurn;
    private string _currentTheme = string.Empty;
    private GameDifficulty _currentDifficulty;
    private GameSettingsDto gameSettings = default!;
    private int _totalMoves;
    private int _successfulMoves;
    private int _mistakes;
    private int _currentStreak;
    private int _accumulatedScore;
    public int TotalMoves => _totalMoves;
    public int CurrentScore => _accumulatedScore;

    public ObservableCollection<KeyValuePair<int, MemoryCard>> CurrentCards { get; } = [];
    public TimeSpan RemainingTime { get; private set; }
    public bool IsGameActive { get; private set; }
    
    public event EventHandler<GameStatisticsEventArgs>? GameFinished;
    public event EventHandler<GameTickEventArgs>? TimerTick;
    public event EventHandler<GameCardFlippedEventArgs>? CardFlipped;
    public GameService(IRepository<CardThemeManifestEntity> manifestRepository, 
        IRepository<GameSettingsEntity> settingsRepository, 
        IRepository<GameStatisticsEntity> statisticsRepository, 
        IRepository<CardThemeEntity> themeRepository, 
        IDispatcher dispatcher)
    {
        this.manifestRepository = manifestRepository;
        this.settingsRepository = settingsRepository;
        this.statisticsRepository = statisticsRepository;
        this.themeRepository = themeRepository;

        _gameTimer = dispatcher.CreateTimer();
        _gameTimer.Interval = TimeSpan.FromSeconds(1);
        _gameTimer.Tick += OnTimerTick;
    }

    public async Task StartGameAsync(int difficulty, string theme)
    {
        _gameTimer.Stop();
        _firstSelectedCard = null;
        _secondSelectedCard = null;
        _isProcessingTurn = false;
        
        _currentTheme = theme;
        _currentDifficulty = (GameDifficulty)difficulty;
        _totalMoves = 0;
        _successfulMoves = 0;
        _mistakes = 0;
        _currentStreak = 0;
        _accumulatedScore = 0;

        CurrentCards.Clear();

        (int pairCount, int totalSeconds) = _currentDifficulty switch
        {
            GameDifficulty.Easy => (6, 75),
            GameDifficulty.Medium => (10, 100),
            GameDifficulty.Hard => (15, 120),
            _ => (6, 75)
        };

        gameSettings = GameSettingsDto.FromEntity((await settingsRepository.ListTrackedAsync(x => x != null, null!, CancellationToken.None))
                   .Single() ?? new());

        CardThemeManifestEntity manifest = (await manifestRepository.ListTrackedAsync(m => m.ThemeName == theme, [x => x.CardTheme], CancellationToken.None)).Single() ?? throw new KeyNotFoundException("Manifesto do Tema não disponível");

        CardThemeEntity cards = await themeRepository.GetByIdAsync(manifest.Id, null!, CancellationToken.None) ?? throw new KeyNotFoundException("Tema não disponível");

        var random = new Random();

        // .OrderBy(_ => random.Next()) ensures always get a random set of cards from manifest
        List<string> rawStrings = cards?.Base64Images.OrderBy(_ => random.Next()).Take(pairCount).ToList() 
            ?? throw new KeyNotFoundException("Imagens do tema não encontradas");

        var gameCards = new List<MemoryCard>();
        int idFactory = 0;
        string pairIdFactory = Guid.Empty.ToString();
        
        foreach (var base64Str in rawStrings)
        {
            if (string.IsNullOrEmpty(base64Str)) continue;
            pairIdFactory = Guid.CreateVersion7().ToString();
            
            gameCards.Add(new MemoryCard { Id = idFactory++, PairId = pairIdFactory, CardImage = base64Str });
            gameCards.Add(new MemoryCard { Id = idFactory++, PairId = pairIdFactory, CardImage = base64Str });
        }

        var shuffledCards = gameCards.OrderBy(_ => random.Next()).ToList();

        int i = 0;
        foreach (MemoryCard? card in shuffledCards)
            CurrentCards.Add(new KeyValuePair<int, MemoryCard>(i+=1, card));
        IsGameActive = true;
        RemainingTime = TimeSpan.FromSeconds(totalSeconds);
        _gameTimer.Start();
    }
 
    public async Task FlipCardAsync(int position, MemoryCard selectedCard)
    {
        if (!IsGameActive || _isProcessingTurn || selectedCard.IsFaceUp || selectedCard.IsMatched)
            return;

        selectedCard.IsFaceUp = true;

        if (_firstSelectedCard == null)
        {
            CardFlipped?.Invoke(this, new((position, selectedCard.CardImage)!));
            _firstSelectedCard = selectedCard;
            return;
        }

        CardFlipped?.Invoke(this, new((position, selectedCard.CardImage)!));
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

            await Task.Delay(gameSettings.Options.CardFlipDelayMs);

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
        if (CurrentCards.All(c => c.Value.IsMatched))
            EndGame(won: true);
    }
    
    public void ForceStopTimer()
    {
        IsGameActive = false;
        _gameTimer?.Stop();
    }

    private void OnTimerTick(object? sender, System.EventArgs e)
    {
        if (!IsGameActive) return;

        RemainingTime = RemainingTime.Subtract(TimeSpan.FromSeconds(1));

        TimerTick?.Invoke(this, new GameTickEventArgs((int)RemainingTime.TotalSeconds));

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
            int unmatchedCardsCount = CurrentCards.Count(c => !c.Value.IsMatched);
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
            PlayedAt = DateTime.UtcNow 
        };

        try
        {
            await statisticsRepository.AddAsync(stats, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Falha ao salvar Scoreboard: {ex.Message}");
        }
        finally
        {
            GameFinished?.Invoke(this, stats.ToEventArgs());
        }
    }
}
#pragma warning restore CA1416