using Application.Dtos;

namespace PRC.Models.Packets;

public class NotifyMultiplePacket
{
    public UserListingPairDto[]? UserListingPairs { get; set; } = null;
}