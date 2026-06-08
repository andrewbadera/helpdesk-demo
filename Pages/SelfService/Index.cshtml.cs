using helpdesk_demo.Models;
using helpdesk_demo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace helpdesk_demo.Pages.SelfService;

public class IndexModel : PageModel
{
    private readonly CurrentUserService _currentUser;

    public IndexModel(CurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public AppUser Me => _currentUser.Current;

    public string? ResultTitle { get; private set; }
    public string? ResultMessage { get; private set; }

    public void OnGet()
    {
        if (TempData["ResultTitle"] is string title)
        {
            ResultTitle = title;
            ResultMessage = TempData["ResultMessage"] as string;
        }
    }

    public IActionResult OnPostResetPassword()
    {
        TempData["ResultTitle"] = "Password reset link sent";
        TempData["ResultMessage"] =
            $"We emailed a secure reset link to {Me.Email}. It expires in 30 minutes. " +
            "If it doesn't arrive, check your spam folder or submit a ticket.";
        return RedirectToPage();
    }

    public IActionResult OnPostResetMfa()
    {
        TempData["ResultTitle"] = "MFA reset started";
        TempData["ResultMessage"] =
            "Your multi-factor authentication has been reset. The next time you sign in, " +
            "you'll be prompted to enroll a new device.";
        return RedirectToPage();
    }

    public IActionResult OnPostUpdateEmail(string? contactEmail)
    {
        if (string.IsNullOrWhiteSpace(contactEmail) || !contactEmail.Contains('@'))
        {
            TempData["ResultTitle"] = "Couldn't update email";
            TempData["ResultMessage"] = "Please enter a valid email address.";
            return RedirectToPage();
        }

        TempData["ResultTitle"] = "Contact email updated";
        TempData["ResultMessage"] =
            $"Your contact email is now {contactEmail.Trim()}. Benefit and enrollment notices will go there.";
        return RedirectToPage();
    }

    public IActionResult OnPostUnlock()
    {
        TempData["ResultTitle"] = "Account unlocked";
        TempData["ResultMessage"] =
            "Your account lockout has been cleared. You can sign in again right away.";
        return RedirectToPage();
    }
}
