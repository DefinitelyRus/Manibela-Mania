using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ButtonSpriteChanger : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Sprite defaultSprite;
    public Sprite clickedSprite;
    public string sceneToLoad;

    public RectTransform buttonText;         // Assign the Text or TMP_Text RectTransform
    public float pressOffset = -5f;          // How much to move the text down

    private Image buttonImage;
    private Vector2 originalTextPosition;

    void Start()
    {
        buttonImage = GetComponent<Image>();
        buttonImage.sprite = defaultSprite;

        if (buttonText != null)
            originalTextPosition = buttonText.anchoredPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        buttonImage.sprite = clickedSprite;

        if (buttonText != null)
        {
            buttonText.anchoredPosition = originalTextPosition + new Vector2(0, pressOffset);
        }
    }
public void OnPointerUp(PointerEventData eventData)
{
    buttonImage.sprite = defaultSprite;

    if (buttonText != null)
    {
        buttonText.anchoredPosition = originalTextPosition;
    }

    //  STOP music playback BEFORE transitioning
    if (SoundManager.Instance != null)
        SoundManager.Instance.StopMusic();

    PlayerPrefs.SetInt("CurrentDay", 1);
    PlayerPrefs.Save();
    Debug.Log(" Reset progress to Day 1!");

    //  THEN load the scene
    SceneManager.LoadScene(sceneToLoad);
}

}
