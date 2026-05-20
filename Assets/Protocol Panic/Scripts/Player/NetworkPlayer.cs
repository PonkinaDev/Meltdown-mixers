using Fusion;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class NetworkPlayer : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 5f;

    [Header("Hold")]
    [SerializeField] private Transform _holdPoint;

    [Header("Interaction")]
    [SerializeField] private float _interactionRadius = 2f;

    private CharacterController _cc;

    [Networked]
    private Vector3 NetworkedPosition { get; set; }

    [Networked]
    private Quaternion NetworkedRotation { get; set; }

    [Networked]
    public IngredientType HeldIngredient { get; set; }

    private GameObject _heldVisual;

    private IngredientType _lastVisualIngredient =
        IngredientType.None;

    private IngredientDispenser _nearbyDispenser;

    private PotionMixer _nearbyMixer;

    private float _pickupCooldown = 0f;

    public override void Spawned()
    {
        _cc = GetComponent<CharacterController>();

        UpdateHeldVisual();
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        if (!GetInput(out PlayerInputData input))
            return;

        Vector3 direction = new Vector3(
            input.MovementInput.x,
            0f,
            input.MovementInput.y
        );

        direction.Normalize();

        Vector3 movement =
            direction *
            _moveSpeed *
            Runner.DeltaTime;

        _cc.Move(movement);

        NetworkedPosition = transform.position;

        if (direction != Vector3.zero)
        {
            NetworkedRotation =
                Quaternion.LookRotation(direction);
        }

        DetectNearbyDispenser();

        DetectNearbyMixer();

        _pickupCooldown -= Runner.DeltaTime;

        if (input.PickupPressed &&
            _pickupCooldown <= 0f)
        {
            _pickupCooldown = 0.25f;

            Interact();
        }
    }

    public override void Render()
    {
        transform.position = Vector3.Lerp(
            transform.position,
            NetworkedPosition,
            Runner.DeltaTime * 15f
        );

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            NetworkedRotation,
            Runner.DeltaTime * 15f
        );

        UpdateHeldVisual();
    }

    private void DetectNearbyDispenser()
    {
        _nearbyDispenser = null;

        Collider[] hits =
            Physics.OverlapSphere(
                transform.position,
                _interactionRadius
            );

        float closestDistance = 999f;

        foreach (Collider hit in hits)
        {
            IngredientDispenser dispenser =
                hit.GetComponent<IngredientDispenser>();

            if (dispenser == null)
                continue;

            float dist =
                Vector3.Distance(
                    transform.position,
                    dispenser.transform.position
                );

            if (dist < closestDistance)
            {
                closestDistance = dist;
                _nearbyDispenser = dispenser;
            }
        }
    }

    private void DetectNearbyMixer()
    {
        _nearbyMixer = null;

        Collider[] hits =
            Physics.OverlapSphere(
                transform.position,
                _interactionRadius
            );

        float closestDistance = 999f;

        foreach (Collider hit in hits)
        {
            PotionMixer mixer =
                hit.GetComponent<PotionMixer>();

            if (mixer == null)
                continue;

            float dist =
                Vector3.Distance(
                    transform.position,
                    mixer.transform.position
                );

            if (dist < closestDistance)
            {
                closestDistance = dist;
                _nearbyMixer = mixer;
            }
        }
    }

    private void Interact()
    {
        if (_nearbyMixer != null)
        {

            if (HeldIngredient ==
                IngredientType.None)
            {
                if (_nearbyMixer.CurrentColor !=
                    IngredientType.None)
                {
                    HeldIngredient =
                        _nearbyMixer.CurrentColor;

                    _nearbyMixer.CurrentColor =
                        IngredientType.None;
                }

                return;
            }


            bool success =
                _nearbyMixer.TryAddIngredient(
                    HeldIngredient
                );

            if (success)
            {
                ClearIngredient();
            }

            return;
        }


        if (_nearbyDispenser != null)
        {
            HeldIngredient =
                _nearbyDispenser.IngredientType;
        }
    }

    public bool HasIngredient()
    {
        return HeldIngredient != IngredientType.None;
    }

    public void ClearIngredient()
    {
        if (!HasStateAuthority)
            return;

        HeldIngredient =
            IngredientType.None;
    }

    private void UpdateHeldVisual()
    {
        if (_lastVisualIngredient ==
            HeldIngredient)
            return;

        _lastVisualIngredient =
            HeldIngredient;

        if (_heldVisual != null)
        {
            Destroy(_heldVisual);
        }

        if (HeldIngredient ==
            IngredientType.None)
            return;

        GameObject visual =
            GameObject.CreatePrimitive(
                PrimitiveType.Cube
            );

        visual.transform.SetParent(
            _holdPoint
        );

        visual.transform.localPosition =
            Vector3.zero;

        visual.transform.localRotation =
            Quaternion.identity;

        visual.transform.localScale =
            Vector3.one * 0.4f;

        Collider col =
            visual.GetComponent<Collider>();

        if (col != null)
        {
            Destroy(col);
        }

        Renderer renderer =
            visual.GetComponent<Renderer>();

        Material mat =
            new Material(
                Shader.Find(
                    "Universal Render Pipeline/Lit"
                )
            );

        renderer.material = mat;

        switch (HeldIngredient)
        {
            case IngredientType.Red:
                renderer.material.color =
                    Color.red;
                break;

            case IngredientType.Blue:
                renderer.material.color =
                    Color.blue;
                break;

            case IngredientType.Yellow:
                renderer.material.color =
                    Color.yellow;
                break;

            case IngredientType.Green:
                renderer.material.color =
                    Color.green;
                break;

            case IngredientType.Orange:
                renderer.material.color =
                    new Color(1f, 0.5f, 0f);
                break;

            case IngredientType.Purple:
                renderer.material.color =
                    new Color(0.5f, 0f, 1f);
                break;
        }

        _heldVisual = visual;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            _interactionRadius
        );
    }
}