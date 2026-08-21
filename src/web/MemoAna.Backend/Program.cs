using MemoAna.Backend.Components;
using MemoAna.Backend.Composition.Extensions;
using MudBlazor.Services;
await WebApplication.CreateBuilder()
    .RunMemoAnaAsync<Program, App>(
    builder => builder.Services.AddMudServices());