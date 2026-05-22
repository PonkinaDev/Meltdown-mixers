using Fusion;
using UnityEngine;

[CreateAssetMenu(fileName = "AvatarDefinition", menuName = "Game/Avatar Definition")]
public class AvatarDefinition : ScriptableObject
{
    [SerializeField] private string _avatarName;
[SerializeField] private NetworkPrefabRef _prefab;
    [SerializeField] private Sprite _preview;
    [SerializeField] private Color _accentColor = Color.white;

    public string AvatarName => _avatarName;
public NetworkPrefabRef Prefab => _prefab;
    public Sprite Preview => _preview;
    public Color AccentColor => _accentColor;
}