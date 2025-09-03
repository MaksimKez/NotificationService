using Application.Dtos;

namespace PRC.Models.Packets;

public class SendVerificationCodePacket
{
    public EmailCodeDto? EmailCode { get; set; } = null!;
}