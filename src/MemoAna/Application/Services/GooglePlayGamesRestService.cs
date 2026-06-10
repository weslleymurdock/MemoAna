namespace MemoAna.Application.Services;

using Google.Apis.Services;
using Google.Apis.Games.v1;
using Google.Apis.Games.v1.Data;
using Google.Apis.Auth.OAuth2;

public class GooglePlayGamesRestService
{
    private GamesService _gamesService;
    private string _accessToken;

    /// <summary>
    /// Inicialize o serviço passando o Token OAuth obtido no login do usuário
    /// </summary>
    /// <param name="accessToken"></param>
    public void Initialize(string accessToken)
    {
        _accessToken = accessToken;

        var credential = GoogleCredential.FromAccessToken(_accessToken);

        _gamesService = new GamesService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "MemoAna"
        });
    }

    /// <summary>
    /// Desbloqueia uma conquista via API REST
    /// </summary>
    public async Task<AchievementUnlockResponse> UnlockAchievementAsync(string achievementId)
    {
        try
        {
            var request = _gamesService.Achievements.Unlock(achievementId);
            var response = await request.ExecuteAsync();
            return response; // Contém informações se foi recém-desbloqueada
        }
        catch (Exception ex)
        {
            // Trate erros de expiração de token ou rede aqui
            Console.WriteLine($"Erro ao desbloquear conquista: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Envia uma pontuação para o Placar (Leaderboard) via API REST
    /// </summary>
    public async Task<PlayerScoreResponse> SubmitScoreAsync(string leaderboardId, long scoreValue)
    {
        try
        {
            var request = _gamesService.Scores.Submit(leaderboardId, scoreValue);
            var response = await request.ExecuteAsync();
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao enviar score: {ex.Message}");
            throw;
        }
    }
}