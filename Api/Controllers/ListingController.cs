using Application.Dtos;
using Application.Results;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ListingController(
    INotificationAggregator notificationAggregator)
    : ControllerBase
{
    [HttpPost("notify-single")]
    public async Task<IActionResult> NotifySingle([FromBody] UserListingPairDto userListingPair)
    {
        var result = await notificationAggregator.NotifySingle(userListingPair);
        return ReturnDependingOn(result);
    } 
    
    [HttpPost("notify-multiple")]
    public async Task<IActionResult> NotifyMultiple([FromBody] UserListingPairDto[] userListingPairs)
    {
        var result = await notificationAggregator.NotifyMultiple(userListingPairs);
        return ReturnDependingOn(result);
    }

    private IActionResult ReturnDependingOn(Result result) =>
        !result.IsSuccess
            ? BadRequest(result.Error)
            : Ok();
}