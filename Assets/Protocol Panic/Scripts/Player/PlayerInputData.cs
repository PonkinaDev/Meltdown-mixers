using Fusion;
using UnityEngine;
public struct PlayerInputData : INetworkInput
{
    public Vector2 MovementInput;
    public NetworkBool PickupPressed;
}