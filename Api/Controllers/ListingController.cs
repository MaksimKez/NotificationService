using Application.Dtos;
using Application.Results;
using Application.Services.Interfaces;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
[EnableRateLimiting("FixedPolicy")]
public class ListingController(
    INotificationAggregator notificationAggregator)
    : ControllerBase
{
    [HttpPost("notify-single")]
    public async Task<IActionResult> NotifySingle([FromBody] UserListingPairDto userListingPair)
    {
        var result = await notificationAggregator.NotifySingleAsync(userListingPair);
        return ReturnDependingOn(result);
    } 
    
    [HttpPost("notify-multiple")]
    public async Task<IActionResult> NotifyMultiple([FromBody] IEnumerable<UserListingPairDto> userListingPairs)
    {
        var result = await notificationAggregator.NotifyMultipleAsync(userListingPairs.ToArray());
        return ReturnDependingOn(result);
    }

    private IActionResult ReturnDependingOn(Result result) =>
        !result.IsSuccess
            ? BadRequest(result.Error)
            : NoContent();
}
