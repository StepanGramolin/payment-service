using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using PaymentService.DataAccess.Postgres.AppDbContext;
using PaymentService.WebApi.Controllers;
using PaymentService.WebApi.Infrastructure;
using PaymentService.WebApi.Mappers;
using PaymentService.WebApi.Validators;


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

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<UpdateStatusRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreatePaymentRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();


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