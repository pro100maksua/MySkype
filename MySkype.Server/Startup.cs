using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MySkype.Server.Interfaces;
using MySkype.Server.Middleware;
using MySkype.Server.Repositories;
using MySkype.Server.Services;
using MySkype.Server.WebSocketManagers;
using Swashbuckle.AspNetCore.Swagger;
using WebSocketManager = MySkype.Server.WebSocketManagers.WebSocketManager;

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
                builder
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowAnyOrigin();
            }));

            services.AddTransient<IUsersRepository, UsersRepository>();
            services.AddTransient<IPhotoRepository, PhotoRepository>();
            services.AddTransient<ICallsRepository, CallsRepository>();
            services.AddTransient<UserFriendsService>();
            services.AddTransient<CallsService>();
            services.AddTransient<PhotoService>();
            services.AddTransient<UserService>();
            services.AddTransient<IdentityService>();
            services.AddTransient<MongoContext>();

            services.AddSingleton<WebSocketManager>();
            services.AddSingleton<WebSocketVideoManager>();
            services.AddTransient<WebSocketConnectionManager>();

            services.AddAutoMapper();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseAuthentication();
            app.UseCors("Policy");
            app.UseWebSockets();
            app.Map("/general",
                builder => builder.UseMiddleware<WebSocketMiddleware>(serviceProvider.GetService<WebSocketManager>()));
            app.Map("/video",
                builder => builder.UseMiddleware<WebSocketMiddleware>(serviceProvider.GetService<WebSocketVideoManager>()));
            app.UseSwagger();
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", ""); });
            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
