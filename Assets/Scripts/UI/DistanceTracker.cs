using UnityEngine;
using TMPro;

public class DistanceTracker : MonoBehaviour
{
    [SerializeField]
    Transform player;

    [SerializeField]
    TMP_Text hudText;

    float startZ;
    bool isRunning = true;

    public float Distance { get; private set; }

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("DistanceTracker: referensi player belum diisi.", this);
            enabled = false;
            return;
        }

        startZ = player.position.z;
    }

    void OnEnable()
    {
        CarHandler.OnPlayerCrashed += StopTracking;
    }

    void OnDisable()
    {
        CarHandler.OnPlayerCrashed -= StopTracking;
    }

    void Update()
    {
        if (!isRunning)
            return;

        Distance = Mathf.Max(0f, player.position.z - startZ);

        if (hudText != null)
            hudText.text = FormatDistance(Distance);
    }

    void StopTracking()
    {
        isRunning = false;
    }

    public static string FormatDistance(float meters)
    {
        if (meters >= 1000f)
            return (meters / 1000f).ToString("F2") + " km";

        return Mathf.FloorToInt(meters) + " m";
    }
}