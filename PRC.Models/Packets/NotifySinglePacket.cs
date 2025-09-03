using Application.Dtos;

namespace PRC.Models.Packets;

public class NotifySinglePacket
{
    public UserListingPairDto? Dto { get; set; } = null;
}