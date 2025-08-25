using Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ListingController(
    )
    : ControllerBase
{
    [HttpPost("notify-single")]
    public async Task<IActionResult> NotifySingle([FromBody] UserListingPairDto userListingPair)
    {
        throw new NotImplementedException();
    } 
    
    [HttpPost("notify-multiple")]
    public async Task<IActionResult> NotifyMultiple([FromBody] UserListingPairDto[] userListingPairs)
    {
        throw new NotImplementedException();
    } 
}