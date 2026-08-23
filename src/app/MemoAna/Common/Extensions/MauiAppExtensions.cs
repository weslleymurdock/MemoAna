using MemoAna.Common.Abstract.ExceptionHandler;
using MemoAna.Game.Entities;
using MemoAna.Common.Persistence;
using System.Reflection;
#if WINDOWS
using Microsoft.UI.Xaml;
#endif

namespace MemoAna.Common.Extensions;

internal static class MauiAppExtensions
{
    extension(MauiApp app)
    {
//        internal MauiApp ConfigureExceptionHandler()
//        {
//            var handler = app.Services.GetRequiredService<IExceptionHandler>();
//            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
//            {
//                if (e.ExceptionObject is Exception ex)
//                    _ = Task.Run(async () => await handler.HandleAsync(ex)); // fire-and-forget, cuidado com deadlocks
//            };

//            TaskScheduler.UnobservedTaskException += (s, e) =>
//            {
//                _ = handler.HandleAsync(e.Exception);
//                e.SetObserved();
//            };

//#if ANDROID
//            Android.Runtime.AndroidEnvironment.UnhandledExceptionRaiser += (s, e) =>
//            {
//                _ = handler.HandleAsync(e.Exception);
//                e.Handled = true;
//            };
//#elif IOS || MACCATALYST
//        ObjCRuntime.Runtime.MarshalManagedException += (s, e) =>
//        {
//            _ = handler.HandleAsync(e.Exception);
//        };
//#elif WINDOWS

//        // dentro do RegisterGlobalHandlers
//        if (Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?
//                .Services.GetService<Microsoft.UI.Xaml.Application>() is { } winUiApp)
//        {
//            winUiApp.UnhandledException += (s, e) =>
//            {
//                e.Handled = true; // <- crucial: impede o crash do processo
//                _ = handler.HandleAsync(e.Exception);
//            };
//        }
//#endif
//            return app;
//        }

        internal MauiApp TrySeed()
        {
            try
            {
                using IServiceScope scope = app.Services.CreateScope();
                GameDbContext context = scope.ServiceProvider.GetRequiredService<GameDbContext>();

                if (!context.GameSettings.Any())
                {
                    context.GameSettings.Add(new GameSettingsEntity()
                    {
                        Id = Guid.CreateVersion7().ToString(),
                        Options = {
                            CardFlipDelayMs = 900,
                            CloudSaveEnabled = true,
                            ConfirmOnExit = true,
                            IsHapticFeedbackEnabled = true,
                            IsMusicEnabled = true,
                            IsSfxEnabled = true,
                            Language = Common.Enums.Language.system
                        }
                    });
                    context.SaveChanges();
                }
                if (context.CardThemeManifests.Any(ct => ct.IsDefault && (ct.ThemeName.ToLower() == "disney" || ct.ThemeName.ToLower() == "marvel" || ct.ThemeName.ToLower() == "pokemon")))
                {
                    return app;
                }

                var assembly = Assembly.GetExecutingAssembly();
                var resourceNames = assembly.GetManifestResourceNames();

                var disneyResources = resourceNames.Where(name => name.Contains("disney") && name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)).ToList();
                var marvelResources = resourceNames.Where(name => name.Contains("marvel") && name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)).ToList();
                var pokemonResources = resourceNames.Where(name => name.Contains("pokemon") && name.EndsWith(".png", StringComparison.OrdinalIgnoreCase)).ToList();

                if (!(marvelResources.Any(x => x is not null) || disneyResources.Any(x => x is not null) || pokemonResources.Any(x => x is not null))) return app;

                // Processa os dados e gera os Manifests emparelhados com os Payloads
                ProcessAndAddTheme(context, assembly, "Disney", disneyResources, "jpeg");
                ProcessAndAddTheme(context, assembly, "Marvel", marvelResources, "jpeg");
                ProcessAndAddTheme(context, assembly, "Pokemon", pokemonResources, "png");

                context.SaveChanges();
                return app;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to seed database: {e.Message}");
                return app;
            }
        }
    }

    private static void ProcessAndAddTheme(GameDbContext context, Assembly assembly, string themeName, List<string> resources, string defaultExt)
    {
        if (resources.Count <= 0) return;

        var base64List = new List<string>();

        foreach (var resourceName in resources)
        {
            using Stream stream = assembly.GetManifestResourceStream(resourceName)!;
            if (stream == null) continue;

            using MemoryStream ms = new();
            stream.CopyTo(ms);
            byte[] imageBytes = ms.ToArray();

            string extension = resourceName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ? "png" : defaultExt;
            string base64String = $"data:image/{extension};base64,{Convert.ToBase64String(imageBytes)}";

            base64List.Add(base64String);
        }

        if (base64List.Count <= 0) return;

        var manifestId = Guid.CreateVersion7().ToString();
        var themeId = Guid.CreateVersion7().ToString();

        var manifest = new CardThemeManifestEntity(manifestId)
        {
            ThemeName = themeName,
            IsDefault = true,
            PreviewBase64Image = base64List.First(),
            CardThemeId = themeId,
            Id = manifestId
        };

        var cardTheme = new CardThemeEntity(themeId)
        {
            Base64Images = base64List,
            ManifestId = manifestId,
            Manifest = manifest,
            Id = themeId
        };

        context.CardThemeManifests.Add(manifest);
        context.CardThemes.Add(cardTheme);
    }
}