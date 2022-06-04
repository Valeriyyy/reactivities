using API.Extensions;
using API.Middleware;
using API.SignalR;
using Application.Activities;
using Application.Core;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Persistence;

namespace API
{
	public class Startup
	{
		private readonly IConfiguration _config;
		public Startup(IConfiguration config) {
			_config = config;
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services) {
			services.AddControllers(options => {
				var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
				options.Filters.Add(new AuthorizeFilter(policy));
			})
			.AddFluentValidation(config => {
				config.RegisterValidatorsFromAssemblyContaining<Create>();
			});
			services.AddRouting(options => options.LowercaseUrls = true);
			services.AddApplicationServices(_config);
			services.AddIdentityServices(_config);
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
			app.UseMiddleware<ExceptionMiddleware>();

			if (env.IsDevelopment()) {
				app.UseSwagger();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPIv5 v1"));
			}

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseCors("CorsPolicy");

			// authentication must come before authorization
			//   <!-- <PackageReference Include="Microsoft.IdentityModel.Tokens" Version="6.16.0" /> -->
			app.UseAuthentication();

			app.UseAuthorization();

			app.UseEndpoints(endpoints => {
				endpoints.MapControllers();
				endpoints.MapHub<ChatHub>("/chat");
			});
		}
	}
}
