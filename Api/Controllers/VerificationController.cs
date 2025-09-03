using Application.Dtos;
using Application.Results;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class VerificationController(
    INotificationAggregator aggregator,
    ILogger<VerificationController> logger)
    : ControllerBase
{
    [HttpPost("send-code")]
    public async Task<IActionResult> SendVerificationCode([FromBody] EmailCodeDto dto)
    {
        //mailjet can sometimes not send an email, but return 200, next line is for debug only
        logger.LogInformation("Sending code {nameof(dto.Token)}", dto.Token);
        var result = await aggregator.NotifySingleAsync(dto);
        return ReturnDependingOn(result);
    }
    
    private IActionResult ReturnDependingOn(Result result) =>
        !result.IsSuccess
            ? BadRequest(result.Error)
            : Ok();

}
