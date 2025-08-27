using Application.Dtos;
using Newtonsoft.Json.Linq;

namespace Infrastructure.EmailNotifier.EmailBuilder.Interfaces;

public interface IEmailMessageBuilder
{
    IEmailMessageBuilder FromTo(string from, string fromName, string to);
    IEmailMessageBuilder WithSubject(string subject);
    IEmailMessageBuilder WithMessage(string body);
    IEmailMessageBuilder WithListing(ListingDto listing);
    JObject Build();
    
    JObject BuildDefault(ListingDto listing, string toEmail);
}