namespace helpdesk_demo.Models;

public class TicketComment
{
    public int Id { get; set; }
    public string AuthorName { get; set; } = "";
    public UserRole AuthorRole { get; set; }
    public string Body { get; set; } = "";
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Internal notes are visible only to help desk agents, not the submitter.
    /// </summary>
    public bool IsInternalNote { get; set; }
}
