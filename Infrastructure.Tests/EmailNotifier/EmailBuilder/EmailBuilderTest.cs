using Application.Dtos;
using FluentAssertions;
using Infrastructure.EmailNotifier.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Infrastructure.Tests.EmailNotifier.EmailBuilder;

public class EmailBuilderTests
{
    private readonly EmailSettings _settings;

    public EmailBuilderTests()
    {
        _settings = new EmailSettings
        {
            FromEmail = "custom@test.com",
            FromName = "Custom Sender"
        };
    }

    private Infrastructure.EmailNotifier.EmailBuilder.EmailBuilder CreateBuilder() => new(Options.Create(_settings));

    [Fact]
    public void Build_Should_Create_JObject_With_Correct_Fields()
    {
        // Arrange
        var listing = new ListingDto { Url = "http://listing.com/1" };

        var builder = CreateBuilder()
            .FromTo("custom@test.com", "Custom Sender", "user@test.com")
            .WithSubject("Custom Subject")
            .WithMessage("Custom Body")
            .WithListing(listing);

        // Act
        var result = builder.Build();

        // Assert
        result.Should().NotBeNull();
        var message = result["Messages"]![0]!;

        message["From"]!["Email"]!.Value<string>().Should().Be(_settings.FromEmail);
        message["From"]!["Name"]!.Value<string>().Should().Be(_settings.FromName);
        message["To"]![0]!["Email"]!.Value<string>().Should().Be("user@test.com");
        message["Subject"]!.Value<string>().Should().Be("Custom Subject");
        message["TextPart"]!.Value<string>().Should().Contain("Custom Body");
        message["TextPart"]!.Value<string>().Should().Contain(listing.Url);
    }

    [Fact]
    public void BuildDefault_Should_Use_Default_Settings_And_Listing()
    {
        // Arrange
        var listing = new ListingDto { Url = "http://listing.com/2" };
        var builder = CreateBuilder();

        // Act
        var result = builder.BuildDefault(listing, "recipient@test.com");

        // Assert
        var message = result["Messages"]![0]!;

        message["From"]!["Email"]!.Value<string>().Should().Be(_settings.FromEmail);
        message["From"]!["Name"]!.Value<string>().Should().Be(_settings.FromName);
        message["To"]![0]!["Email"]!.Value<string>().Should().Be("recipient@test.com");
        message["Subject"]!.Value<string>().Should().Be("New listing for you");
        message["TextPart"]!.Value<string>().Should().Contain(listing.Url);
    }

    [Fact]
    public void Build_Should_Throw_When_Listing_Is_Not_Set()
    {
        // Arrange
        var builder = CreateBuilder()
            .FromTo("from@test.com", "From Name", "to@test.com")
            .WithSubject("No Listing Subject")
            .WithMessage("Body without listing");

        // Act
        var act = () => builder.Build();

        // Assert
        act.Should().Throw<NullReferenceException>();
    }
}