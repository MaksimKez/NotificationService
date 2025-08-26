using Application.Dtos;
using Infrastructure.EmailNotifier.EmailBuilder.Interfaces;
using Infrastructure.EmailNotifier.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Infrastructure.EmailNotifier.EmailBuilder;

public class EmailBuilder(IOptions<EmailSettings> defaultSettings) : IEmailMessageBuilder
{
    private readonly EmailSettings _settings = defaultSettings.Value;

    private EmailInfo _emailInfo = new();
    private ListingDto? _listing;

    private class EmailInfo
    {
        public string Subject { get; set; } = "Notification";
        public string To { get; set; } = string.Empty;
        public string Body { get; set; } = "New listing matching your filters";
    }

    public IEmailMessageBuilder FromTo(string from, string fromName, string to)
    {
        _settings.FromEmail = from;
        _settings.FromName = fromName;
        
        _emailInfo.To = to;
        return this;
    }

    public IEmailMessageBuilder WithSubject(string subject)
    {
        _emailInfo.Subject = subject;
        return this;
    }

    public IEmailMessageBuilder WithMessage(string body)
    {
        _emailInfo.Body = body;
        return this;
    }

    public IEmailMessageBuilder WithListing(ListingDto listing)
    {
        _listing = listing;
        return this;
    }

    public JObject Build()
    {
        var message = new JObject
        {
            ["From"] = new JObject
            {
                ["Email"] = _settings.FromEmail,
                ["Name"] = _settings.FromName
            },
            ["To"] = new JArray
            {
                new JObject
                {
                    ["Email"] = _emailInfo.To
                }
            },
            ["Subject"] = _emailInfo.Subject,
            ["TextPart"] = _emailInfo.Body
            // ["HTMLPart"] = BuildHtmlBody()
        };

        return new JObject
        {
            ["Messages"] = new JArray { message }
        };
    }

    public JObject BuildDefault(ListingDto listing, string toEmail)
    {
        return new EmailBuilder(Options.Create(_settings))
            .FromTo(_settings.FromEmail, _settings.FromName, toEmail)
            .WithSubject("New listing for you")
            .WithMessage("A new listing matches your filters")
            .WithListing(listing)
            .Build();
    }

    // private string BuildHtmlBody()
}
