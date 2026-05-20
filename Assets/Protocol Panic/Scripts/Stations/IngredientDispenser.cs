using UnityEngine;

public class IngredientDispenser : MonoBehaviour
{
    [SerializeField]
    private IngredientType _ingredientType;

    public IngredientType IngredientType =>
        _ingredientType;
}