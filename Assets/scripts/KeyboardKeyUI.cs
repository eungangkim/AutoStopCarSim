using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KeyboardKeyUI : MonoBehaviour
{
    [Header("Key")]
    public KeyCode keyCode;

    [Header("UI")]
    public Image keyImage;
    public TMP_Text keyText;

    [Header("Colors")]
    public Color normalColor = new Color(0.18f, 0.18f, 0.18f, 0.9f);
    public Color pressedColor = new Color(0.2f, 0.8f, 0.3f, 1f);
    public Color normalTextColor = Color.white;
    public Color pressedTextColor = Color.black;

    [Header("Scale")]
    public float normalScale = 1.0f;
    public float pressedScale = 1.08f;
    public float scaleSpeed = 12f;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (keyImage == null)
        {
            keyImage = GetComponent<Image>();
        }

        if (keyText == null)
        {
            keyText = GetComponentInChildren<TMP_Text>();
        }
    }

    private void Update()
    {
        bool isPressed = Input.GetKey(keyCode);

        UpdateVisual(isPressed);
    }

    private void UpdateVisual(bool isPressed)
    {
        if (keyImage != null)
        {
            keyImage.color = isPressed ? pressedColor : normalColor;
        }

        if (keyText != null)
        {
            keyText.color = isPressed ? pressedTextColor : normalTextColor;
        }

        if (rectTransform != null)
        {
            float targetScale = isPressed ? pressedScale : normalScale;

            rectTransform.localScale = Vector3.Lerp(
                rectTransform.localScale,
                Vector3.one * targetScale,
                Time.deltaTime * scaleSpeed
            );
        }
    }
}