using AIHUB_Server.Application;
using AIHUB_Server.Application.Common.Interfaces;
using AIHUB_Server.Common.Helpers;
using AIHUB_Server.Controllers;
using AIHUB_Server.Infrastructure;
using AIHUB_Server.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.OpenApi.Models;

namespace AIHUB_Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration);
            
            builder.Services.AddSignalR();

            // Services are added in .AddApplication() or .AddInfrastructure() by now.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSignalRSwaggerGen();

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type=ReferenceType.SecurityScheme,
                                Id="Bearer"
                            }
                        },
                        new string[]{}
                    }
                });
            });

            // CORS
            builder.Services.AddCors(p => p.AddPolicy("cors", policy =>
            {
                policy
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            }));

            builder.Services.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = int.MaxValue; // In case of multipart, dummy "fix", TODO: make / find system for partial uploading
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // CORS
            app.UseCors("cors");
            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapHub<SignalRCmd>("/streamcmd"); // SignalR

            app.UseMiddleware<JwtMiddleware>(); // Our middleware for JWT token authorization.

            app.MapControllers();

            app.Run();
        }
    }
}