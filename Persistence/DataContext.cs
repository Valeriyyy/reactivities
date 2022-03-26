using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Persistence;

public class DataContext : IdentityDbContext<AppUser>
{
	public DataContext() { }

	public DataContext(DbContextOptions options) : base(options) { }

	protected override void OnConfiguring(DbContextOptionsBuilder options) {
		if (!options.IsConfigured) {
			options.UseSqlite("A FALLBACK CONNECTION STRING");
		}
	}

	public DbSet<Activity> Activities { get; set; }
}