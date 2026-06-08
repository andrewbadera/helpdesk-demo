using helpdesk_demo.Models;

namespace helpdesk_demo.Services;

/// <summary>
/// Resolves the "logged in" persona for the demo. A real app would use
/// ASP.NET Core authentication; here we let the visitor switch between a
/// student, a faculty member, and a help desk agent to exercise both sides
/// of the help desk experience.
/// </summary>
public class CurrentUserService
{
    private const string SessionKey = "personaUserId";
    private readonly IHttpContextAccessor _accessor;
    private readonly HelpDeskStore _store;

    public CurrentUserService(IHttpContextAccessor accessor, HelpDeskStore store)
    {
        _accessor = accessor;
        _store = store;
    }

    public AppUser Current
    {
        get
        {
            var session = _accessor.HttpContext?.Session;
            var id = session?.GetInt32(SessionKey) ?? _store.Users.First().Id;
            return _store.Users.FirstOrDefault(u => u.Id == id) ?? _store.Users.First();
        }
    }

    public bool IsAgent => Current.Role == UserRole.Agent;

    public void SetPersona(int userId)
    {
        var session = _accessor.HttpContext?.Session;
        if (session is not null && _store.Users.Any(u => u.Id == userId))
        {
            session.SetInt32(SessionKey, userId);
        }
    }
}
