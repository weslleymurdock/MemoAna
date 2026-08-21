using CommunityToolkit.Maui;
using MemoAna.Application.Common.Abstract.Localization;
using MemoAna.Application.Common.Abstract.Repositories;
using MemoAna.Application.Common.Abstract.Services;
using MemoAna.Application.Game.Abstract.Services;
using MemoAna.Application.Game.Core;
using MemoAna.Infrastructure.Common.Services;
using MemoAna.Infrastructure.Game.Services;
using MemoAna.Infrastructure.Localization;
using MemoAna.Infrastructure.Persistence;
using MemoAna.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Plugin.Maui.Audio;

namespace MemoAna.Composition.Extensions;

public static class MauiAppBuilderExtensions
{
    extension(MauiAppBuilder builder)
    {
        public MauiApp RunMauiApp<TApp>(Action<MauiAppBuilder> configure)
            where TApp : Microsoft.Maui.Controls.Application
        {
            builder.UseMauiApp<TApp>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Gwenchana.ttf", "Gwenchana");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif
            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
            builder.Services.AddSingleton<ILocalizer, Localizer>();
            return builder.AddInfrastructure()
                .AddApplication()
                .AddPresentation(configure)
                .TryMigrateDb()
                .TrySeed();
        }
        
        private MauiAppBuilder AddApplication()
        {
            builder.Services.AddScoped<IImageConverterService, ImageConverterService>();
            builder.Services.AddScoped<IGameService, GameService>();
            builder.Services.AddScoped<MemoryCard>();
            return builder;
        }

        private  MauiAppBuilder AddInfrastructure()
        {
            builder.Services.AddScoped<IAudioService, AudioService>();
            builder.Services.AddSingleton<HttpClient>();
            builder.Services.AddSingleton(AudioManager.Current);
            builder.AddSqlite();
            return builder;
        }
//        private  MauiAppBuilder AddPlatformServices()
//        {
//#if ANDROID
//            builder.Services.AddSingleton<IGamePlatformService, PlayGamesService>();
//#endif
//#if WINDOWS
//            builder.Services.AddSingleton<IGamePlatformService, WindowsGamingService>();
//#endif

//            return builder; 
//        }
        private  MauiAppBuilder AddPresentation(Action<MauiAppBuilder> configure)
        {
            builder.Services.AddSingleton(sp
                => Microsoft.Maui.Controls.Application.Current?.Dispatcher
                ?? Microsoft.Maui.Dispatching.Dispatcher.GetForCurrentThread()!);
            configure?.Invoke(builder);
            return builder;
        }
        private MauiAppBuilder AddSqlite()
        {
            builder.Services.AddDbContext<GameDbContext>(options =>
            {
                var dbPath = Path.Combine(FileSystem.AppDataDirectory, "memoana.db3");
                options.UseSqlite($"Data Source={dbPath}", dbOptions =>
                {
                    dbOptions.CommandTimeout(TimeSpan.FromSeconds(60).Seconds);
                });
            });
            builder.Services.AddTransient<IRepository, Repository>();
            return builder;
        }

        private  MauiApp TryMigrateDb()
        {
            MauiApp app = builder.Build();
            using IServiceScope scope = app.Services.CreateScope();
            GameDbContext context = scope.ServiceProvider.GetRequiredService<GameDbContext>();
            try
            {
                if (context.Database.GetPendingMigrations().Any())
                {
                    context.Database.Migrate();
                }
            }
            catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.SqliteErrorCode == 1 || ex.SqliteErrorCode == 19)
            {
                try
                {
                    context.Database.EnsureDeleted();
                    context.Database.Migrate();
                }
                catch 
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database migration/merge failed: {ex.Message}");
                throw;
            } 

            return app;
        } 
    }
     

}
