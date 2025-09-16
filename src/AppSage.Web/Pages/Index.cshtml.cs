using AppSage.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppSage.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }



        /// <summary>
        /// Handles the initial GET request to the page
        /// </summary>
        public void OnGet()
        {
          
        }        

    }
}
