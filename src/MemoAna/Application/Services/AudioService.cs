using MemoAna.Application.Abstract.Services;
using Plugin.Maui.Audio;

namespace MemoAna.Application.Services;

internal class AudioService(IAudioManager manager) : IAudioService, IDisposable
{
    private readonly IAudioManager _manager = manager;
    private IAudioPlayer? _player;
    private Stream? _currentStream;

    public bool IsPlaying => _player?.IsPlaying ?? false;

    private async Task InitializeAsync(string fileName)
    {
        try
        {
            // 1. Previous player and stream cleanup to free the hardware channel
            CleanUpCurrentPlayer();

            // 2. Safelly open the stream
            _currentStream = await FileSystem.OpenAppPackageFileAsync(fileName);

            // 3. Player creation and loop activation if background audio
            _player = _manager.CreatePlayer(_currentStream);

            // 4. If is main title or main game, set continuous loop
            if (fileName.Contains("whispers") || fileName.Contains("running_out"))
                _player.Loop = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AudioService] Erro ao carregar arquivo {fileName}: {ex.Message}");
            throw;
        }
    }

    public async Task PlayMainTitleAsync()
    {
        await InitializeAsync("wood-chapter-whispers-of-the-grove.mp3");
        _player?.Play();
    }

    public async Task PlayMainGameAsync()
    {
        await InitializeAsync("times_running_out-spencer_y_k.mp3");
        _player?.Play();
    }

    public async Task PlayWinAsync()
    {
        // Certifique-se de preencher o nome quando tiver o arquivo
        await InitializeAsync("win_effect.mp3");
        _player?.Play();
    }

    public async Task PlayLoseAsync()
    {
        await InitializeAsync("lose_effect.mp3");
        _player?.Play();
    }

    public async Task StopAsync()
    {
        if (_player != null && _player.IsPlaying)
        {
            _player.Stop();
        }
        CleanUpCurrentPlayer();
    }

    private void CleanUpCurrentPlayer()
    {
        if (_player != null)
        {
            _player.Stop();
            _player.Dispose();
            _player = null;
        }

        if (_currentStream != null)
        {
            _currentStream.Close();
            _currentStream.Dispose();
            _currentStream = null;
        }
    }

    public void Dispose()
    {
        CleanUpCurrentPlayer();
    }
}