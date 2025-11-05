namespace Shared
{
    public enum Personality { AGGRESSIVE, DEFENSIVE, TACTICAL }

    public class LobbyPlayer
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public Personality Personality { get; set; }
    }

    public class LobbyState
    {
        public string LobbyId { get; set; } = "";
        public string Status { get; set; } = "waiting"; // waiting | running | ended
        public List<LobbyPlayer> Players { get; set; } = new();
    }

    public class CreateLobbyResponse
    {
        public string LobbyId { get; set; } = "";

    }

    public class KillEvent
    {
        public string LobbyId { get; set; } = "";
        public string? KillerId { get; set; }
        public string VictimId { get; set; } = "";
        public int Tick { get; set; }
    }

    public class RoundEnd
    {
        public string LobbyId { get; set; } = "";
        public string? WinnerId { get; set; }
    }
}