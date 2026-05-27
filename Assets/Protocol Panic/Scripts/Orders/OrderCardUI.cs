using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderCardUI : MonoBehaviour
{
    [SerializeField] private Image _potionImage;
    [SerializeField] private TextMeshProUGUI _rewardText;
    [SerializeField] private IngredientIconDatabase _iconDB;

    public IngredientType OrderType { get; private set; }

    public void Setup(IngredientType type, int reward)
    {
        OrderType = type;
        _rewardText.text = "$" + reward;

        if (_iconDB == null)
        {
            return;
        }

        Sprite icon = _iconDB.Get(type);

        if (icon == null)
        {
            return;
        }

        _potionImage.sprite = icon;
        _potionImage.color = Color.white;
    }
}