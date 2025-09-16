using Application.Abstractions;
using Application.Dtos;
using Application.Results;
using Application.Services;
using Application.Services.Interfaces;
using FluentAssertions;
using Moq;

namespace Application.Test.Services;

public class NotificationAggregatorTests
{
    private readonly Mock<INotificationStrategy> _strategyMock;
    private readonly List<Mock<INotifier>> _notifierMocks;
    private readonly NotificationAggregator _aggregator;

    public NotificationAggregatorTests()
    {
        _strategyMock = new Mock<INotificationStrategy>();

        _notifierMocks =
        [
            new Mock<INotifier>(),
            new Mock<INotifier>()
        ];

        _aggregator = new NotificationAggregator(
            _notifierMocks.ConvertAll(m => m.Object),
            _strategyMock.Object
        );
    }

    [Fact]
    public async Task NotifySingle_Should_Call_Strategy_And_Return_Result()
    {
        // Arrange
        var dto = new UserListingPairDto();
        var expectedResult = Result.Success();

        _strategyMock
            .Setup(s => s.Notify(dto, It.IsAny<IEnumerable<INotifier>>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _aggregator.NotifySingleAsync(dto);

        // Assert
        result.Should().Be(expectedResult);
        _strategyMock.Verify(s => s.Notify(dto, It.Is<IEnumerable<INotifier>>(n => n != null)), Times.Once);
    }

    [Fact]
    public async Task NotifyMultiple_Should_Stop_At_First_Failure()
    {
        // Arrange
        var dtos = new[]
        {
            new UserListingPairDto(),
            new UserListingPairDto()
        };

        _strategyMock
            .SetupSequence(s => s.Notify(It.IsAny<UserListingPairDto>(), It.IsAny<IEnumerable<INotifier>>()))
            .ReturnsAsync(Result.Failure("error")) // первая ошибка
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _aggregator.NotifyMultipleAsync(dtos);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("error");

        // Убедимся, что второй не вызывался
        _strategyMock.Verify(
            s => s.Notify(dtos[1], It.IsAny<IEnumerable<INotifier>>()), 
            Times.Never
        );
    }

    [Fact]
    public async Task NotifyMultiple_Should_Return_Success_When_All_Success()
    {
        // Arrange
        var dtos = new[]
        {
            new UserListingPairDto(),
            new UserListingPairDto()
        };

        _strategyMock
            .Setup(s => s.Notify(It.IsAny<UserListingPairDto>(), It.IsAny<IEnumerable<INotifier>>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _aggregator.NotifyMultipleAsync(dtos);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _strategyMock.Verify(s => s.Notify(It.IsAny<UserListingPairDto>(), It.IsAny<IEnumerable<INotifier>>()), Times.Exactly(2));
    }
}