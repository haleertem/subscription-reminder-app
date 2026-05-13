using Microsoft.EntityFrameworkCore;
using SubscriptionReminder.Api.Data;
using SubscriptionReminder.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5000");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        var origin = builder.Configuration["Cors:FrontendOrigin"] ?? "http://localhost:5173";
        policy.WithOrigins(origin).AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddHttpClient("MockApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["MockServices:BaseUrl"] ?? "http://localhost:5000/api/mock");
});

builder.Services.AddScoped<IDebtQueryService, DebtQueryService>();
builder.Services.AddScoped<IPaymentGatewayService, PaymentGatewayService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IReminderService, ReminderService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    DataSeeder.Seed(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Frontend");
app.MapControllers();

app.Run();
