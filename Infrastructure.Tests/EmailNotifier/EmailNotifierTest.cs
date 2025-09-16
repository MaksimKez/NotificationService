using Application.Dtos;
using Application.Dtos.Settings;
using Application.Results;
using FluentAssertions;
using Infrastructure.EmailNotifier.EmailBuilder.Interfaces;
using Infrastructure.EmailNotifier.Interfaces;
using Infrastructure.EmailNotifier.Models;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json.Linq;

namespace Infrastructure.Tests.EmailNotifier
{
    public class EmailNotifierTests
    {
        private readonly Mock<IEmailMessageBuilder> _builderMock;
        private readonly Mock<IEmailSender> _senderMock;
        private readonly Infrastructure.EmailNotifier.EmailNotifier _notifier;

        public EmailNotifierTests()
        {
            _builderMock = new Mock<IEmailMessageBuilder>();
            _senderMock = new Mock<IEmailSender>();

            var options = Options.Create(new EmailNotifierSettings());
            _notifier = new Infrastructure.EmailNotifier.EmailNotifier(_builderMock.Object, _senderMock.Object, options);
        }

        private static UserListingPairDto CreateUserListingPair(Guid? userId = null, string? email = "test@test.com")
        {
            return new UserListingPairDto
            {
                User = new UserDto
                {
                    Id = userId ?? Guid.NewGuid(),
                    Email = email
                },
                Listing = new ListingDto { Url = "http://listing.com/1" }
            };
        }

        [Fact]
        public async Task NotifySingle_Should_Return_Success_When_Sender_Succeeds()
        {
            // Arrange
            var dto = CreateUserListingPair();
            var emailMessage = new JObject();

            _builderMock
                .Setup(b => b.BuildDefault(dto.Listing, dto.User.Email))
                .Returns(emailMessage);

            _senderMock
                .Setup(s => s.SendEmailAsync(emailMessage, CancellationToken.None))
                .ReturnsAsync(Result.Success());

            // Act
            var result = await _notifier.NotifySingle(dto);

            // Assert
            result.IsSuccess.Should().BeTrue();
            _builderMock.Verify(b => b.BuildDefault(dto.Listing, dto.User.Email), Times.Once);
            _senderMock.Verify(s => s.SendEmailAsync(emailMessage, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task NotifySingle_Should_Return_Failure_When_Sender_Fails()
        {
            // Arrange
            var dto = CreateUserListingPair();
            var emailMessage = new JObject();

            _builderMock
                .Setup(b => b.BuildDefault(dto.Listing, dto.User.Email))
                .Returns(emailMessage);

            _senderMock
                .Setup(s => s.SendEmailAsync(emailMessage, CancellationToken.None))
                .ReturnsAsync(Result.Failure("send error"));

            // Act
            var result = await _notifier.NotifySingle(dto);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("send error");
        }

        [Fact]
        public async Task NotifySingle_Should_Throw_When_UserEmail_Is_Null()
        {
            // Arrange
            var dto = CreateUserListingPair(email: null);

            // Act
            var act = async () => await _notifier.NotifySingle(dto);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task NotifyMultiple_Should_Return_Success_When_All_Succeed()
        {
            // Arrange
            var pairs = new[]
            {
                CreateUserListingPair(),
                CreateUserListingPair()
            };

            _builderMock
                .Setup(b => b.BuildDefault(It.IsAny<ListingDto>(), It.IsAny<string>()))
                .Returns(new JObject());

            _senderMock
                .Setup(s => s.SendEmailAsync(It.IsAny<JObject>(), CancellationToken.None))
                .ReturnsAsync(Result.Success());

            // Act
            var result = await _notifier.NotifyMultiple(pairs);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEmpty();
        }

        [Fact]
        public async Task NotifyMultiple_Should_Return_PartialFailure_When_Some_Fail()
        {
            // Arrange
            var successPair = CreateUserListingPair(userId: Guid.NewGuid(), email: "ok@test.com");
            var failedPair = CreateUserListingPair(userId: Guid.NewGuid(), email: "fail@test.com");

            _builderMock
                .Setup(b => b.BuildDefault(It.IsAny<ListingDto>(), It.IsAny<string>()))
                .Returns(new JObject());

            _senderMock
                .SetupSequence(s => s.SendEmailAsync(It.IsAny<JObject>(), CancellationToken.None))
                .ReturnsAsync(Result.Failure("failed to send"))
                .ReturnsAsync(Result.Success());

            var pairs = new[] { failedPair, successPair };

            // Act
            var result = await _notifier.NotifyMultiple(pairs);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Value.Should().ContainKey(failedPair.User.Id);
            result.Value[failedPair.User.Id].Should().Be("failed to send");
        }
    }
}
