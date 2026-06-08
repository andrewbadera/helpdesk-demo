using System.ComponentModel.DataAnnotations;
using helpdesk_demo.Models;
using helpdesk_demo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace helpdesk_demo.Pages.Tickets;

public class CreateModel : PageModel
{
    private readonly HelpDeskStore _store;
    private readonly CurrentUserService _currentUser;

    public CreateModel(HelpDeskStore store, CurrentUserService currentUser)
    {
        _store = store;
        _currentUser = currentUser;
    }

    public AppUser Me => _currentUser.Current;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [StringLength(120, MinimumLength = 5)]
        [Display(Name = "Subject")]
        public string Subject { get; set; } = "";

        [Required]
        [StringLength(4000, MinimumLength = 10)]
        [Display(Name = "Describe the issue")]
        public string Description { get; set; } = "";

        [Display(Name = "Category")]
        public TicketCategory Category { get; set; } = TicketCategory.Account;

        [Display(Name = "Priority")]
        public TicketPriority Priority { get; set; } = TicketPriority.Normal;
    }

    public void OnGet(TicketCategory? category)
    {
        if (category.HasValue)
        {
            Input.Category = category.Value;
        }
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var ticket = _store.CreateTicket(new Ticket
        {
            Subject = Input.Subject.Trim(),
            Description = Input.Description.Trim(),
            Category = Input.Category,
            Priority = Input.Priority,
            SubmitterName = Me.FullName,
            SubmitterEmail = Me.Email,
            SubmitterRole = Me.Role
        });

        TempData["Flash"] = $"Ticket {ticket.Reference} submitted. We'll be in touch soon.";
        return RedirectToPage("/Tickets/Details", new { id = ticket.Id });
    }
}
