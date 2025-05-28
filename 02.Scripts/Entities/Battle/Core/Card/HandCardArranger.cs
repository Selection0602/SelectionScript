using System.Collections.Generic;
using UnityEngine;

public enum ArrangeType
{
    RightToLeft,
    FanShape
}

public class HandCardArranger : MonoBehaviour
{
    [SerializeField] private ArrangeType _arrangeType;

    [Header("부채꼴 정렬")]
    [SerializeField] private float _paddingAngle = 4f;
    [SerializeField] private float _radius = 3000f;

    [Header("왼쪽 방향 정렬")]
    [SerializeField] private float _padding = 10f;

    private List<Card> _handCards = new();

    public void AddCard(Card card)
    {
        _handCards.Add(card);
        if (card.CardAnimation != null)
        {
            card.CardAnimation.OnUpdateCardSibling = ArrangePanShapeCards;
        }
        ArrangeCards();
    }

    public void RemoveCard(Card card)
    {
        _handCards.Remove(card);
        if (card.CardAnimation != null)
        {
            card.CardAnimation.OnUpdateCardSibling = null;
        }
        ArrangeCards();
    }

    private void ArrangeCards()
    {
        switch (_arrangeType)
        {
            case ArrangeType.FanShape:
                ArrangePanShapeCards();
                break;
            case ArrangeType.RightToLeft:
                ArrangeRightToLeftCards();
                break;
        }
    }

    private void ArrangePanShapeCards()
    {
        int count = _handCards.Count;

        if (count == 1)
        {
            var card = _handCards[0];
            card.CardAnimation.SetBasePosition(Vector3.zero);
            card.CardAnimation.SetBaseRotate(Quaternion.identity);
            return;
        }

        float minRadius = 1000f;     // 카드가 겹치지 않을 최소 반지름

        float shrinkFactor = 1f - Mathf.Min(0.05f * (count - 1), 0.8f); //5%씩 최대 80%까지 감소
        float radius = Mathf.Clamp(_radius * shrinkFactor, minRadius, _radius);

        float angleRange = _paddingAngle * (count - 1);
        float angleStep = angleRange / (count - 1);

        for (int i = 0; i < count; i++)
        {
            float angle = -angleRange / 2f + angleStep * i;
            float rad = angle * Mathf.Deg2Rad;

            Vector2 offset = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad) - 1f) * radius;
            Quaternion rot = Quaternion.Euler(0, 0, -angle);

            _handCards[i].CardAnimation.SetBasePosition(offset);
            _handCards[i].CardAnimation.SetBaseRotate(rot);
            _handCards[i].transform.SetSiblingIndex(i);
        }
    }

    private void ArrangeRightToLeftCards()
    {
        for (int i = 0; i < _handCards.Count; i++)
        {
            var cardRect = _handCards[i].GetComponent<RectTransform>();
            float cardWidth = cardRect.sizeDelta.x;
            float x = -i * (_padding + 80);
            cardRect.localPosition = new Vector2(x, 0);
        }
    }
}
