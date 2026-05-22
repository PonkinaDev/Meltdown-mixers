using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AvatarSlotUI : MonoBehaviour
{
    [SerializeField] private Image _preview;
    [SerializeField] private Image _border;
    [SerializeField] private Button _button;
    [SerializeField] private GameObject _takenOverlay;
    [SerializeField] private GameObject _selectedIndicator;
    [SerializeField] private TextMeshProUGUI _nameLabel;

    private int _index;
    private IAvatarSelectionService _service;

    public void Initialize(int index, AvatarDefinition definition, IAvatarSelectionService service)
    {
        _index = index;
        _service = service;
        _preview.sprite = definition.Preview;
        _nameLabel.text = definition.AvatarName;
        _border.color = definition.AccentColor;
        _button.onClick.AddListener(OnClicked);
    }

    public void Refresh(bool taken, bool selected)
    {
        _takenOverlay.SetActive(taken && !selected);
        _selectedIndicator.SetActive(selected);
        _button.interactable = !taken || selected;
    }

    public void SetInteractable(bool value)
{
    _button.interactable = value;
}

    private void OnClicked() => _service.RequestSelectAvatar(_index);

    private void OnDestroy() => _button.onClick.RemoveListener(OnClicked);
}