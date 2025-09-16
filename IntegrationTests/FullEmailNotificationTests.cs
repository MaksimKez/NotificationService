using Application.Dtos;
using Application.Dtos.Settings;
using Application.Results;
using Application.Services;
using Application.Services.Strategies;
using Infrastructure.EmailNotifier;
using Infrastructure.EmailNotifier.EmailBuilder;
using Infrastructure.EmailNotifier.Models;
using Infrastructure.EmailNotifier.Interfaces;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using FluentAssertions;
using Moq;

namespace IntegrationTests
{
    public class FullEmailNotificationTests
    {
        [Fact]
        public async Task Should_Send_Email_Notification_Successfully()
        {
            // Arrange
            var settings = Options.Create(new EmailSettings
            {
                FromEmail = "noreply@test.com",
                FromName = "Notification Service"
            });

            var emailSenderMock = new Mock<IEmailSender>();
            emailSenderMock
                .Setup(s => s.SendEmailAsync(It.IsAny<JObject>(), CancellationToken.None))
                .ReturnsAsync(Result.Success());

            var options = Options.Create(new EmailNotifierSettings());
            var emailBuilder = new EmailBuilder(settings);
            var emailNotifier = new EmailNotifier(emailBuilder, emailSenderMock.Object, options);
            var strategy = new FallbackNotificationStrategy();
            var aggregator = new NotificationAggregator(new[] { emailNotifier }, strategy);

            var userListing = new UserListingPairDto
            {
                User = new UserDto { Id = Guid.NewGuid(), Email = "user@test.com" },
                Listing = new ListingDto { Url = "http://listing.com/1" }
            };

            // Act
            var result = await aggregator.NotifySingleAsync(userListing);

            // Assert
            result.IsSuccess.Should().BeTrue();
            emailSenderMock.Verify(s => s.SendEmailAsync(It.IsAny<JObject>(), CancellationToken.None), Times.Once);
        }
    }
}
