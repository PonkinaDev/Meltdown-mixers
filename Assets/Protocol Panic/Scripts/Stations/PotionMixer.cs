using Fusion;
using UnityEngine;

public class PotionMixer : NetworkBehaviour
{
    [Networked]
    public IngredientType CurrentColor { get; set; }

    private Renderer _renderer;

    private IngredientType _lastVisualColor =
        IngredientType.None;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    public override void Render()
    {
        UpdateVisual();
    }

    // ─────────────────────────
    // INTERACCIÓN
    // ─────────────────────────

    public bool TryAddIngredient(
        IngredientType ingredient
    )
    {
        // Nada
        if (ingredient == IngredientType.None)
            return false;

        if (IsSecondary(CurrentColor))
            return false;

        if (CurrentColor == IngredientType.None)
        {
            CurrentColor = ingredient;
            return true;
        }

        if (CurrentColor == ingredient)
            return false;

        IngredientType result =
            Mix(CurrentColor, ingredient);

        if (result == IngredientType.None)
            return false;

        CurrentColor = result;

        return true;
    }
    
    private IngredientType Mix(
        IngredientType a,
        IngredientType b
    )
    {
        // rojo + azul
        if (
            (a == IngredientType.Red &&
             b == IngredientType.Blue)
            ||
            (a == IngredientType.Blue &&
             b == IngredientType.Red)
        )
        {
            return IngredientType.Purple;
        }

        // azul + amarillo
        if (
            (a == IngredientType.Blue &&
             b == IngredientType.Yellow)
            ||
            (a == IngredientType.Yellow &&
             b == IngredientType.Blue)
        )
        {
            return IngredientType.Green;
        }

        // rojo + amarillo
        if (
            (a == IngredientType.Red &&
             b == IngredientType.Yellow)
            ||
            (a == IngredientType.Yellow &&
             b == IngredientType.Red)
        )
        {
            return IngredientType.Orange;
        }

        return IngredientType.None;
    }

    private bool IsSecondary(
        IngredientType type
    )
    {
        return
            type == IngredientType.Green
            || type == IngredientType.Orange
            || type == IngredientType.Purple;
    }

    private void UpdateVisual()
    {
        if (_lastVisualColor ==
            CurrentColor)
            return;

        _lastVisualColor =
            CurrentColor;

        Material mat =
            new Material(
                Shader.Find(
                    "Universal Render Pipeline/Lit"
                )
            );

        _renderer.material = mat;

        switch (CurrentColor)
        {
            case IngredientType.None:
                _renderer.material.color =
                    Color.white;
                break;

            case IngredientType.Red:
                _renderer.material.color =
                    Color.red;
                break;

            case IngredientType.Blue:
                _renderer.material.color =
                    Color.blue;
                break;

            case IngredientType.Yellow:
                _renderer.material.color =
                    Color.yellow;
                break;

            case IngredientType.Green:
                _renderer.material.color =
                    Color.green;
                break;

            case IngredientType.Orange:
                _renderer.material.color =
                    new Color(1f, 0.5f, 0f);
                break;

            case IngredientType.Purple:
                _renderer.material.color =
                    new Color(0.5f, 0f, 1f);
                break;
        }
    }
}