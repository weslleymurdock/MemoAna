#pragma warning disable CA1860 
using MemoAna.Application.Abstract.Repositories;
using MemoAna.Application.Abstract.Services;
using MemoAna.Application.Core;
using MemoAna.Application.Services;
using MemoAna.Domain.Entities;
using MemoAna.Infrastructure.Persistence;
using MemoAna.Infrastructure.Repositories;
using MemoAna.Presentation.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Plugin.Maui.Audio;
using System.Reflection;
using CommunityToolkit.Maui;

namespace MemoAna.Common.Extensions;

internal static class MauiAppBuilderExtensions
{
    extension(MauiAppBuilder builder)
    {
        internal MauiAppBuilder CreateMauiApp<TApp>()
            where TApp : Microsoft.Maui.Controls.Application
        {
            builder
                .UseMauiApp<App>()
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

            return builder;
        }
        internal MauiAppBuilder AddApplication()
        {
            builder.Services.AddScoped<IImageConverterService, ImageConverterService>();
            builder.Services.AddScoped<IAudioService, AudioService>();
            builder.Services.AddScoped<IGameService, GameService>();
            builder.Services.AddScoped<MemoryCard>();
            return builder;
        }
        internal MauiAppBuilder AddInfrastructure()
        {
            builder.Services.AddSingleton<HttpClient>();
            builder.Services.AddSingleton(AudioManager.Current);
            builder.AddSqlite();
            return builder;
        }
        internal MauiAppBuilder AddPresentation()
        {
            builder.Services.AddSingleton(sp
                => Microsoft.Maui.Controls.Application.Current?.Dispatcher
                ?? Microsoft.Maui.Dispatching.Dispatcher.GetForCurrentThread()!);
            builder.AddViews();
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
        private MauiAppBuilder AddViews()
        {
            builder.Services.AddTransientWithShellRoute<MainPage, MainViewModel>(nameof(MainPage));
            builder.Services.AddTransientWithShellRoute<OptionsPage, OptionsViewModel>(nameof(OptionsPage));
            builder.Services.AddTransientWithShellRoute<AboutPage, MainViewModel>(nameof(AboutPage));
            builder.Services.AddTransientWithShellRoute<GameSelectionPage, GameSelectionViewModel>(nameof(GameSelectionPage));
            builder.Services.AddTransientWithShellRoute<GamePage, GameViewModel>(nameof(GamePage));
            return builder;
        }

        internal MauiApp TryMigrateDb()
        {
            var app = builder.Build();
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<GameDbContext>();
            try
            {
                // Legacy data verification before migration: We check if there are any records in the old structure before it gets deleted by the migration.
                var oldData = LegacyBackup(context);

                if (context.Database.GetPendingMigrations().Any())
                {
                    // if there are pending migrations, it means we are moving from the old structure to the new one.
                    // The migration will drop the old tables, so we need to backup the data before that happens.
                    // After the backup, we can safely delete the old database if there was any data,
                    // and then apply the new migration to create the new structure.
                    if (oldData.Any())
                    {
                        context.Database.EnsureDeleted();
                    }

                    // Cria a estrutura nova perfeitamente (Tabelas: ThemeManifests e CardThemes com FK)
                    context.Database.Migrate();
                }

                // 2. Se pegamos dados do modelo antigo, fazemos o merge reconstruindo as chaves!
                if (oldData.Any())
                {
                    RestoreAndMerge(context, oldData);
                }
            }
            catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.SqliteErrorCode == 1 || ex.SqliteErrorCode == 19)
            {
                Console.WriteLine($"Conflito de migração detectado ({ex.Message}). Forçando reset físico do banco...");
                try
                {
                    context.Database.EnsureDeleted();
                    context.Database.Migrate();
                }
                catch (Exception deepEx)
                {
                    Console.WriteLine($"Falha crítica no reset automático: {deepEx.Message}");
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
    
    private static List<LegacyData> LegacyBackup(GameDbContext context)
    {
        var backup = new List<LegacyData>();
        try
        {
            // Abrimos a conexão nativa para rodar um SQL bruto na tabela antiga
            var connection = context.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();

            // Query para ler o modelo antigo antes dele deixar de existir
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT ThemeName, IsDefault, Base64Images FROM CardThemes";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                backup.Add(new LegacyData(
                    ThemeName: reader.GetString(0),
                    IsDefault: reader.GetBoolean(1),
                    Base64ImagesJson: reader.GetString(2) // original json data
                ));
            }
        }
        catch
        {
            //safe ignore
        }

        return backup;
    }

    private static void RestoreAndMerge(GameDbContext context, List<LegacyData> legacyData)
    {
        foreach (var legacy in legacyData)
        {
            if (legacy.IsDefault) continue;

            // Geramos novos IDs sincronizados para respeitar a nova restrição de Chave Estrangeira (FK)
            var novoManifestId = Guid.NewGuid().ToString();
            var novoThemeId = Guid.NewGuid().ToString();

            // 1. Reconstruímos a lista de imagens a partir do JSON/texto armazenado
            List<string> images;
            try
            {
                images = System.Text.Json.JsonSerializer.Deserialize<List<string>>(legacy.Base64ImagesJson)
                               ?? [];
            }
            catch
            {
                // Fallback 
                images = [.. legacy.Base64ImagesJson.Split(',')];
            }

            if (!images.Any()) continue;

            // 2. New structure assembly (Meta)
            var novoManifest = new CardThemeManifestEntity(novoManifestId)
            {
                ThemeName = legacy.ThemeName,
                IsDefault = false,
                PreviewBase64Image = images.First(), // Extrai o preview para otimizar o carrossel
                CardThemeId = novoThemeId
            };

            // 3. new structure assembly (Details)
            var novoCardTheme = new CardThemeEntity(novoThemeId)
            {
                Base64Images = images,
                ManifestId = novoManifestId
            };

            context.CardThemeManifests.Add(novoManifest);
            context.CardThemes.Add(novoCardTheme);
        }

        context.SaveChanges();
        Console.WriteLine($"Merge concluído com sucesso! {legacyData.Count} temas customizados foram migrados.");
    }
    private record LegacyData(string ThemeName, bool IsDefault, string Base64ImagesJson);

}
#pragma warning restore CA1860 
