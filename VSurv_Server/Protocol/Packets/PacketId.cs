namespace VSurvServer.Protocol.Packets;

public enum PacketId : byte
{
    None = 0,

    StartGameRequest = 1,
    StartGameResponse = 2,

    PingRequest = 3,
    PingResponse = 4,

    EndGameRequest = 5,
    EndGameResponse = 6,

    RegisterRequest = 7,
    RegisterResponse = 8,
    LoginRequest = 9,
    LoginResponse = 10,

    RankingRequest = 11,
    RankingResponse = 12,

    GachaRequest = 13,
    GachaResponse = 14,
    InventoryRequest = 15,
    InventoryResponse = 16,
    EquipRequest = 17,
    EquipResponse = 18
}