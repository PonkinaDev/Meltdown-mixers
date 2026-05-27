using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class PotionHotPlate : NetworkBehaviour
{
    [Header("Visual")]
    [SerializeField] private Material _potionMaterial;

    // Arrastra aquí SOLO el objeto del líquido (por ejemplo, Element 1)
    [SerializeField] private GameObject _liquidVisual;

    [Header("Progress")]
    [SerializeField] private Slider _progressBar;

    [SerializeField] private Image _progressFill;

    [Header("Cook Settings")]
    [SerializeField] private float _cookTime = 5f;

    [SerializeField] private float _burnTime = 10f;

    [Networked]
    public IngredientType CurrentPotion { get; set; }

    [Networked]
    public PotionState CurrentState { get; set; }

    [Networked]
    public float CookTimer { get; set; }

    private IngredientType _lastPotion = IngredientType.None;
    private PotionState _lastState = PotionState.Raw;

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        if (CurrentPotion == IngredientType.None)
            return;

        CookTimer += Runner.DeltaTime;

        if (CookTimer >= _burnTime)
            CurrentState = PotionState.Burned;
        else if (CookTimer >= _cookTime)
            CurrentState = PotionState.Cooked;
    }

    public override void Render()
    {
        UpdateVisual();
        UpdateProgressBar();
    }

    public bool TryAddPotion(IngredientType ingredient)
    {
        if (CurrentPotion != IngredientType.None)
            return false;

        CurrentPotion = ingredient;
        CurrentState = PotionState.Raw;
        CookTimer = 0f;

        return true;
    }

    public bool TryTakePotion(out IngredientType potion, out PotionState state)
    {
        potion = IngredientType.None;
        state = PotionState.Raw;

        if (CurrentPotion == IngredientType.None)
            return false;

        potion = CurrentPotion;
        state = CurrentState;

        CurrentPotion = IngredientType.None;
        CurrentState = PotionState.Raw;
        CookTimer = 0f;

        return true;
    }

    private void UpdateVisual()
    {
        bool hasPotion = CurrentPotion != IngredientType.None;

        if (_liquidVisual != null)
            _liquidVisual.SetActive(hasPotion);

        if (!hasPotion)
        {
            _lastPotion = IngredientType.None;
            _lastState = PotionState.Raw;
            return;
        }

        if (_lastPotion == CurrentPotion && _lastState == CurrentState)
            return;

        _lastPotion = CurrentPotion;
        _lastState = CurrentState;

        if (_potionMaterial == null)
            return;

        _potionMaterial.color =
            IngredientColorUtility.GetPotionColor(CurrentPotion, CurrentState);
    }

    private void UpdateProgressBar()
    {
        if (_progressBar == null)
            return;

        if (CurrentPotion == IngredientType.None)
        {
            _progressBar.gameObject.SetActive(false);
            return;
        }

        _progressBar.gameObject.SetActive(true);

        if (CookTimer <= _cookTime)
        {
            _progressBar.value = Mathf.Clamp01(CookTimer / _cookTime);

            if (_progressFill != null)
                _progressFill.color = Color.green;
        }
        else
        {
            _progressBar.value = 1f;

            float burnProgress = Mathf.InverseLerp(_cookTime, _burnTime, CookTimer);

            if (_progressFill != null)
            {
                _progressFill.color = Color.Lerp(
                    Color.green,
                    Color.red,
                    burnProgress
                );
            }
        }
    }
}