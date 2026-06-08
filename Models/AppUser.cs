namespace helpdesk_demo.Models;

public class AppUser
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public UserRole Role { get; set; }

    /// <summary>
    /// Branch of service or veteran status shown on the profile to give
    /// help desk staff useful context (e.g. "U.S. Army - Active Duty").
    /// </summary>
    public string MilitaryAffiliation { get; set; } = "";

    public string StudentId { get; set; } = "";

    public string Initials
    {
        get
        {
            var parts = FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "?";
            if (parts.Length == 1) return char.ToUpperInvariant(parts[0][0]).ToString();
            return $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[^1][0])}";
        }
    }
}
