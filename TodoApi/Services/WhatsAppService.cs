using System.Text;
using System.Text.Json;

namespace BikeRentalApi.Services;

public class WhatsAppService(HttpClient httpClient, IConfiguration config)
{
    private readonly string _phoneNumberId = config["WhatsApp:PhoneNumberId"]!;

    public async Task<string> SendMessageAsync(string to, string message)
    {
        var url = $"https://graph.facebook.com/v25.0/{_phoneNumberId}/messages";

        var payload = new
        {
            messaging_product = "whatsapp",
            to,
            type = "text",
            text = new { body = message }
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(url, content);
        return await response.Content.ReadAsStringAsync();
    }
}
