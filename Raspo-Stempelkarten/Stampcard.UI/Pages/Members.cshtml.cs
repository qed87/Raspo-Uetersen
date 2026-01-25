using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Stampcard.UI.Pages;

public class Members : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string Team { get; set; }
    
    [BindProperty]
    public Member NewMember { get; set; }

    public List<Member> Items { get; set; } = [];
    
    public void OnGet()
    {
        Items.Add(new Member
        {
            Id = Guid.NewGuid(),
            FirstName = "Franz Leopold",
            Surname = "Kirchner",
            Birthdate = new DateOnly(2017, 8, 13),
            Birthplace = "Hamburg"
        });
        
        Items.Add(new Member
        {
            Id = Guid.NewGuid(),
            FirstName = "Julius Richard",
            Surname = "Kirchner",
            Birthdate = new DateOnly(2020, 9, 7),
            Birthplace = "Pinneberg"
        });
        
    }

    public IActionResult OnPost()
    {
        if (ModelState.IsValid)
        {
            // Member Speichern
            return RedirectToPage();
        }
        
        return Page();
    }
    
    public IActionResult OnGetDelete(Guid id)
    {
        return RedirectToPage();
    }
    
    public class Member
    {
        public Guid? Id { get; set; }
        
        [Required]
        public string FirstName { get; set; }
        
        [Required]
        public string Surname { get; set; }
        
        [Required]
        [DataType(DataType.Date)]
        public DateOnly Birthdate { get; set; }
        
        [Required]
        [MinLength(2)]
        public string Birthplace { get; set; }
    }
}