using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIhandler : MonoBehaviour
{
    [SerializeField] GameObject SettingsUI;

    [Header("Sprite Toggle")]
    [SerializeField] Image toggleTarget; 
    [SerializeField] Sprite openedSprite;
    [SerializeField] Sprite closedSprite;

   private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (SettingsUI.activeSelf)
                OnExit();
            else
                open();
        }
    }

       public void open()
    {
        SettingsUI.SetActive(true);
        if (toggleTarget != null && openedSprite != null)
            toggleTarget.sprite = openedSprite;
    }

    public void OnExit()
    {
        SettingsUI.SetActive(false);
        if (toggleTarget != null && closedSprite != null)
            toggleTarget.sprite = closedSprite;
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

