using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppSage.Web.Pages.Settings
{
    public class ProfileModel : PageModel
    {
        public ProfileData Profile { get; set; }

        public void OnGet()
        {
            // Populate the Profile property with sample data
            Profile = new ProfileData
            {
                FirstName = "John",
                LastName = "Smith",
                Email = "john.smith@appsage.com",
                Phone = "+1 (555) 123-4567",
                JobTitle = "Project Manager",
                Department = "Management",
                Bio = "Experienced project manager with 8+ years in software development. Passionate about team collaboration and delivering high-quality products on time.",
                Projects = 24,
                Tasks = 142,
                Teams = 8
            };
        }
    }
}
