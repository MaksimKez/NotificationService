using Application.Dtos;
using Infrastructure.EmailNotifier.EmailBuilder.Interfaces;
using Infrastructure.EmailNotifier.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Infrastructure.EmailNotifier.EmailBuilder;

public class EmailBuilder : IEmailMessageBuilder
{
    private readonly EmailSettings _settings;

    private EmailInfo emailInfo = new();
    private ListingDto? listing;

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
        emailInfo.FromEmail = from;
        emailInfo.FromName = fromName;
        emailInfo.To = to;
        return this;
    }

    public IEmailMessageBuilder WithSubject(string subject)
    {
        emailInfo.Subject = subject;
        return this;
    }

    public IEmailMessageBuilder WithMessage(string body)
    {
        emailInfo.Body = body;
        return this;
    }

    public IEmailMessageBuilder WithListing(ListingDto listing)
    {
        this.listing = listing;
        return this;
    }

    public JObject Build()
    {
        var fromEmail = !string.IsNullOrEmpty(emailInfo.FromEmail) ? emailInfo.FromEmail : _settings.FromEmail;
        var fromName = !string.IsNullOrEmpty(emailInfo.FromName) ? emailInfo.FromName : _settings.FromName;


        return new JObject
        {
            ["Messages"] = new JArray
            {
                new JObject
                {
                    ["From"] = new JObject
                    {
                        ["Email"] = fromEmail,
                        ["Name"] = fromName
                    },
                    ["To"] = new JArray
                    {
                        new JObject
                        {
                            ["Email"] = emailInfo.To,
                            ["Name"] = "User"
                        }
                    },
                    
                    ["Subject"] = emailInfo.Subject,
                    ["TextPart"] = emailInfo.Body + "\n" + listing?.Url,
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
}
