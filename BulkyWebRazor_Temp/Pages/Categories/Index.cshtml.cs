using BulkyWebRazor_Temp.Data;
using BulkyWebRazor_Temp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazor_Temp.Pages.Categories
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public List<Category> CategoryList { get; set; }

        public IndexModel(ApplicationDbContext applicationDbContext)
        {
            _context = applicationDbContext;
        }
        public void OnGet()
        {
            CategoryList = _context.Categories.ToList();
        }
    }
}
