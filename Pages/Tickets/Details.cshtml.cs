using System.ComponentModel.DataAnnotations;
using helpdesk_demo.Models;
using helpdesk_demo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace helpdesk_demo.Pages.Tickets;

public class DetailsModel : PageModel
{
    private readonly HelpDeskStore _store;
    private readonly CurrentUserService _currentUser;

    public DetailsModel(HelpDeskStore store, CurrentUserService currentUser)
    {
        _store = store;
        _currentUser = currentUser;
    }

    public AppUser Me => _currentUser.Current;
    public bool IsAgent => _currentUser.IsAgent;

    public Ticket Ticket { get; private set; } = null!;

    [BindProperty]
    public string? Reply { get; set; }

    [BindProperty]
    public bool IsInternalNote { get; set; }

    [BindProperty]
    public TicketStatus Status { get; set; }

    [BindProperty]
    public TicketPriority Priority { get; set; }

    [BindProperty]
    public string? AssignedAgent { get; set; }

    public List<string> Agents { get; private set; } = new();

    private bool CanView(Ticket ticket) =>
        IsAgent || string.Equals(ticket.SubmitterEmail, Me.Email, StringComparison.OrdinalIgnoreCase);

    public IActionResult OnGet(int id)
    {
        var ticket = _store.GetTicket(id);
        if (ticket is null || !CanView(ticket))
        {
            return NotFound();
        }

        Ticket = ticket;
        Status = ticket.Status;
        Priority = ticket.Priority;
        AssignedAgent = ticket.AssignedAgent;
        LoadAgents();
        return Page();
    }

    public IActionResult OnPostReply(int id)
    {
        var ticket = _store.GetTicket(id);
        if (ticket is null || !CanView(ticket))
        {
            return NotFound();
        }

        if (!string.IsNullOrWhiteSpace(Reply))
        {
            _store.AddComment(id, new TicketComment
            {
                AuthorName = Me.FullName,
                AuthorRole = Me.Role,
                Body = Reply.Trim(),
                // Only agents can post internal notes.
                IsInternalNote = IsAgent && IsInternalNote
            });
        }

        return RedirectToPage(new { id });
    }

    public IActionResult OnPostUpdate(int id)
    {
        var ticket = _store.GetTicket(id);
        if (ticket is null)
        {
            return NotFound();
        }

        // Only agents can change ticket workflow fields.
        if (!IsAgent)
        {
            return Forbid();
        }

        _store.UpdateTicket(id, Status, Priority, AssignedAgent);
        TempData["Flash"] = "Ticket updated.";
        return RedirectToPage(new { id });
    }

    private void LoadAgents()
    {
        Agents = _store.Users
            .Where(u => u.Role == UserRole.Agent)
            .Select(u => u.FullName)
            .ToList();
    }
}
