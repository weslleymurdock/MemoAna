using MemoAna.Common.Extensions;

namespace MemoAna;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp() 
        => MauiApp.CreateBuilder()
            .RunMauiApp<App>();
}
