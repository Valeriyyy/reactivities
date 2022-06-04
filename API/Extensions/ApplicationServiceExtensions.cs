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
		services.AddDbContext<DataContext>(options => {
			options.UseSqlite(config.GetConnectionString("DefaultConnection"));
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
