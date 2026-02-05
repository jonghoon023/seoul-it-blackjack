using Seoul.It.Blackjack.Backend.Domains;

namespace Seoul.It.Blackjack.Backend.Domains;

/// <summary>
/// 52장 카드를 담는 덱을 표현합니다. 덱은 매 라운드마다 새로 생성하고 셔플합니다.
/// </summary>
public class Deck
{
    private readonly Stack<Card> _cards;
    private static readonly string[] Ranks =
    {
        "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K"
    };
    private static readonly string[] Suits =
    {
        "♠", "♥", "♦", "♣"
    };

    public Deck()
    {
        // 모든 카드 생성 후 무작위로 섞어 Stack에 저장합니다.
        var list = new List<Card>();
        foreach (var suit in Suits)
        {
            foreach (var rank in Ranks)
            {
                list.Add(new Card(rank, suit));
            }
        }
        // 무작위 섞기
        var rng = new Random();
        var shuffled = list.OrderBy(_ => rng.Next()).ToList();
        _cards = new Stack<Card>(shuffled);
    }

    /// <summary>
    /// 카드 한 장을 뽑습니다. 덱이 비어 있으면 예외를 던집니다.
    /// </summary>
    public Card Draw()
    {
        if (_cards.Count == 0)
        {
            throw new InvalidOperationException("덱에 카드가 더 이상 없습니다.");
        }
        return _cards.Pop();
    }

    /// <summary>
    /// 남은 카드 수를 반환합니다.
    /// </summary>
    public int Count => _cards.Count;
}