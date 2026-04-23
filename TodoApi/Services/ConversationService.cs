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
                if (text.Contains("orçamento", StringComparison.OrdinalIgnoreCase))
                {
                    session.State = ConversationState.WaitingName;
                    await Send(from,
                        "Olá! Fico feliz em ajudar com o orçamento.\n\n" +
                        "Primeiro, preciso do seu *nome completo*.\n\n" +
                        "Exemplo: _João da Silva_");
                }
                break;

            case ConversationState.WaitingName:
                if (IsValidName(text))
                {
                    session.Name = text;
                    session.State = ConversationState.WaitingEmail;
                    await Send(from,
                        $"Obrigado, *{text}*!\n\n" +
                        "Agora, informe seu *e-mail*.\n\n" +
                        "Exemplo: _joao@email.com_");
                }
                else
                {
                    await Send(from,
                        "Nome invalido. Informe nome e sobrenome, apenas letras.\n\n" +
                        "Exemplo: _João da Silva_");
                }
                break;

            case ConversationState.WaitingEmail:
                if (IsValidEmail(text))
                {
                    session.Email = text;
                    session.State = ConversationState.WaitingBirthDate;
                    await Send(from,
                        "Perfeito!\n\n" +
                        "Por último, qual é a sua *data de nascimento*?\n\n" +
                        "Exemplo: _15/03/1990_");
                }
                else
                {
                    await Send(from,
                        "E-mail invalido. Verifique e tente novamente.\n\n" +
                        "Exemplo: _joao@email.com_");
                }
                break;

            case ConversationState.WaitingBirthDate:
                if (IsValidBirthDate(text, out var birthDate))
                {
                    _sessions.TryRemove(from, out _);
                    await Send(from,
                        "Dados recebidos com sucesso!\n\n" +
                        $"*Nome:* {session.Name}\n" +
                        $"*E-mail:* {session.Email}\n" +
                        $"*Nascimento:* {birthDate:dd/MM/yyyy}\n\n" +
                        "Em breve entraremos em contato com o seu orçamento.");
                }
                else
                {
                    await Send(from,
                        "Data invalida. Use o formato dd/MM/yyyy e informe uma data real.\n\n" +
                        "Exemplo: _15/03/1990_");
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
        if (!DateOnly.TryParseExact(input, "dd/MM/yyyy", out date))
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
