using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField]
    AudioSource musicSource;

    [SerializeField]
    AudioClip[] playlist;

    [Range(0f, 1f)]

    [SerializeField]
    float volume = 0.5f;

    [SerializeField]
    float fadeInDuration = 3f;

    [SerializeField]
    float fadeOutDuration = 1.5f;

    static int lastIndex = -1;
    static readonly System.Random rng = new System.Random(System.Guid.NewGuid().GetHashCode());

    float fadeTimer;
    bool isFadingOut = false;
    float fadeOutStartVolume;
    float fadeOutTimer;

    void OnEnable()
    {
        CarHandler.OnPlayerCrashed += StartFadeOut;
    }

    void OnDisable()
    {
        CarHandler.OnPlayerCrashed -= StartFadeOut;
    }

    void Start()
    {
        if (musicSource == null || playlist == null || playlist.Length == 0)
        {
            Debug.LogError("MusicManager: AudioSource atau playlist belum diisi.", this);
            enabled = false;
            return;
        }

        musicSource.Stop();
        musicSource.loop = false;
        musicSource.playOnAwake = false;

        PlayRandomTrack();
    }

    void Update()
    {
        if (isFadingOut)
        {
            fadeOutTimer += Time.deltaTime;
            float t = Mathf.Clamp01(fadeOutTimer / fadeOutDuration);
            musicSource.volume = Mathf.Lerp(fadeOutStartVolume, 0f, t);

            if (t >= 1f)
            {
                musicSource.Stop();
                enabled = false; 
            }
            return;
        }

        if (!musicSource.isPlaying)
        {
            PlayRandomTrack();
            return;
        }

        if (fadeTimer < fadeInDuration)
        {
            fadeTimer += Time.deltaTime;
            float t = Mathf.Clamp01(fadeTimer / fadeInDuration);
            musicSource.volume = Mathf.Lerp(0f, volume, t);
        }
    }

    void StartFadeOut()
    {
        if (isFadingOut || musicSource == null)
            return;

        isFadingOut = true;
        fadeOutTimer = 0f;
        fadeOutStartVolume = musicSource.volume;
    }

    void PlayRandomTrack()
    {
        int index = rng.Next(0, playlist.Length);

        if (playlist.Length > 1)
        {
            while (index == lastIndex)
                index = rng.Next(0, playlist.Length);
        }

        lastIndex = index;

        fadeTimer = 0f;
        musicSource.volume = 0f;
        musicSource.clip = playlist[index];
        musicSource.Play();
    }
}