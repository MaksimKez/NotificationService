using Application.Dtos;
using Infrastructure.EmailNotifier.EmailBuilder.Interfaces;
using Infrastructure.EmailNotifier.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Infrastructure.EmailNotifier.EmailBuilder;

public class EmailBuilder : IEmailMessageBuilder
{
    private readonly EmailSettings _settings;

    private EmailInfo _emailInfo = new();
    private ListingDto? _listing;

    public EmailBuilder(IOptions<EmailSettings> defaultSettings)
    {
        _settings = defaultSettings.Value;
    }

    private class EmailInfo
    {
        public string Subject { get; set; } = "Notification";
        public string To { get; set; } = string.Empty;
        public string Body { get; set; } = "New listing matching your filters";
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
    }

    public IEmailMessageBuilder FromTo(string from, string fromName, string to)
    {
        _emailInfo.FromEmail = from;
        _emailInfo.FromName = fromName;
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
        var fromEmail = !string.IsNullOrEmpty(_emailInfo.FromEmail) ? _emailInfo.FromEmail : _settings.FromEmail;
        var fromName = !string.IsNullOrEmpty(_emailInfo.FromName) ? _emailInfo.FromName : _settings.FromName;


        return new JObject
        {
            ["Messages"] = new JArray
            {
                new JObject
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
                            ["Email"] = _emailInfo.To,
                            ["Name"] = "User"
                        }
                    },
                    
                    ["Subject"] = _emailInfo.Subject,
                    ["TextPart"] = _emailInfo.Body + "\n" + _listing!.Url,
                }
            }
        };
    }

    public JObject BuildDefault(ListingDto listing, string toEmail)
    {
        return FromTo(_settings.FromEmail, _settings.FromName, toEmail)
            .WithSubject("New listing for you")
            .WithMessage("A new listing matches your filters")
            .WithListing(listing)
            .Build();
    }

    // private string BuildHtmlBody()
}