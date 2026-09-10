namespace com.MirenlightStudio.NovaSandbox.Networking
{
    public enum NetworkPacketEnum : byte
    {
        Auth,
        AuthResponse,
        BannedResponse,
        GetRoomListPacket,
        RoomListResponse,
        CreateRoom,
    }
}