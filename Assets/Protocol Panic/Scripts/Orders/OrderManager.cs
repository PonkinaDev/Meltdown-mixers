using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class OrderManager : NetworkBehaviour
{
    public static OrderManager Instance;

    [Header("UI")]
    [SerializeField]
    private Image _orderImage;

    [Networked]
    public IngredientType CurrentOrder { get; set; }

    private IngredientType _lastVisualOrder =
        IngredientType.None;

    private void Awake()
    {
        Instance = this;
    }

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            GenerateNewOrder();
        }
    }

    public override void Render()
    {
        UpdateUI();
    }

    public bool TryDeliver(
        IngredientType ingredient
    )
    {
        if (ingredient != CurrentOrder)
            return false;

        GenerateNewOrder();

        return true;
    }

    private void GenerateNewOrder()
    {
        int random =
            Random.Range(0, 6);

        switch (random)
        {
            case 0:
                CurrentOrder =
                    IngredientType.Red;
                break;

            case 1:
                CurrentOrder =
                    IngredientType.Blue;
                break;

            case 2:
                CurrentOrder =
                    IngredientType.Yellow;
                break;

            case 3:
                CurrentOrder =
                    IngredientType.Green;
                break;

            case 4:
                CurrentOrder =
                    IngredientType.Orange;
                break;

            case 5:
                CurrentOrder =
                    IngredientType.Purple;
                break;
        }
    }

    private void UpdateUI()
    {
        if (_lastVisualOrder ==
            CurrentOrder)
            return;

        _lastVisualOrder =
            CurrentOrder;

        switch (CurrentOrder)
        {
            case IngredientType.Red:
                _orderImage.color =
                    Color.red;
                break;

            case IngredientType.Blue:
                _orderImage.color =
                    Color.blue;
                break;

            case IngredientType.Yellow:
                _orderImage.color =
                    Color.yellow;
                break;

            case IngredientType.Green:
                _orderImage.color =
                    Color.green;
                break;

            case IngredientType.Orange:
                _orderImage.color =
                    new Color(1f, 0.5f, 0f);
                break;

            case IngredientType.Purple:
                _orderImage.color =
                    new Color(0.5f, 0f, 1f);
                break;
        }
    }
}