using Application.DI;
using Infrastructure.DI;
using Infrastructure.EmailNotifier.Models;


var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(EmailSettings.DefaultConfigName));
builder.Services.Configure<RetryPolicySettings>(builder.Configuration.GetSection(RetryPolicySettings.DefaultConfigName));
builder.Services.Configure<MailjetSettings>(builder.Configuration.GetSection(MailjetSettings.DefaultConfigName));

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();