using API.Extensions;
using API.Middleware;
using API.SignalR;
using Application.Activities;
using Domain;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options => {
	var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
	options.Filters.Add(new AuthorizeFilter(policy));
})
			.AddFluentValidation(config => {
				config.RegisterValidatorsFromAssemblyContaining<Create>();
			});
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);

var app = builder.Build();
// middleware
app.UseMiddleware<ExceptionMiddleware>();

app.UseXContentTypeOptions();
app.UseReferrerPolicy(opt => opt.NoReferrer());
app.UseXXssProtection(opt => opt.EnabledWithBlockMode());
app.UseXfo(opt => opt.Deny());
app.UseCspReportOnly(opt => opt
	.BlockAllMixedContent()
	.StyleSources(s => s.Self().CustomSources("https://fonts.googleapis.com"))
	.FontSources(s => s.Self().CustomSources("https://fonts.gstatic.com", "data:"))
	.FormActions(s => s.Self())
	.FrameAncestors(s => s.Self())
	.ImageSources(s => s.Self().CustomSources("https://res.cloudinary.com", "data:"))
	.ScriptSources(s => s.Self())
// .ScriptSources(s => s.Self().CustomSources("some sha hash))
);

if (app.Environment.IsDevelopment()) {
	app.UseSwagger();
	app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPIv5 v1"));
} else {
	app.Use(async (context, next) => {
		context.Response.Headers.Add("Strict-Transport-Security", "max-age=3153600");
		await next.Invoke();
	});
}

app.UseHttpsRedirection();

// commands used for running the react app
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors("CorsPolicy");

// authentication must come before authorization
//   <!-- <PackageReference Include="Microsoft.IdentityModel.Tokens" Version="6.16.0" /> -->
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chat");
app.MapFallbackToController("Index", "Fallback");

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

try {
	var context = services.GetRequiredService<DataContext>();
	var userManager = services.GetRequiredService<UserManager<AppUser>>();
	await context.Database.MigrateAsync();
	await Seed.SeedData(context, userManager);
} catch (Exception ex) {
	var logger = services.GetRequiredService<ILogger<Program>>();
	logger.LogError(ex, "An error occured during migration");
}

await app.RunAsync();
