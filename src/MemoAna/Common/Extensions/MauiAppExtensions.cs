#pragma warning disable CA1860
using MemoAna.Domain.Entities;
using MemoAna.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace MemoAna.Common.Extensions;

internal static class MauiAppExtensions
{
    extension(MauiApp app)
    {
        public MauiApp TrySeed()
        {
            try
            {
                using var scope = app.Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<GameDbContext>();

                // Validação do Seed baseada na tabela leve (Manifest)
                if (context.CardThemeManifests.Any(ct => ct.IsDefault && (ct.ThemeName.ToLower() == "disney" || ct.ThemeName.ToLower() == "marvel" || ct.ThemeName.ToLower() == "pokemon")))
                {
                    return app;
                }

                var assembly = Assembly.GetExecutingAssembly();
                var resourceNames = assembly.GetManifestResourceNames();

                var disneyResources = resourceNames.Where(name => name.Contains("disney") && name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)).ToList();
                var marvelResources = resourceNames.Where(name => name.Contains("marvel") && name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)).ToList();
                var pokemonResources = resourceNames.Where(name => name.Contains("pokemon") && name.EndsWith(".png", StringComparison.OrdinalIgnoreCase)).ToList();

                if (!marvelResources.Any() && !disneyResources.Any() && !pokemonResources.Any()) return app;

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
        if (!resources.Any()) return;

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

        if (!base64List.Any()) return;

        var manifestId = Guid.NewGuid().ToString();
        var themeId = Guid.NewGuid().ToString();

        var manifest = new CardThemeManifestEntity(manifestId)
        {
            ThemeName = themeName,
            IsDefault = true,
            PreviewBase64Image = base64List.First(),
            CardThemeId = themeId
        };

        var cardTheme = new CardThemeEntity(themeId)
        {
            Base64Images = base64List,
            ManifestId = manifestId
        };

        context.CardThemeManifests.Add(manifest);
        context.CardThemes.Add(cardTheme);
    }
}
#pragma warning restore CA1860