using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemoAna.Application.Abstract.Services;
using MemoAna.Application.Core;
using MemoAna.Application.Dtos;
using MemoAna.Application.Services;
using MemoAna.Domain.Entities;
using MemoAna.Domain.Enums;
using System.Collections.ObjectModel;

namespace MemoAna.Presentation.ViewModels;

public partial class GameViewModel(IGameService gameService, IAudioService audioService) : BaseViewModel, IQueryAttributable
{
    [ObservableProperty]
    public partial string Theme { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string GameTime { get; set; } = "00:00";

    [ObservableProperty]
    public partial string CurrentScore { get; set; } = "0";

    [ObservableProperty]
    public partial string MovesCount { get; set; } = "0";

    [ObservableProperty]
    public partial string Difficulty { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int Rows { get; set; } = 3;

    [ObservableProperty]
    public partial int Columns { get; set; } = 4;

    [ObservableProperty]
    public partial bool IsGameActive { get; set; } = false;

    [ObservableProperty]
    public partial bool GameEnded { get; set; } = false;

    [ObservableProperty]
    public partial string IsVictory { get; set; } = string.Empty;

    [ObservableProperty]
    public partial GameStatisticsDto Statistics { get; set; } = GameStatisticsDto.Default;

    [ObservableProperty]
    public partial ObservableCollection<MemoryCard> Cards { get; set; } = [];

    private string _lastTimeStr = string.Empty;
    private string _lastScoreStr = string.Empty;

    [RelayCommand]
    private async Task FlipCardAsync(MemoryCard card)
    {
        if (card == null || !IsGameActive) return;

        await gameService.FlipCardAsync(card);

        MovesCount = gameService.TotalMoves.ToString();

        CurrentScore = gameService.CurrentScore.ToString("D4");
    }

    [RelayCommand]
    private static async Task MainTitle() => await Shell.Current.GoToAsync("MainPage");
    [RelayCommand]
    private async Task InitializeGameAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(Theme) || string.IsNullOrEmpty(Difficulty)) return;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                IsBusy = true;
                GameEnded = false;
                IsGameActive = false;
                GameTime = "00:00";
                CurrentScore = "000";
                MovesCount = "0";
                Cards.Clear();
            });

            var level = int.Parse(Difficulty);
            GameDifficulty difficulty = (GameDifficulty)level;

            ConfigureGridGeometry(difficulty);

            gameService.GameFinished -= OnGameFinished;
            gameService.GameFinished += OnGameFinished;
            gameService.TimerTick -= OnTimerTick;
            gameService.TimerTick += OnTimerTick;

            // Loads database and generates ImageSources in background
            await Task.Run(async () => await gameService.StartGameAsync(difficulty, Theme));

            // Single safe UI update 
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (gameService.CurrentCards is ObservableCollection<MemoryCard> cardsList)
                    foreach (var card in cardsList)
                        Cards.Add(card);

                int remainingTime = difficulty switch
                {
                    GameDifficulty.Easy => 75,
                    GameDifficulty.Medium => 100,
                    GameDifficulty.Hard => 120,
                    _ => 75
                };
                GameTime = TimeSpan.FromSeconds(remainingTime).ToString(@"mm\:ss");

                IsGameActive = true;
                IsBusy = false;
            });
            if (!GameEnded)
            {
                await audioService.StopAsync();
                await audioService.PlayMainGameAsync();
            }
        }
        catch (Exception e)
        {
            System.Diagnostics.Debug.WriteLine($"[GameViewModel] Erro crítico: {e.Message}");
            MainThread.BeginInvokeOnMainThread(() => IsBusy = false);
        }
    }

    /// <summary>
    /// Game timer tick event.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="gameTick"></param>
    /// <remarks>
    /// Format first outside main thread to avoid high cpu usage.
    /// Only invokes Main Thread if a real value change happens to avoid unnecessary repainting.
    /// </remarks>
    private void OnTimerTick(object? sender, GameTickEntity gameTick)
    {
        var newScore = gameService.CurrentScore.ToString("D3");

        if (gameTick.NextTime != _lastTimeStr || newScore != _lastScoreStr)
        {
            _lastTimeStr = gameTick.NextTime;
            _lastScoreStr = newScore;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                GameTime = gameTick.NextTime;
                CurrentScore = newScore;
            });
        }
    }

    private void OnGameFinished(object? sender, GameStatisticsEntity e)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            IsGameActive = false;
            GameEnded = true;
            Statistics = e.ToDto();
            IsVictory = e.IsVictory ? "+1 CONQUISTA!" : "DERROTA!";
            await audioService.StopAsync();
        });
    }

    /// <summary>
    /// Critical memory cleanup method. 
    /// </summary>
    /// <remarks>
    /// Called by <see cref="View"/>.OnDisappearing 
    /// </remarks>
    public async void CleanupGame()
    {
        if (gameService is GameService service)
        {
            service.ForceStopTimer();
        }

        gameService.GameFinished -= OnGameFinished;
        gameService.TimerTick -= OnTimerTick;

        IsGameActive = false;
        await audioService.StopAsync();
    }

    private void ConfigureGridGeometry(GameDifficulty difficulty)
    {
        (Rows, Columns) = difficulty switch
        {
            GameDifficulty.Easy => (4, 3),
            GameDifficulty.Medium => (5, 4),
            GameDifficulty.Hard => (6, 5),
            _ => (4, 3)
        };
    }

    private bool _isinitializing = false;
    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (_isinitializing) return;

        if (query.TryGetValue("Theme", out var themeValue))
            Theme = themeValue?.ToString() ?? string.Empty;

        if (query.TryGetValue("Difficulty", out var difficultyValue))
            Difficulty = difficultyValue?.ToString() ?? string.Empty;

        if (!string.IsNullOrEmpty(Theme) && !string.IsNullOrEmpty(Difficulty))
        {
            _isinitializing = true;
            await InitializeGameAsync();
            _isinitializing = false;
        }
    }
}
