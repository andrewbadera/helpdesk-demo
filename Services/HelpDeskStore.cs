using helpdesk_demo.Models;

namespace helpdesk_demo.Services;

/// <summary>
/// Thread-safe in-memory data store that mocks a help desk back end.
/// Data is seeded on startup and lives for the lifetime of the process,
/// which is plenty for a demo / prototype.
/// </summary>
public class HelpDeskStore
{
    private readonly object _lock = new();
    private readonly List<Ticket> _tickets = new();
    private readonly List<AppUser> _users = new();
    private int _nextTicketId = 1;
    private int _nextCommentId = 1;

    public HelpDeskStore()
    {
        Seed();
    }

    public IReadOnlyList<AppUser> Users
    {
        get { lock (_lock) { return _users.ToList(); } }
    }

    public IReadOnlyList<Ticket> GetTickets()
    {
        lock (_lock)
        {
            return _tickets
                .OrderByDescending(t => t.IsOpen)
                .ThenByDescending(t => t.Priority)
                .ThenByDescending(t => t.UpdatedUtc)
                .ToList();
        }
    }

    public IReadOnlyList<Ticket> GetTicketsForSubmitter(string email)
    {
        lock (_lock)
        {
            return _tickets
                .Where(t => string.Equals(t.SubmitterEmail, email, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(t => t.UpdatedUtc)
                .ToList();
        }
    }

    public Ticket? GetTicket(int id)
    {
        lock (_lock)
        {
            return _tickets.FirstOrDefault(t => t.Id == id);
        }
    }

    public Ticket CreateTicket(Ticket ticket)
    {
        lock (_lock)
        {
            ticket.Id = _nextTicketId++;
            ticket.CreatedUtc = DateTime.UtcNow;
            ticket.UpdatedUtc = DateTime.UtcNow;
            ticket.Status = TicketStatus.New;
            _tickets.Add(ticket);
            return ticket;
        }
    }

    public void AddComment(int ticketId, TicketComment comment)
    {
        lock (_lock)
        {
            var ticket = _tickets.FirstOrDefault(t => t.Id == ticketId);
            if (ticket is null) return;

            comment.Id = _nextCommentId++;
            comment.CreatedUtc = DateTime.UtcNow;
            ticket.Comments.Add(comment);
            ticket.UpdatedUtc = DateTime.UtcNow;

            // A submitter replying to a pending ticket reopens it for the agent.
            if (ticket.Status == TicketStatus.Pending && comment.AuthorRole != UserRole.Agent)
            {
                ticket.Status = TicketStatus.Open;
            }
        }
    }

    public void UpdateTicket(int ticketId, TicketStatus status, TicketPriority priority, string? assignedAgent)
    {
        lock (_lock)
        {
            var ticket = _tickets.FirstOrDefault(t => t.Id == ticketId);
            if (ticket is null) return;

            ticket.Status = status;
            ticket.Priority = priority;
            ticket.AssignedAgent = string.IsNullOrWhiteSpace(assignedAgent) ? null : assignedAgent;
            ticket.UpdatedUtc = DateTime.UtcNow;
        }
    }

    public HelpDeskStats GetStats()
    {
        lock (_lock)
        {
            return new HelpDeskStats
            {
                Total = _tickets.Count,
                Open = _tickets.Count(t => t.IsOpen),
                Unassigned = _tickets.Count(t => t.IsOpen && t.AssignedAgent is null),
                Urgent = _tickets.Count(t => t.IsOpen && t.Priority == TicketPriority.Urgent),
                Resolved = _tickets.Count(t => t.Status is TicketStatus.Resolved or TicketStatus.Closed)
            };
        }
    }

    private void Seed()
    {
        _users.AddRange(new[]
        {
            new AppUser { Id = 1, FullName = "Marcus Reyes", Email = "marcus.reyes@veteranscollege.edu", Role = UserRole.Student, MilitaryAffiliation = "U.S. Army - Veteran (GI Bill)", StudentId = "S-100245" },
            new AppUser { Id = 2, FullName = "Dr. Angela Brooks", Email = "angela.brooks@veteranscollege.edu", Role = UserRole.Faculty, MilitaryAffiliation = "U.S. Navy - Veteran", StudentId = "" },
            new AppUser { Id = 3, FullName = "Sam Okafor", Email = "sam.okafor@veteranscollege.edu", Role = UserRole.Agent, MilitaryAffiliation = "Help Desk - Tier 1", StudentId = "" },
        });

        var t1 = CreateTicket(new Ticket
        {
            Subject = "Can't log in to the student portal after deployment",
            Description = "I just got back from a deployment and my account seems locked. I've tried resetting twice but never get the email.",
            Category = TicketCategory.Account,
            Priority = TicketPriority.High,
            SubmitterName = "Marcus Reyes",
            SubmitterEmail = "marcus.reyes@veteranscollege.edu",
            SubmitterRole = UserRole.Student
        });
        t1.Status = TicketStatus.Open;
        t1.AssignedAgent = "Sam Okafor";
        AddComment(t1.Id, new TicketComment { AuthorName = "Sam Okafor", AuthorRole = UserRole.Agent, Body = "Thanks for your service, Marcus. I've unlocked your account and sent a fresh reset link to your personal email on file. Let me know if it doesn't arrive in 10 minutes." });

        var t2 = CreateTicket(new Ticket
        {
            Subject = "GI Bill tuition assistance not reflected on account balance",
            Description = "My TA paperwork was approved by the VA but my balance still shows the full amount due. Worried about being dropped.",
            Category = TicketCategory.FinancialAid,
            Priority = TicketPriority.Urgent,
            SubmitterName = "Marcus Reyes",
            SubmitterEmail = "marcus.reyes@veteranscollege.edu",
            SubmitterRole = UserRole.Student
        });
        t2.Status = TicketStatus.Pending;
        t2.AssignedAgent = "Sam Okafor";
        AddComment(t2.Id, new TicketComment { AuthorName = "Sam Okafor", AuthorRole = UserRole.Agent, Body = "I've escalated this to the VA certifying official. Can you confirm the date your TA was approved so I can match it to our records?" });
        AddComment(t2.Id, new TicketComment { AuthorName = "Sam Okafor", AuthorRole = UserRole.Agent, Body = "Note: hold on any late fees until financial aid confirms TA posting.", IsInternalNote = true });

        var t3 = CreateTicket(new Ticket
        {
            Subject = "LMS gradebook not showing my submitted assignment",
            Description = "I submitted my week 3 essay in the LMS but it shows as missing. I have the confirmation screenshot.",
            Category = TicketCategory.Technical,
            Priority = TicketPriority.Normal,
            SubmitterName = "Marcus Reyes",
            SubmitterEmail = "marcus.reyes@veteranscollege.edu",
            SubmitterRole = UserRole.Student
        });
        t3.Status = TicketStatus.New;

        var t4 = CreateTicket(new Ticket
        {
            Subject = "Need to bulk-upload grades for HIST-210",
            Description = "Requesting access to the CSV grade import tool so I can post midterm grades for 40 students.",
            Category = TicketCategory.Coursework,
            Priority = TicketPriority.Low,
            SubmitterName = "Dr. Angela Brooks",
            SubmitterEmail = "angela.brooks@veteranscollege.edu",
            SubmitterRole = UserRole.Faculty
        });
        t4.Status = TicketStatus.Resolved;
        t4.AssignedAgent = "Sam Okafor";
        AddComment(t4.Id, new TicketComment { AuthorName = "Sam Okafor", AuthorRole = UserRole.Agent, Body = "Access granted. The CSV import tool is now visible under your course tools menu. Closing this out — reopen if you hit any snags." });

        var t5 = CreateTicket(new Ticket
        {
            Subject = "Request official transcript for VA education benefits",
            Description = "I need an official transcript sent to the VA regional office for my benefits review.",
            Category = TicketCategory.Records,
            Priority = TicketPriority.Normal,
            SubmitterName = "Marcus Reyes",
            SubmitterEmail = "marcus.reyes@veteranscollege.edu",
            SubmitterRole = UserRole.Student
        });
        t5.Status = TicketStatus.New;
    }
}

public class HelpDeskStats
{
    public int Total { get; set; }
    public int Open { get; set; }
    public int Unassigned { get; set; }
    public int Urgent { get; set; }
    public int Resolved { get; set; }
}
