namespace helpdesk_demo.Models;

public class Ticket
{
    public int Id { get; set; }
    public string Subject { get; set; } = "";
    public string Description { get; set; } = "";

    public TicketCategory Category { get; set; } = TicketCategory.Other;
    public TicketPriority Priority { get; set; } = TicketPriority.Normal;
    public TicketStatus Status { get; set; } = TicketStatus.New;

    public string SubmitterName { get; set; } = "";
    public string SubmitterEmail { get; set; } = "";
    public UserRole SubmitterRole { get; set; } = UserRole.Student;

    public string? AssignedAgent { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;

    public List<TicketComment> Comments { get; set; } = new();

    public string Reference => $"HD-{Id:D4}";

    public bool IsOpen => Status is not (TicketStatus.Resolved or TicketStatus.Closed);

    public string StatusBadgeClass => Status switch
    {
        TicketStatus.New => "bg-primary",
        TicketStatus.Open => "bg-info text-dark",
        TicketStatus.Pending => "bg-warning text-dark",
        TicketStatus.Resolved => "bg-success",
        TicketStatus.Closed => "bg-secondary",
        _ => "bg-secondary"
    };

    public string PriorityBadgeClass => Priority switch
    {
        TicketPriority.Low => "bg-light text-dark border",
        TicketPriority.Normal => "bg-secondary",
        TicketPriority.High => "bg-warning text-dark",
        TicketPriority.Urgent => "bg-danger",
        _ => "bg-secondary"
    };

    public string CategoryLabel => Category switch
    {
        TicketCategory.FinancialAid => "Financial Aid / GI Bill",
        TicketCategory.Account => "Account & Access",
        _ => Category.ToString()
    };
}
