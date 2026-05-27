using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OrderBoardUI : MonoBehaviour
{
    [SerializeField]
    private OrderManager _orderManager;

    [SerializeField]
    private Transform _orderContainer;

    [SerializeField]
    private GameObject _orderCardPrefab;

    [SerializeField]
    private TMP_Text _totalMoneyText;

    private readonly List<OrderCardUI>
        _spawnedCards = new();

    private void Update()
    {
        if (_orderManager == null)
            return;

        if (!_orderManager.IsUiDirty)
            return;

        RefreshUI();
    }

    private void RefreshUI()
    {
        UpdateMoneyText();
        RebuildCards();
    }

    private void UpdateMoneyText()
    {
        if (_totalMoneyText == null)
            return;

        _totalMoneyText.text =
            $"X {_orderManager.TotalMoney}";
    }

    private void RebuildCards()
    {
        ClearCards();

        for (int i = 0;
             i < _orderManager.Orders.Length;
             i++)
        {
            OrderManager.OrderNetworkData order =
                _orderManager.Orders[i];

            if (order.Ingredient ==
                IngredientType.None)
            {
                continue;
            }

            CreateCard(order);
        }
    }

    private void CreateCard(
        OrderManager.OrderNetworkData order
    )
    {
        GameObject cardObject =
            Instantiate(
                _orderCardPrefab,
                _orderContainer
            );

        OrderCardUI card =
            cardObject.GetComponent<OrderCardUI>();

        if (card == null)
        {
            Destroy(cardObject);
            return;
        }

        card.Setup(
            order.Ingredient,
            order.Reward
        );

        _spawnedCards.Add(card);
    }

    private void ClearCards()
    {
        foreach (OrderCardUI card
                 in _spawnedCards)
        {
            if (card != null)
            {
                Destroy(card.gameObject);
            }
        }

        _spawnedCards.Clear();
    }
}