using System;
using System.Collections.Generic;
using System.Text;
using Battleship.Business.Models;
using Battleship.Business.Models.Contracts;
using Battleship.Business.Services;
using Battleship.Business.Services.Contracts;
using Battleship.Data;
using Battleship.Data.Repositories;
using Battleship.Domain;
using Battleship.Domain.GameDomain;
using Battleship.Domain.GameDomain.Contracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using NSwag.Generation.Processors.Security;

namespace Battleship.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(options =>
            {
                var policy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser().Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            });
            services.AddCors();
            services.AddDbContext<BattleshipContext>(options =>
            {
                string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=BattleshipDb;Integrated Security=True";
                options.UseSqlServer(connectionString);
            });
            services.AddIdentity<User, IdentityRole<Guid>>(options =>
                {
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                    options.Lockout.MaxFailedAccessAttempts = 8;
                    options.Lockout.AllowedForNewUsers = true;

                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireDigit = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequiredLength = 5;

                    options.SignIn.RequireConfirmedEmail = false;
                    options.SignIn.RequireConfirmedPhoneNumber = false;
                })
                .AddEntityFrameworkStores<BattleshipContext>()
                .AddDefaultTokenProviders();

            var tokenSettings = new TokenSettings();
            Configuration.Bind("Token", tokenSettings);

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = tokenSettings.Issuer,
                        ValidAudience = tokenSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings.Key)),
                    };
                });

            services.AddOpenApiDocument(config =>
            {
                config.PostProcess = document =>
                {
                    document.SecurityDefinitions.Add("bearer", new OpenApiSecurityScheme
                    {
                        Type = OpenApiSecuritySchemeType.ApiKey,
                        Name = "Authorization",
                        Description =
                            "Copy 'Bearer ' + valid token into field. You can retrieve a bearer token via '/api/authentication/token'",
                        In = OpenApiSecurityApiKeyLocation.Header
                    });
                    document.Schemes = new List<OpenApiSchema> { OpenApiSchema.Https };
                    document.Info.Title = "Battleship Api";
                    document.Info.Description =
                        "Data services for the Battleship web application";
                };
                config.OperationProcessors.Add(new OperationSecurityScopeProcessor("bearer"));
            });

            services.AddSingleton<ITokenFactory>(new JwtTokenFactory(tokenSettings));
            services.AddSingleton<IGameRepository, InMemoryGameRepository>();
            services.AddSingleton<IGameService, GameService>();
            services.AddSingleton<IGameInfoFactory, GameInfoFactory>();
            services.AddSingleton<IGridInfoFactory, GridInfoFactory>();
            services.AddSingleton<IShipInfoFactory, ShipInfoFactory>();
            services.AddSingleton<IGameFactory, GameFactory>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader());
            app.UseHttpsRedirection();

            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
