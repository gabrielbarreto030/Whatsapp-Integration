using System.Collections.Concurrent;
using System.Net.Mail;

namespace BikeRentalApi.Services;

public class ConversationService(WhatsAppService whatsApp)
{
    private readonly ConcurrentDictionary<string, ConversationSession> _sessions = new();

    public async Task HandleAsync(string from, string text)
    {
        var session = _sessions.GetOrAdd(from, _ => new ConversationSession());
        text = text.Trim();

        switch (session.State)
        {
            case ConversationState.None:
                if (text.Contains("quote", StringComparison.OrdinalIgnoreCase))
                {
                    session.State = ConversationState.WaitingName;
                    await Send(from,
                        "Hi! I'd be happy to help you with a quote.\n\n" +
                        "First, could you share your *full name*?\n\n" +
                        "Example: _John Smith_");
                }
                break;

            case ConversationState.WaitingName:
                if (IsValidName(text))
                {
                    session.Name = text;
                    session.State = ConversationState.WaitingEmail;
                    await Send(from,
                        $"Thanks, *{text}*!\n\n" +
                        "Now, what is your *email address*?\n\n" +
                        "Example: _john@email.com_");
                }
                else
                {
                    await Send(from,
                        "That doesn't look like a valid name. Please enter your first and last name (letters only).\n\n" +
                        "Example: _John Smith_");
                }
                break;

            case ConversationState.WaitingEmail:
                if (IsValidEmail(text))
                {
                    session.Email = text;
                    session.State = ConversationState.WaitingBirthDate;
                    await Send(from,
                        "Got it!\n\n" +
                        "Last one — what is your *date of birth*?\n\n" +
                        "Example: _03/15/1990_");
                }
                else
                {
                    await Send(from,
                        "That doesn't look like a valid email. Please try again.\n\n" +
                        "Example: _john@email.com_");
                }
                break;

            case ConversationState.WaitingBirthDate:
                if (IsValidBirthDate(text, out var birthDate))
                {
                    _sessions.TryRemove(from, out _);
                    await Send(from,
                        "All done! Here's a summary of your information:\n\n" +
                        $"*Name:* {session.Name}\n" +
                        $"*Email:* {session.Email}\n" +
                        $"*Date of birth:* {birthDate:MM/dd/yyyy}\n\n" +
                        "We'll be in touch with your quote shortly.");
                }
                else
                {
                    await Send(from,
                        "That doesn't look like a valid date. Please use the format MM/dd/yyyy.\n\n" +
                        "Example: _03/15/1990_");
                }
                break;
        }
    }

    private Task Send(string to, string message) =>
        whatsApp.SendMessageAsync(to, message);

    private static bool IsValidName(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2 && parts.All(p => p.All(c => char.IsLetter(c) || c == '-'));
    }

    private static bool IsValidEmail(string email)
    {
        try { return new MailAddress(email).Address == email; }
        catch { return false; }
    }

    private static bool IsValidBirthDate(string input, out DateOnly date)
    {
        date = default;
        if (!DateOnly.TryParseExact(input, "MM/dd/yyyy", out date))
            return false;

        var today = DateOnly.FromDateTime(DateTime.Today);
        var age = today.Year - date.Year;
        if (date > today.AddYears(-age)) age--;

        return age is >= 1 and <= 120;
    }
}

public class ConversationSession
{
    public ConversationState State { get; set; } = ConversationState.None;
    public string? Name { get; set; }
    public string? Email { get; set; }
}

public enum ConversationState
{
    None,
    WaitingName,
    WaitingEmail,
    WaitingBirthDate
}
