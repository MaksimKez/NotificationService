using System.Threading.RateLimiting;
using Application.DI;
using Infrastructure.DI;
using Infrastructure.EmailNotifier.Models;
using Microsoft.AspNetCore.RateLimiting;


var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(EmailSettings.DefaultConfigName));
builder.Services.Configure<RetryPolicySettings>(builder.Configuration.GetSection(RetryPolicySettings.DefaultConfigName));
builder.Services.Configure<MailjetSettings>(builder.Configuration.GetSection(MailjetSettings.DefaultConfigName));

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("FixedPolicy", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 1000;
        opt.QueueLimit = 2000;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
    
    options.OnRejected = async (context, t) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.Headers.RetryAfter = "10";
        await context.HttpContext.Response.WriteAsync("Too many requests, try later", t);
    };

});



var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers();

app.Run();