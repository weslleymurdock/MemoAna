#if ANDROID
//using Android.Gms.Games;
//using ICompletedListener = Android.Gms.Tasks.IOnCompleteListener;
//using ATask = Android.Gms.Tasks.Task;

namespace MemoAna.Platforms.Android.Services;

public class PlayGamesService : MemoAna.Application.Abstract.Services.IGamePlatformService
{
    public void Initialize()
    {
        //var activity = Platform.CurrentActivity;
        //if (activity == null) return;

        //// Inicialização obrigatória do SDK v2 do Google
        //PlayGamesSdk.Initialize(activity);
    }

    public Task<bool> SilentLoginAsync()
    {
        var tcs = new TaskCompletionSource<bool>();
        var activity = Platform.CurrentActivity;

        if (activity == null)
        {
            tcs.SetResult(false);
            return tcs.Task;
        }

        //IGamesSignInClient signInClient = PlayGames.GetGamesSignInClient(activity);

        // Executa a Task nativa do Android
        //signInClient.IsAuthenticated().AddOnCompleteListener(new CompletedListenerRunnable(task =>
        //{
        //    if (task.IsSuccessful && (bool)task.Result)
        //    {
        //        System.Diagnostics.Debug.WriteLine("[PlayGameService]: Authenticated!");
        //        tcs.SetResult(true);
        //    }
        //    else
        //    {
        //        System.Diagnostics.Debug.WriteLine("[PlayGameService]: Login failed.");
        //        tcs.SetResult(false);
        //    }
        //}));

        return tcs.Task;
    }

    public void SendScoreToBoard(string boardId, long pontuacao)
    {
        var activity = Platform.CurrentActivity;
        if (activity == null) return;

        // Envia direto para a nuvem do Google usando o Client nativo de Leaderboards
        //PlayGames.GetLeaderboardsClient(activity).SubmitScore(boardId, pontuacao);
    }

    public void UnlockAchievement(string achievementId)
    {
        var activity = Platform.CurrentActivity;
        if (activity == null) return;

        // Desbloqueia a conquista nativamente (sobe aquele banner clássico verde no topo da tela)
        //PlayGames.GetAchievementsClient(activity).Unlock(achievementId);
    }
}

//// Classe auxiliar para mapear o listener de tarefas do Android (Java.Lang.Object) para o C#
//public class CompletedListenerRunnable : Java.Lang.Object, ICompletedListener
//{
//    private readonly Action<ATask> _action;
//    public CompletedListenerRunnable(Action<ATask> action) => _action = action;
//    public void OnComplete(ATask task) => _action(task);
//}
#endif