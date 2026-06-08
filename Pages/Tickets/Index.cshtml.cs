using helpdesk_demo.Models;
using helpdesk_demo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace helpdesk_demo.Pages.Tickets;

public class IndexModel : PageModel
{
    private readonly HelpDeskStore _store;
    private readonly CurrentUserService _currentUser;

    public IndexModel(HelpDeskStore store, CurrentUserService currentUser)
    {
        _store = store;
        _currentUser = currentUser;
    }

    public bool IsAgent => _currentUser.IsAgent;
    public AppUser Me => _currentUser.Current;

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool MineOnly { get; set; }

    public IReadOnlyList<Ticket> Tickets { get; private set; } = new List<Ticket>();

    public void OnGet()
    {
        // Students and faculty only ever see their own tickets. Agents see
        // everything but can choose to filter to tickets assigned to them.
        IEnumerable<Ticket> query = IsAgent
            ? _store.GetTickets()
            : _store.GetTicketsForSubmitter(Me.Email);

        if (IsAgent && MineOnly)
        {
            query = query.Where(t => t.AssignedAgent == Me.FullName);
        }

        if (!string.IsNullOrWhiteSpace(StatusFilter) &&
            Enum.TryParse<TicketStatus>(StatusFilter, out var status))
        {
            query = query.Where(t => t.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var term = Search.Trim();
            query = query.Where(t =>
                t.Subject.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                t.Description.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                t.Reference.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                t.SubmitterName.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        Tickets = query.ToList();
    }
}
