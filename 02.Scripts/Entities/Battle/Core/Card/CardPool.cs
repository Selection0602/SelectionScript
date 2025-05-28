using System.Collections.Generic;
using UnityEngine;

public class CardPool : MonoBehaviour
{
    [SerializeField] private HandCardArranger _handCardArranger;
    [SerializeField] private Material _dissolveMaterial;
    private Queue<Card> _cardPool = new();

    private void Awake()
    {
        var cards = transform.GetComponentsInChildren<Card>(true);

        for (int i = 0; i < cards.Length; i++)
        {
            var card = cards[i];
            card.gameObject.SetActive(false);
            card.SetMaterial(_dissolveMaterial);
            _cardPool.Enqueue(card);
        }
    }

    public Card GetCard()
    {
        if (_cardPool.Count == 0)
        {
            return null;
        }

        var card = _cardPool.Dequeue();
        card.gameObject.SetActive(true);
        _handCardArranger.AddCard(card);
        return card;
    }

    public void ReturnCard(Card card)
    {
        card.gameObject.SetActive(false);
        _cardPool.Enqueue(card);
        _handCardArranger.RemoveCard(card);
    }
}
