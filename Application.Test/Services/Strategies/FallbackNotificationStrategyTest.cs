using Application.Abstractions;
using Application.Dtos;
using Application.Results;
using Application.Services.Strategies;
using FluentAssertions;
using Moq;

namespace Application.Test.Services.Strategies;

public class FallbackNotificationStrategyTests
{
    private readonly FallbackNotificationStrategy _strategy = new();

    [Fact]
    public async Task Notify_Should_Return_Success_From_First_Successful_Notifier()
    {
        // Arrange
        var dto = new UserListingPairDto();

        var notifier1 = new Mock<INotifier>();
        notifier1.Setup(n => n.Priority).Returns(1);
        notifier1.Setup(n => n.NotifySingle(dto)).ReturnsAsync(Result.Success());

        var notifier2 = new Mock<INotifier>();
        notifier2.Setup(n => n.Priority).Returns(2);

        var notifiers = new[] { notifier1.Object, notifier2.Object };

        // Act
        var result = await _strategy.Notify(dto, notifiers);

        // Assert
        result.IsSuccess.Should().BeTrue();
        notifier1.Verify(n => n.NotifySingle(dto), Times.Once);
        notifier2.Verify(n => n.NotifySingle(It.IsAny<UserListingPairDto>()), Times.Never);
    }

    [Fact]
    public async Task Notify_Should_Fallback_To_Second_Notifier_When_First_Fails()
    {
        // Arrange
        var dto = new UserListingPairDto();

        var notifier1 = new Mock<INotifier>();
        notifier1.Setup(n => n.Priority).Returns(1);
        notifier1.Setup(n => n.NotifySingle(dto)).ReturnsAsync(Result.Failure("fail"));

        var notifier2 = new Mock<INotifier>();
        notifier2.Setup(n => n.Priority).Returns(2);
        notifier2.Setup(n => n.NotifySingle(dto)).ReturnsAsync(Result.Success());

        var notifiers = new[] { notifier1.Object, notifier2.Object };

        // Act
        var result = await _strategy.Notify(dto, notifiers);

        // Assert
        result.IsSuccess.Should().BeTrue();
        notifier1.Verify(n => n.NotifySingle(dto), Times.Once);
        notifier2.Verify(n => n.NotifySingle(dto), Times.Once);
    }

    [Fact]
    public async Task Notify_Should_Return_Failure_When_All_Notifiers_Fail()
    {
        // Arrange
        var dto = new UserListingPairDto();

        var notifier1 = new Mock<INotifier>();
        notifier1.Setup(n => n.Priority).Returns(1);
        notifier1.Setup(n => n.NotifySingle(dto)).ReturnsAsync(Result.Failure("fail1"));

        var notifier2 = new Mock<INotifier>();
        notifier2.Setup(n => n.Priority).Returns(2);
        notifier2.Setup(n => n.NotifySingle(dto)).ReturnsAsync(Result.Failure("fail2"));

        var notifiers = new[] { notifier1.Object, notifier2.Object };

        // Act
        var result = await _strategy.Notify(dto, notifiers);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("All notification channel failed.");
        notifier1.Verify(n => n.NotifySingle(dto), Times.Once);
        notifier2.Verify(n => n.NotifySingle(dto), Times.Once);
    }
}