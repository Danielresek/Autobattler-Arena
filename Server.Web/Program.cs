using Microsoft.AspNetCore.SignalR;
using Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddCors(o =>
{
    o.AddDefaultPolicy(p => p
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
        .SetIsOriginAllowed(_ => true));
});
builder.Services.AddSingleton<LobbyStore>();

var app = builder.Build();

app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/health", () => Results.Ok(new { ok = true }));

app.MapPost("/api/lobbies", (LobbyStore store) =>
{
    var lobby = store.CreateLobby();
    return Results.Ok(new CreateLobbyResponse { LobbyId = lobby.LobbyId });
});

app.MapHub<LobbyHub>("/hub");

app.Run();


// ---------------- support classes ----------------
public class Lobby
{
    public string LobbyId { get; set; } = "";
    public string Status { get; set; } = "waiting";
    public Dictionary<string, LobbyPlayer> Players { get; set; } = new();
}

public class LobbyStore
{
    private readonly Dictionary<string, Lobby> _lobbies = new();

    public Lobby CreateLobby()
    {
        var id = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        var l = new Lobby { LobbyId = id };
        _lobbies[id] = l;
        return l;
    }

    public Lobby? Get(string lobbyId) => _lobbies.TryGetValue(lobbyId, out var l) ? l : null;
    public IEnumerable<Lobby> All() => _lobbies.Values;
}

public class LobbyHub : Hub
{
    private readonly LobbyStore _store;
    public LobbyHub(LobbyStore store) => _store = store;

    public async Task CreateLobby()
    {
        var lobby = _store.CreateLobby();
        await Clients.Caller.SendAsync("LobbyCreated", new CreateLobbyResponse { LobbyId = lobby.LobbyId });
        await SendLobbyState(lobby.LobbyId);
    }

    public async Task JoinLobby(string lobbyId, string name, Personality personality)
    {
        var lobby = _store.Get(lobbyId);
        if (lobby is null)
        {
            await Clients.Caller.SendAsync("Error", "Lobby not found");
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, lobbyId);

        lobby.Players[Context.ConnectionId] = new LobbyPlayer
        {
            Id = Context.ConnectionId,
            Name = name,
            Personality = personality
        };

        await SendLobbyState(lobbyId);
    }

    public async Task LeaveLobby(string lobbyId)
    {
        var lobby = _store.Get(lobbyId);
        if (lobby is null) return;

        lobby.Players.Remove(Context.ConnectionId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, lobbyId);
        await SendLobbyState(lobbyId);
    }

    public async Task StartRound(string lobbyId)
    {
        var lobby = _store.Get(lobbyId);
        if (lobby is null) return;

        lobby.Status = "running";
        await SendLobbyState(lobbyId);
        await Clients.Group(lobbyId).SendAsync("RoundStarted", new { lobbyId, players = lobby.Players.Values.ToList() });
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        foreach (var lobby in _store.All())
        {
            if (lobby.Players.Remove(Context.ConnectionId))
            {
                await SendLobbyState(lobby.LobbyId);
            }
        }
        await base.OnDisconnectedAsync(exception);
    }

    private async Task SendLobbyState(string lobbyId)
    {
        var lobby = _store.Get(lobbyId);
        if (lobby is null) return;

        var state = new LobbyState
        {
            LobbyId = lobby.LobbyId,
            Status = lobby.Status,
            Players = lobby.Players.Values.ToList()
        };

        await Clients.Group(lobbyId).SendAsync("LobbyState", state);
    }
}
