namespace Seoul.It.Blackjack.Backend.Services.Commands;

/// <summary>
/// 게임 방 서비스가 처리할 명령 종류입니다.
/// </summary>
internal enum GameCommandType
{
    /// <summary>
    /// 입장 명령입니다.
    /// </summary>
    Join,

    /// <summary>
    /// 퇴장 명령입니다.
    /// </summary>
    Leave,

    /// <summary>
    /// 라운드 시작 명령입니다.
    /// </summary>
    StartRound,

    /// <summary>
    /// 히트 명령입니다.
    /// </summary>
    Hit,

    /// <summary>
    /// 스탠드 명령입니다.
    /// </summary>
    Stand,

    /// <summary>
    /// 연결 종료 명령입니다.
    /// </summary>
    Disconnect
}
