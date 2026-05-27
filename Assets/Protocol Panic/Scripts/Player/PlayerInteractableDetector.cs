using UnityEngine;

public class PlayerInteractableDetector : MonoBehaviour
{
    [SerializeField]
    private float _interactionRadius = 2f;

    public IngredientDispenser NearbyDispenser
    {
        get;
        private set;
    }

    public PotionMixer NearbyMixer
    {
        get;
        private set;
    }

    public PotionHotPlate NearbyHotPlate
    {
        get;
        private set;
    }

    public DeliveryZone NearbyDeliveryZone
    {
        get;
        private set;
    }

    public TrashBin NearbyTrashBin
    {
        get;
        private set;
    }

    public void Detect()
    {
        Collider[] hits =
            Physics.OverlapSphere(
                transform.position,
                _interactionRadius
            );

        NearbyDispenser =
            FindClosest<IngredientDispenser>(hits);

        NearbyMixer =
            FindClosest<PotionMixer>(hits);

        NearbyHotPlate =
            FindClosest<PotionHotPlate>(hits);

        NearbyDeliveryZone =
            FindClosest<DeliveryZone>(hits);

        NearbyTrashBin =
            FindClosest<TrashBin>(hits);
    }

    private T FindClosest<T>(
        Collider[] hits
    ) where T : Component
    {
        T closest = null;

        float closestDistance =
            float.MaxValue;

        foreach (Collider hit in hits)
        {
            T component =
                hit.GetComponent<T>();

            if (component == null)
                continue;

            float distance =
                Vector3.Distance(
                    transform.position,
                    component.transform.position
                );

            if (distance >= closestDistance)
                continue;

            closestDistance = distance;
            closest = component;
        }

        return closest;
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