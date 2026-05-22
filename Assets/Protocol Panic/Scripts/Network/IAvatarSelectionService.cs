using System;
using Fusion;

public interface IAvatarSelectionService
{
    event Action OnStateChanged;

    int AvatarCount { get; }
    bool IsAvatarTaken(int avatarIndex);
    int GetPlayerSelection(PlayerRef player);
    AvatarDefinition GetAvatarDefinition(int index);
    void RequestSelectAvatar(int avatarIndex);
    void RequestReady();
}