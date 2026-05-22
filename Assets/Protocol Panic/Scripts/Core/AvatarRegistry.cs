using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AvatarRegistry", menuName = "Game/Avatar Registry")]
public class AvatarRegistry : ScriptableObject
{
    [SerializeField] private AvatarDefinition[] _avatars;

    public int Count => _avatars.Length;
    public IReadOnlyList<AvatarDefinition> All => _avatars;
    public AvatarDefinition Get(int index) => _avatars[index];
}