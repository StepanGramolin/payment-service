using Microsoft.EntityFrameworkCore;
using PaymentService.DataAccess.Postgres.AppDbContext;
using PaymentService.WebApi.Infrastructure;
using PaymentService.WebApi.Mappers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DB (PostgreSQL)
builder.Services.AddDbContext<PaymentsDbContext>(opt =>
{
    var cs = builder.Configuration.GetConnectionString("PaymentsDb");
    opt.UseNpgsql(cs);
});

// Kafka producer (publishes JSON events)
builder.Services.AddSingleton<KafkaProducer>();

// Регистрации Mapperly
builder.Services.AddSingleton<PaymentMapper>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok("payment-service ok"));

app.Run();