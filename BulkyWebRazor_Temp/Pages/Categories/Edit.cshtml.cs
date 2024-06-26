using BulkyWebRazor_Temp.Data;
using BulkyWebRazor_Temp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazor_Temp.Pages.Categories
{
	[BindProperties]
    public class EditModel : PageModel
    {
		private readonly ApplicationDbContext _context;
		public Category Category { get; set; }

		public EditModel(ApplicationDbContext applicationDbContext)
		{
			_context = applicationDbContext;
		}
		public void OnGet(int? id)
		{
			if(id is not null)
			{
				Category = _context.Categories.FirstOrDefault(c => c.Id == id)!;
			}
		}

		public IActionResult OnPost()
		{
			if (ModelState.IsValid)
			{
				_context.Categories.Update(Category!);
				_context.SaveChanges();

				TempData["success"] = "Category edited successfully!";

				return RedirectToPage("Index");
			}
			return Page();
		}
    }
}
