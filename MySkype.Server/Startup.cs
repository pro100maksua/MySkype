using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MySkype.Server.Data;
using MySkype.Server.Data.Interfaces;
using MySkype.Server.Data.Models;
using MySkype.Server.Data.Repositories;
using MySkype.Server.Logic.Interfaces;
using MySkype.Server.Logic.Services;
using MySkype.Server.Logic.WebSocketManagers;
using MySkype.Server.Middleware;
using Swashbuckle.AspNetCore.Swagger;
using WebSocketManager = MySkype.Server.Logic.WebSocketManagers.WebSocketManager;

namespace MySkype.Server
{
    public class Startup
    {
        private readonly IConfiguration _config;

        public Startup(IConfiguration config)
        {
            _config = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddIdentityCore<User>(options =>
            {
                options.Password.RequiredLength = 5;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
            });
            services.AddScoped<IUserStore<User>, UserStore>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "MySkype.Server" });

                var security = new Dictionary<string, IEnumerable<string>>
                {
                    {"Bearer", new string[] { }}
                };

                c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });
                c.AddSecurityRequirement(security);
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = _config["Domain"],
                        ValidAudience = _config["Domain"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(_config["Identity:SecurityKey"]))
                    };
                });

            services.AddCors(c => c.AddPolicy("Policy", builder =>
            {
                builder.AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowAnyOrigin();
            }));

            services.AddTransient<IUsersRepository, UsersRepository>();
            services.AddTransient<ICallsRepository, CallsRepository>();
            services.AddTransient<IUserFriendsService,UserFriendsService>();
            services.AddTransient<ICallsService,CallsService>();
            services.AddTransient<IPhotoService,PhotoService>();
            services.AddTransient<IUserService,UserService>();
            services.AddTransient<IIdentityService,IdentityService>();
            services.AddTransient<MongoContext>();

            services.AddSingleton<WebSocketManager>();
            services.AddSingleton<WebSocketVideoManager>();
            services.AddSingleton<WebSocketConnectionManager>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("Policy");
            app.UseAuthentication();
            app.UseWebSockets();
            app.Map("/general",
                builder => builder.UseMiddleware<WebSocketMiddleware>(serviceProvider.GetService<WebSocketManager>()));
            app.Map("/video",
                builder => builder.UseMiddleware<WebSocketMiddleware>(serviceProvider.GetService<WebSocketVideoManager>()));
            app.UseSwagger();
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", ""); });

            app.UseMvc();
        }
    }
}