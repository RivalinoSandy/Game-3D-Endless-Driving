using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [SerializeField]
    GameObject gameOverPanel;

    [SerializeField]
    DistanceTracker distanceTracker;

    [SerializeField]
    TMP_Text finalDistanceText;

    void Awake()
    {
        gameOverPanel.SetActive(false);
    }

    void OnEnable()
    {
        CarHandler.OnPlayerCrashed += ShowGameOver;
    }

    void OnDisable()
    {
        CarHandler.OnPlayerCrashed -= ShowGameOver;
    }

    void ShowGameOver()
    {
        float distance = distanceTracker != null ? distanceTracker.Distance : 0f;
        float best = PlayerPrefs.GetFloat("BestDistance", 0f);

        if (distance > best)
        {
            best = distance;
            PlayerPrefs.SetFloat("BestDistance", best);
            PlayerPrefs.Save();
        }

        if (finalDistanceText != null)
            finalDistanceText.text =
                "Distance: " + DistanceTracker.FormatDistance(distance) + "\n" +
                "Best Distance: " + DistanceTracker.FormatDistance(best);

        gameOverPanel.SetActive(true);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}