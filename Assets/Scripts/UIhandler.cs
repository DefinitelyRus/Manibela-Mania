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
            SceneManager.LoadScene("MainMenu");
       
    }

    public void RestartScene()
    {
            SceneManager.LoadScene("MainRoad");
       
    }

    public void doExitGame() 
    { Application.Quit(); }
}
