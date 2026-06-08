namespace helpdesk_demo.Models;

public enum UserRole
{
    Student,
    Faculty,
    Agent
}

public enum TicketStatus
{
    New,
    Open,
    Pending,      // waiting on submitter
    Resolved,
    Closed
}

public enum TicketPriority
{
    Low,
    Normal,
    High,
    Urgent
}

public enum TicketCategory
{
    Account,            // login / password / MFA
    Enrollment,
    FinancialAid,       // includes GI Bill / Tuition Assistance
    Technical,          // LMS, hardware, software
    Coursework,
    Records,            // transcripts, certificates
    Other
}
