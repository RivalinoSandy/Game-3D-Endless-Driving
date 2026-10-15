using UnityEngine;

public class RandomizeObject : MonoBehaviour
{
    [SerializeField] Vector3 localRotationMin = Vector3.zero;
    [SerializeField] Vector3 localRotationMax = Vector3.zero;

    [SerializeField] float localScaleMultiplierMin = 0.8f;
    [SerializeField] float localScaleMultiplierMax = 1.5f;

    Vector3 localScaleOriginal;

    private void Awake()
    {
        localScaleOriginal = transform.localScale;
    }

    void OnEnable()
    {
        transform.localRotation = Quaternion.Euler(
            Random.Range(localRotationMin.x, localRotationMax.x),
            Random.Range(localRotationMin.y, localRotationMax.y),
            Random.Range(localRotationMin.z, localRotationMax.z));

        transform.localScale = localScaleOriginal *
            Random.Range(localScaleMultiplierMin, localScaleMultiplierMax);
    }
}