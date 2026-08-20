using MemoAna.Backend.Application.Common.Abstractions;
using MemoAna.Backend.Application.Common.Contracts;
using MemoAna.Backend.Application.Common.Pipeline.Validation;
using MemoAna.Backend.Application.Identity.Abstractions;
using MemoAna.Backend.Application.Identity.Handlers;
using MemoAna.Backend.Application.Identity.Validators;
using MemoAna.Backend.Infrastructure.Common.Repository;
using MemoAna.Backend.Infrastructure.Common.UnitOfWork;
using MemoAna.Backend.Infrastructure.Identity.Models;
using MemoAna.Backend.Infrastructure.Identity.Options;
using MemoAna.Backend.Infrastructure.Identity.Services;
using MemoAna.Backend.Infrastructure.Persistence;
using MemoAna.Backend.Infrastructure.Persistence.Middlewares;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MudBlazor.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using MemoAna.Backend.Infrastructure.Persistence.Options;

namespace MemoAna.Backend.Composition.Extensions;

/// <summary>Adds MemoAna.Backend services to the web host.</summary>
public static class WebApplicationBuilderExtensions
{
    extension(WebApplicationBuilder builder)
    {
        /// <summary>Runs the MemoAna.Backend web application.</summary>
        /// <typeparam name="TProgram">The program type.</typeparam>
        /// <typeparam name="TApp">The root component.</typeparam>
        /// <returns>A task for application startup.</returns>
        public async Task RunMemoAnaAsync<TProgram, TApp>()
            where TProgram : class
            where TApp : IComponent
        {

            builder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
                
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();
            builder.Services.AddControllers();
            builder.Services.AddSignalR();
            builder.Services.AddOpenApi();
            builder.Services.AddMudServices();
            builder.Services.Configure<JwtOptions>(
                builder.Configuration.GetSection(
                    JwtOptions.SectionName));
            builder.Services.Configure<ConnectionStringsOptions>(
                builder.Configuration.GetSection(
                    ConnectionStringsOptions.SectionName));

            builder.Services.AddDbContext<MemoAnaDbContext>(
                options => {
                    if (builder.Environment.IsProduction())
                    {
                        ConnectionStringsOptions cs = builder.Configuration
                            .GetSection(ConnectionStringsOptions.SectionName)
                            .Get<ConnectionStringsOptions>()
                            ?? throw new InvalidOperationException(
                                "MemoAna ConnectionStrings configuration is missing.");

                        options.UseNpgsql(cs.MemoAna, sql => sql.CommandTimeout(90));
                    }
                    else if (builder.Environment.IsDevelopment())
                    {
                        string? ConnectionStrings__MemoAna = Environment.GetEnvironmentVariable("ConnectionStrings__MemoAna")?.ToString();
                        ArgumentNullException.ThrowIfNullOrEmpty(ConnectionStrings__MemoAna,nameof(ConnectionStrings__MemoAna));
                        ArgumentNullException.ThrowIfNullOrWhiteSpace(ConnectionStrings__MemoAna,nameof(ConnectionStrings__MemoAna));
                        options.UseNpgsql(ConnectionStrings__MemoAna, sql => sql.CommandTimeout(90));
                    }
                    else throw new InvalidOperationException("No data provider configured");
                });

            builder.Services.AddIdentityCore<User>(
                options =>
                {
                    options.User.RequireUniqueEmail = true;
                    options.SignIn.RequireConfirmedEmail = false;
                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.DefaultLockoutTimeSpan =
                        TimeSpan.FromMinutes(15);
                })
                .AddRoles<Role>()
                .AddEntityFrameworkStores<MemoAnaDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

            builder.Services.AddScoped<IIdentityService, IdentityService>();
            builder.Services.AddSingleton<IRevokedTokenStore, RevokedTokenStore>();
            builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
            builder.Services.AddScoped<IIdentityEmailSender, LoggingIdentityEmailSender>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            builder.Services.AddValidatorsFromAssemblyContaining<RegisterCommandValidator>();
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    JwtOptions jwt = builder.Configuration
                        .GetSection(JwtOptions.SectionName)
                        .Get<JwtOptions>()
                        ?? throw new InvalidOperationException(
                            "JWT configuration is missing.");

                    if (Encoding.UTF8.GetByteCount(jwt.Key) < 32)
                    {
                        throw new InvalidOperationException(
                            "Jwt:Key must contain 256 bits.");
                    }

                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey =
                                new SymmetricSecurityKey(
                                    Encoding.UTF8.GetBytes(
                                        jwt.Key)),
                            ValidateIssuer = true,
                            ValidIssuer = jwt.Issuer,
                            ValidateAudience = true,
                            ValidAudience = jwt.Audience,
                            ValidateLifetime = true,
                            ClockSkew = TimeSpan.FromSeconds(30)
                        };

                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = context =>
                        {
                            JwtSecurityToken? token =
                                context.SecurityToken
                                    as JwtSecurityToken;
                            string? tokenType = token?
                                .Claims
                                .FirstOrDefault(
                                    claim => claim.Type ==
                                        JwtRegisteredClaimNames.Typ)
                                ?.Value;

                            if (!string.Equals(
                                tokenType,
                                "access",
                                StringComparison.Ordinal))
                            {
                                context.Fail(
                                    "The token is not an access token.");
                                return Task.CompletedTask;
                            }

                            if (token is not null &&
                                context.HttpContext
                                    .RequestServices
                                    .GetRequiredService<
                                        IRevokedTokenStore>()
                                    .IsRevoked(token.Id))
                            {
                                context.Fail(
                                    "The access token "
                                    + "has been revoked.");
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            builder.Services.AddAuthorizationBuilder()
                .AddPolicy(
                    IdentityPolicies.Administrator,
                    policy => policy.RequireClaim(
                        IdentityClaimTypes.Permission,
                        "system.admin"))
                .AddPolicy(
                    IdentityPolicies.User,
                    policy => policy.RequireClaim(
                        IdentityClaimTypes.Permission,
                        "system.user"));

            builder.Services.AddMediator(options =>
            {
                options.ServiceLifetime =
                    ServiceLifetime.Scoped;
                options.Assemblies =
                    [typeof(IdentityHandlers).Assembly];
                options.PipelineBehaviors =
                [
                    typeof(ValidationMiddleware<,>),
                    typeof(TransactionMiddleware<,>)
                ];
            });

            await builder.Build().RunMemoAnaAsync<TApp>();
        }
    }
}
