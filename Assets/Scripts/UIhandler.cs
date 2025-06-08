using UnityEngine;
using UnityEngine.SceneManagement;

public class UIhandler : MonoBehaviour
{
    [SerializeField] GameObject SettingsUI;

   

    public void open()
    {
        SettingsUI.SetActive(true);
    }

    public void OnExit()
    {
        SettingsUI.SetActive(false);
    }

    public string sceneName;

    public void ChangeScene()
    {
        Destroy(GameManager.Instance.gameObject);
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void doExitGame()
    {
        Application.Quit();
    }

    public void StartNextDay()
    {
        SceneManager.sceneLoaded += OnSceneLoadedAfterNextDay;
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnSceneLoadedAfterNextDay(Scene scene, LoadSceneMode mode)
    {
        // Reconnect and resume gameplay only AFTER the scene is fully loaded
        GameManager.Instance.ReconnectUI();
        GameManager.Instance.StartNextDay();

        // Unregister so this only runs once
        SceneManager.sceneLoaded -= OnSceneLoadedAfterNextDay;
    }
    
    
    }

