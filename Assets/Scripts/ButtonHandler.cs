using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class ButtonHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Sprite defaultSprite;
    public Sprite clickedSprite;
    private Image buttonImage;


      void Start()
    {
        buttonImage = GetComponent<Image>();
        buttonImage.sprite = defaultSprite;

       
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        buttonImage.sprite = clickedSprite;

       
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        buttonImage.sprite = defaultSprite;
    }

    
}
