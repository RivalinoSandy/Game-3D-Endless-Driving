using UnityEngine;

/// <summary>
/// Dipasang di prefab mobil traffic. Bergerak lurus pada sumbu Z (searah atau berlawanan arah pemain).
/// Spawner yang mengatur kecepatan, arah, dan lane.
/// Jika prefab belum punya Collider, Box Collider dibuat otomatis dari ukuran model
/// lalu dikecilkan sesuai pengaturan di Inspector.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class TrafficCar : MonoBehaviour
{
    [HideInInspector] public int prefabIndex;
    [HideInInspector] public int laneIndex;

    [Header("Ukuran Collider (1 = sama dengan model)")]
    [SerializeField] Vector3 colliderScale = new Vector3(0.8f, 0.9f, 0.9f);
    [SerializeField] bool applyToExistingBoxColliders = false;

    Rigidbody rb;
    float speed;
    int direction = 1; 

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        SetupCollider();
    }
    public static bool IsFrozen { get; private set; }

    public static void FreezeAll() => IsFrozen = true;

    public static void UnfreezeAll() => IsFrozen = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => IsFrozen = false;

    void SetupCollider()
    {
        Collider[] existing = GetComponentsInChildren<Collider>(true);

        if (existing.Length > 0)
        {
            if (applyToExistingBoxColliders)
            {
                foreach (Collider c in existing)
                {
                    if (c is BoxCollider existingBox)
                    {
                        existingBox.size = new Vector3(
                            existingBox.size.x * colliderScale.x,
                            existingBox.size.y * colliderScale.y,
                            existingBox.size.z * colliderScale.z);
                    }
                }
            }
            return;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            Debug.LogWarning($"TrafficCar '{name}': tidak ada Renderer, collider tidak bisa dibuat otomatis.", this);
            return;
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        Vector3 scale = transform.lossyScale;
        BoxCollider box = gameObject.AddComponent<BoxCollider>();
        box.center = transform.InverseTransformPoint(bounds.center);
        box.size = new Vector3(
            bounds.size.x / Mathf.Max(scale.x, 0.0001f) * colliderScale.x,
            bounds.size.y / Mathf.Max(scale.y, 0.0001f) * colliderScale.y,
            bounds.size.z / Mathf.Max(scale.z, 0.0001f) * colliderScale.z);
    }

    public void Init(float newSpeed, bool oncoming)
    {
        speed = newSpeed;
        direction = oncoming ? -1 : 1;

        rb.position = transform.position;
        rb.rotation = transform.rotation;
    }

    void FixedUpdate()
    {
        if (IsFrozen)
            return;

        rb.MovePosition(rb.position + Vector3.forward * (direction * speed * Time.fixedDeltaTime));
    }
}
