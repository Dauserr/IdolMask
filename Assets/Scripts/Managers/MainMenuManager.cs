using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Button playButton;

    private void Start()
    {
        playButton.onClick.AddListener(OnPlayPressed);
    }

    private void OnPlayPressed()
    {
        SceneManager.LoadScene("GameItself");
    }
}