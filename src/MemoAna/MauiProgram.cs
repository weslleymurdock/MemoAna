using MemoAna.Common.Extensions;

namespace MemoAna;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp() 
        => MauiApp.CreateBuilder()
            .CreateMauiApp<App>()
            .AddInfrastructure()
            .AddApplication()
            .AddPresentation()
            .TryMigrateDb()
            .TrySeed();
}
