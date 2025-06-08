using UnityEngine;

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
}
