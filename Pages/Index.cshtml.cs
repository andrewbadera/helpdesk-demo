using helpdesk_demo.Models;
using helpdesk_demo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace helpdesk_demo.Pages;

public class IndexModel : PageModel
{
    private readonly HelpDeskStore _store;
    private readonly CurrentUserService _currentUser;

    public IndexModel(HelpDeskStore store, CurrentUserService currentUser)
    {
        _store = store;
        _currentUser = currentUser;
    }

    public AppUser Me => _currentUser.Current;
    public bool IsAgent => _currentUser.IsAgent;
    public HelpDeskStats Stats { get; private set; } = new();

    public IReadOnlyList<Ticket> MyRecentTickets { get; private set; } = new List<Ticket>();
    public IReadOnlyList<Ticket> AgentQueue { get; private set; } = new List<Ticket>();

    public void OnGet()
    {
        Stats = _store.GetStats();

        if (_currentUser.IsAgent)
        {
            AgentQueue = _store.GetTickets()
                .Where(t => t.IsOpen)
                .Take(6)
                .ToList();
        }
        else
        {
            MyRecentTickets = _store.GetTicketsForSubmitter(Me.Email)
                .Take(6)
                .ToList();
        }
    }

    public IActionResult OnPostSwitchPersona(int userId)
    {
        _currentUser.SetPersona(userId);
        return RedirectToPage("/Index");
    }
}
