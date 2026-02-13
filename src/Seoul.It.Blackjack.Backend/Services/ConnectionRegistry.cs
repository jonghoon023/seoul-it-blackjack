using System.Collections.Generic;

namespace Seoul.It.Blackjack.Backend.Services;

/// <summary>
/// 연결 ID와 플레이어 ID의 매핑을 관리하는 저장소입니다.
/// </summary>
internal sealed class ConnectionRegistry
{
    /// <summary>
    /// 스레드 안전을 위한 잠금 객체입니다.
    /// </summary>
    private readonly object _sync = new();

    /// <summary>
    /// 연결 ID를 키로 사용하는 매핑 테이블입니다.
    /// </summary>
    private readonly Dictionary<string, string> _connectionToPlayer = new();

    /// <summary>
    /// 특정 연결 ID가 등록되어 있는지 확인합니다.
    /// </summary>
    /// <param name="connectionId">확인할 연결 ID입니다.</param>
    /// <returns>등록되어 있으면 true입니다.</returns>
    public bool ContainsConnection(string connectionId)
    {
        lock (_sync)
        {
            return _connectionToPlayer.ContainsKey(connectionId);
        }
    }

    /// <summary>
    /// 연결 ID와 플레이어 ID 매핑을 추가하거나 갱신합니다.
    /// </summary>
    /// <param name="connectionId">연결 ID입니다.</param>
    /// <param name="playerId">플레이어 ID입니다.</param>
    public void Add(string connectionId, string playerId)
    {
        lock (_sync)
        {
            _connectionToPlayer[connectionId] = playerId;
        }
    }

    /// <summary>
    /// 연결 ID 매핑을 제거하고 플레이어 ID를 반환합니다.
    /// </summary>
    /// <param name="connectionId">제거할 연결 ID입니다.</param>
    /// <param name="playerId">제거 성공 시 연결되어 있던 플레이어 ID입니다.</param>
    /// <returns>제거에 성공하면 true입니다.</returns>
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

    /// <summary>
    /// 모든 연결 매핑을 비웁니다.
    /// </summary>
    public void Clear()
    {
        lock (_sync)
        {
            _connectionToPlayer.Clear();
        }
    }
}
