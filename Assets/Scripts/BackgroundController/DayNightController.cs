using UnityEngine;

public class DayNightController : MonoBehaviour
{
    [Header("Referensi")]
    [SerializeField] DistanceTracker distanceTracker;
    [SerializeField] Light sun; 

    [Header("Siklus")]
    [SerializeField] float distancePerPhase = 200f;

    [SerializeField] float transitionDuration = 5f;

    [Header("Siang")]
    [SerializeField] Color daySunColor = new Color(1f, 0.96f, 0.84f);
    [SerializeField] float daySunIntensity = 1.2f;
    [SerializeField] Color dayAmbient = new Color(0.55f, 0.6f, 0.7f);
    [SerializeField] float daySkyExposure = 1f;
    [SerializeField] Color dayFog = new Color(0.7f, 0.8f, 0.9f);

    [Header("Malam")]
    [SerializeField] Color nightSunColor = new Color(0.3f, 0.4f, 0.8f);
    [SerializeField] float nightSunIntensity = 0.15f;
    [SerializeField] Color nightAmbient = new Color(0.08f, 0.1f, 0.2f);
    [SerializeField] float nightSkyExposure = 0.1f;
    [SerializeField] Color nightFog = new Color(0.03f, 0.04f, 0.1f);



    float nightAmount;      
    Material skyboxInstance;

    void Start()
    {
        if (distanceTracker == null || sun == null)
        {
            Debug.LogError("DayNightController: DistanceTracker atau Sun belum diisi.", this);
            enabled = false;
            return;
        }

        if (RenderSettings.skybox != null)
        {
            skyboxInstance = new Material(RenderSettings.skybox);
            RenderSettings.skybox = skyboxInstance;
        }

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;

        Apply(0f);
    }

    void Update()
    {

        int phase = Mathf.FloorToInt(distanceTracker.Distance / distancePerPhase);
        float target = (phase % 2 == 1) ? 1f : 0f;

        float step = Time.deltaTime / Mathf.Max(transitionDuration, 0.01f);
        float newAmount = Mathf.MoveTowards(nightAmount, target, step);

        if (!Mathf.Approximately(newAmount, nightAmount))
        {
            nightAmount = newAmount;
            Apply(nightAmount);
        }
    }

    void Apply(float t)
    {
        sun.color = Color.Lerp(daySunColor, nightSunColor, t);
        sun.intensity = Mathf.Lerp(daySunIntensity, nightSunIntensity, t);

        RenderSettings.ambientLight = Color.Lerp(dayAmbient, nightAmbient, t);

        if (RenderSettings.fog)
            RenderSettings.fogColor = Color.Lerp(dayFog, nightFog, t);

        if (skyboxInstance != null && skyboxInstance.HasProperty("_Exposure"))
            skyboxInstance.SetFloat("_Exposure", Mathf.Lerp(daySkyExposure, nightSkyExposure, t));

    }
}