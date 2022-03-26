using Microsoft.AspNetCore.Identity;

namespace Domain;

public class AppUser : IdentityUser
{
	public string Displayname { get; set; }
	public string Bio { get; set; }
}