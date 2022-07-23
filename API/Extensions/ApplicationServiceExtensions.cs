using Application.Activities;
using Application.Core;
using Infrastructure.Photos;
using Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Persistence;
using Security;

namespace API.Extensions;
public static class ApplicationServiceExtensions
{
	public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config) {
		services.AddSwaggerGen(c => {
			c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebAPIv5", Version = "v1" });
		});
		// services.AddDbContext<DataContext>(options => {
		// 	// options.UseSqlite(config.GetConnectionString("SqliteConnection"));
		// 	options.UseNpgsql(config.GetConnectionString("PostgresConnection"));
		// });
		services.AddDbContext<DataContext>(options => {
			var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
			string connUrl;

			// Depending on if in development or production, use either Heroku-provided
			// connection string, or development connection string from env var.
			if (env == "Development") {
				// Use connection string from file.
				connUrl = config.GetConnectionString("DATABASE_URL");
			} else {
				// Use connection string provided at runtime by Heroku.
				connUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
				// Parse connection URL to connection string for Npgsql
			}
			connUrl = connUrl.Replace("postgres://", string.Empty);
			var pgUserPass = connUrl.Split("@")[0];
			var pgHostPortDb = connUrl.Split("@")[1];
			var pgHostPort = pgHostPortDb.Split("/")[0];
			var pgDb = pgHostPortDb.Split("/")[1];
			var pgUser = pgUserPass.Split(":")[0];
			var pgPass = pgUserPass.Split(":")[1];
			var pgHost = pgHostPort.Split(":")[0];
			var pgPort = pgHostPort.Split(":")[1];

			var connStr = $"Server={pgHost};Port={pgPort};User Id={pgUser};Password={pgPass};Database={pgDb}; SSL Mode=Require; Trust Server Certificate=true";

			// Whether the connection string came from the local development configuration file
			// or from the environment variable from Heroku, use it to set up your DbContext.
			options.UseNpgsql(connStr);
		});

		services.AddCors(opt => {
			opt.AddPolicy("CorsPolicy", policy => {
				policy.AllowAnyMethod().AllowAnyHeader().AllowCredentials().AllowAnyOrigin().WithOrigins("http://localhost:3000");
			});
		});
		services.AddMediatR(typeof(List.Handler).Assembly);
		services.AddAutoMapper(typeof(MappingProfiles).Assembly);
		services.AddScoped<IUserAccessor, UserAccessor>();
		services.AddScoped<IPhotoAccessor, PhotoAccessor>();
		services.Configure<CloudinarySettings>(config.GetSection("Cloudinary"));
		services.AddSignalR();

		return services;
	}
}
