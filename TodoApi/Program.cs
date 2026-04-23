using BikeRentalApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<WhatsAppService>(client =>
{
    var token = builder.Configuration["WhatsApp:AccessToken"]!;
    client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
});

builder.Services.AddSingleton<ConversationService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Bike Rental API", Version = "v1" });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bike Rental API v1"));

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
