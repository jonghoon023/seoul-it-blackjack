using System.Collections.Generic;

namespace Seoul.It.Blackjack.Backend.Services;

internal sealed class ConnectionRegistry
{
    private readonly object _sync = new();
    private readonly Dictionary<string, string> _connectionToPlayer = new();

    public bool ContainsConnection(string connectionId)
    {
        lock (_sync)
        {
            return _connectionToPlayer.ContainsKey(connectionId);
        }
    }

    public void Add(string connectionId, string playerId)
    {
        lock (_sync)
        {
            _connectionToPlayer[connectionId] = playerId;
        }
    }

    public bool TryRemove(string connectionId, out string playerId)
    {
        lock (_sync)
        {
            if (_connectionToPlayer.TryGetValue(connectionId, out string? id))
            {
                _connectionToPlayer.Remove(connectionId);
                playerId = id;
                return true;
            }

            playerId = string.Empty;
            return false;
        }
    }

    public void Clear()
    {
        lock (_sync)
        {
            _connectionToPlayer.Clear();
        }
    }
}
