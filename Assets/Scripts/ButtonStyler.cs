using System.Collections;
using NaughtyAttributes;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Styles a button according to Figma <see href="https://www.figma.com/design/cQXaxDvw8ePX61iXzgBAGN/2.0-Elements--For-External-Use-?node-id=17-131">Button Styles</see>.
/// If you need a reference to the button, refer to it directly, do not reference this script.
/// You should not have to edit any of the children of the button, only the fields in this script.
/// 
/// NOTE: The disabled state only updates in play mode.
/// </summary>
#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
[RequireComponent(typeof(Button))]
public class ButtonStyler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler

{
    [InfoBox("\n  Styles a button according to Figma Button Styles.\n"
        + "  If you need a reference to the button, refer to it directly, do not reference this script.\n"
        + "  You should not have to edit any of the children of the button, only the fields in this script.\n\n"
        + "  NOTE: The disabled state only updates in play mode.")]

    [Space]

    [SerializeField] private ButtonType type = ButtonType.Filled;
    [SerializeField] private ButtonColor color = ButtonColor.Brand;
    [ShowIf("color", ButtonColor.Custom)][SerializeField] private Color customBackgroundColor;
    [ShowIf("color", ButtonColor.Custom)][SerializeField] private Color customBorderColor;
    [ShowIf("color", ButtonColor.Custom)][SerializeField] private Color customTextColor;
    [ShowIf("color", ButtonColor.Custom)][SerializeField] private Color customLeadingIconColor;
    [ShowIf("color", ButtonColor.Custom)][SerializeField] private Color customTrailingIconColor;
    [SerializeField] private bool isRounded = false;
    //[SerializeField] private Sprite leadingIcon;
    [SerializeField] private string buttonText = "Button";
    //[SerializeField] private Sprite trailingIcon;

    [HorizontalLine]

    [Foldout("References (Do not touch)")][SerializeField] private Button button;
    [Foldout("References (Do not touch)")][SerializeField] private Image leadingImage;
    [Foldout("References (Do not touch)")][SerializeField] private Image trailingImage;
    [Foldout("References (Do not touch)")][SerializeField] private Image background;
    [Foldout("References (Do not touch)")][SerializeField] private Image border;
    [Foldout("References (Do not touch)")][SerializeField] private UniformModifier[] roundModifiers;
    [Foldout("References (Do not touch)")][SerializeField] private HorizontalLayoutGroup layoutGroup;
    [Foldout("References (Do not touch)")][SerializeField] private TextMeshProUGUI text;

    private Coroutine _disableStateCoroutine;
    private readonly CompositeDisposable _disposable = new();

    /// <summary>
    /// Delay before applying the disabled state to prevent flickering by UIButton's Disable Button Interval.
    /// </summary>
    private const float DISABLED_STATE_DELAY = 0.11f;
    private const int PADDING_WITH_ICON = 16;
    private const int PADDING_WITHOUT_ICON = 24;
    private const float DEFAULT_RADIUS = 12f;
    private const float ROUNDED_RADIUS = 999f;


    //NEW REFERENCES FOR HOVER EFFECTS AND PRESS EFFECTS
    public Image targetImage;
    public Color normalColor;
    public Color hoverColor;
    public RectTransform targetTransform;   // Usually the button's RectTransform
    public Image backgroundImage;           // The background to color
    public Vector3 pressedScale = new Vector3(0.96f, 0.96f, 1f);
    public float animationSpeed = 8f;
    public Color pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    private Vector3 originalScale;
    private Color originalColor;
    private Coroutine scaleRoutine;
    private Coroutine colorRoutine;


    private void Awake()
    {
        //UpdateUI();

        // Always ensure targetImage is active at startup
        if (targetImage != null && targetImage.gameObject.activeSelf == false)
        {
            targetImage.gameObject.SetActive(false);
        }
        else
        {
            targetImage.gameObject.SetActive(true);
        }

        // (Optional) Also reset its color if needed
        if (backgroundImage != null)
            backgroundImage.color = originalColor;
    }

    private void Start()
    {
        if (Application.isPlaying)
        {
            button.ObserveEveryValueChanged(x => x.interactable)
                .Subscribe(_ => OnInteractableChanged())
                .AddTo(_disposable);
        }

        //null checking and get components
        if (targetTransform == null)
            targetTransform = GetComponent<RectTransform>();

        if (targetTransform != null)
            originalScale = targetTransform.localScale;

        if (backgroundImage != null)
            originalColor = backgroundImage.color;
    }



    public void OnHoverEnter()
    {
        // When the mouse pointer enters the button area, change the target image color to the hover color
        if (targetImage != null)
            targetImage.color = hoverColor;
    }

    public void OnHoverExit()
    {
        // When the mouse pointer exits the button area, revert the target image color to the normal color
        if (targetImage != null)
            targetImage.color = normalColor;
    }

    public void ImageShow()
    {
        // Show the background image when needed (e.g., on hover)
        SetBackgroundOnHover(true);
    }

    public void ImageHide()
    {
        // Hide the background image when not needed (e.g., on hover exit)
        SetBackgroundOnHover(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Called when the button is pressed (mouse/touch down)
        // Instantly apply the pressed scale and color for immediate feedback
        if (scaleRoutine != null) StopCoroutine(scaleRoutine);
        if (colorRoutine != null) StopCoroutine(colorRoutine);

        if (targetTransform != null)
            targetTransform.localScale = pressedScale; // Shrink the button slightly
        if (backgroundImage != null)
        {
            backgroundImage.color = pressedColor; // Change background to pressed color
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Called when the button is released (mouse/touch up)
        // Smoothly animate back to the original scale and color
        if (targetTransform != null)
            scaleRoutine = StartCoroutine(ScaleTo(originalScale)); // Animate scale back to normal
        if (backgroundImage != null)
        {
            // Restore to original color (not transparent)
            colorRoutine = StartCoroutine(ColorTo(originalColor));
        }

        if (targetImage != null && !targetImage.gameObject.activeSelf)
            targetImage.gameObject.SetActive(true);
    }

    // Coroutine to smoothly scale the button to the target scale
    IEnumerator ScaleTo(Vector3 target)
    {
        if (targetTransform == null)
            yield break;
        // Continue scaling until the button is close enough to the target scale
        while (Vector3.Distance(targetTransform.localScale, target) > 0.001f)
        {
            targetTransform.localScale = Vector3.Lerp(targetTransform.localScale, target, Time.deltaTime * animationSpeed);
            yield return null;
        }
        targetTransform.localScale = target; // Ensure final scale is set
    }

    // Coroutine to smoothly change the background color to the target color
    IEnumerator ColorTo(Color target)
    {
        if (backgroundImage == null)
            yield break;
        // Continue changing color until close enough to the target color
        while (Vector4.Distance(backgroundImage.color, target) > 0.01f)
        {
            backgroundImage.color = Color.Lerp(backgroundImage.color, target, Time.deltaTime * animationSpeed);
            yield return null;
        }
        backgroundImage.color = target; // Ensure final color is set
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        //UpdateUI();
    }
#endif

    private void OnDestroy()
    {
        if (_disableStateCoroutine != null)
        {
            StopCoroutine(_disableStateCoroutine);
        }

        if (_disposable?.IsDisposed == false)
        {
            _disposable.Dispose();
        }
    }

    /// <summary>
    /// Update the button when the interactable state changes.
    /// Debounces the update to prevent flickering caused by UIButton.
    /// </summary>
    private void OnInteractableChanged()
    {
        if (_disableStateCoroutine != null)
        {
            StopCoroutine(_disableStateCoroutine);
        }

        if (!button.interactable)
        {
            if (isActiveAndEnabled)
            {
                _disableStateCoroutine = StartCoroutine(DelayedApplyDisabledState());
            }
            else
            {
                ApplyDisabledState();
            }
        }
        else
        {
            //UpdateUiColor();
        }
    }

    /// <summary>
    /// Delay the application of the disabled state to prevent flickering.
    /// </summary>
    /// <returns></returns>
    private IEnumerator DelayedApplyDisabledState()
    {
        yield return new WaitForSeconds(DISABLED_STATE_DELAY);

        if (!button.interactable)
        {
            ApplyDisabledState();
        }
        else
        {
            //UpdateUiColor();
        }
    }

    /// <summary>
    /// Update the UI, including the colors, text, icons, padding, and rounding.
    /// </summary>
    private void UpdateUI()
    {
        //UpdateUiColor();

        // Text
        text.gameObject.SetActive(!string.IsNullOrEmpty(buttonText));
        text.text = buttonText;

        // Icons
        //leadingImage.gameObject.SetActive(leadingIcon != null);
        //leadingImage.sprite = leadingIcon;

        //trailingImage.gameObject.SetActive(trailingIcon != null);
        //trailingImage.sprite = trailingIcon;

        // Padding
        // layoutGroup.padding = new RectOffset(
        //     leadingIcon != null ? PADDING_WITH_ICON : PADDING_WITHOUT_ICON,
        //     trailingIcon != null ? PADDING_WITH_ICON : PADDING_WITHOUT_ICON,
        //     0,
        //     0
        // );

        // Rounding
        foreach (var modifier in roundModifiers)
        {
            modifier.Radius = isRounded ? ROUNDED_RADIUS : DEFAULT_RADIUS;
        }
    }

    private void UpdateUiColor()
    {
        ApplyContainerVisibility();

        if (!button.interactable)
        {
            ApplyDisabledState();
            return;
        }

        SetTargetGraphic();

        if (color == ButtonColor.Custom)
        {
            background.color = customBackgroundColor;
            border.color = customBorderColor;
            text.color = customTextColor;
            leadingImage.color = customLeadingIconColor;
            trailingImage.color = customTrailingIconColor;
            return;
        }

        ApplyTextAndIconColor();
        ApplyBackgroundAndBorderColor();
    }

    /// <summary>
    /// Apply the visibility of the container based on the button type.
    /// </summary>
    private void ApplyContainerVisibility()
    {
        background.gameObject.SetActive(type != ButtonType.NoContainer);
        border.gameObject.SetActive(type == ButtonType.Outlined);
    }

    /// <summary>
    /// Apply the disabled state of the button.
    /// NOTE: This only works in play mode.
    /// </summary>
    private void ApplyDisabledState()
    {
        background.color = ColorManager.GetColorByToken("brand/subtle/surface-container/highest").WithAlpha(0.2f);
        border.color = ColorManager.GetColorByToken("brand/subtle/surface-border/low");
        text.color = ColorManager.GetColorByToken("system/text/light/disabled");
        leadingImage.color = ColorManager.GetColorByToken("system/text/light/disabled");
        trailingImage.color = ColorManager.GetColorByToken("system/text/light/disabled");

        background.gameObject.SetActive(type != ButtonType.NoContainer && type != ButtonType.Outlined);
    }

    /// <summary>
    /// Apply the text and icon color based on the button type and color.
    /// </summary>
    private void ApplyTextAndIconColor()
    {
        Color targetColor = Color.clear;
        switch (type)
        {
            case ButtonType.Filled:
                targetColor = ColorManager.GetColorByToken("system/text/dark/high");
                break;

            case ButtonType.Tonal:
                targetColor = ColorManager.GetColorByToken("system/text/light/high");
                break;

            case ButtonType.Outlined:
            case ButtonType.NoContainer:
                switch (color)
                {
                    case ButtonColor.Brand:
                        targetColor = ColorManager.GetColorByToken("brand/primary/on-surface/dark/high");
                        break;

                    case ButtonColor.Warning:
                        targetColor = ColorManager.GetColorByToken("warning/on-surface/dark/high");
                        break;

                    case ButtonColor.Danger:
                        targetColor = ColorManager.GetColorByToken("danger/on-surface/dark/high");
                        break;

                    case ButtonColor.Subtle:
                        targetColor = ColorManager.GetColorByToken("system/text/light/high");
                        break;
                }
                break;
        }

        text.color = targetColor;
        leadingImage.color = targetColor;
        trailingImage.color = targetColor;
    }

    /// <summary>
    /// Apply the background and border color based on the button color.
    /// </summary>
    private void ApplyBackgroundAndBorderColor()
    {
        if (type == ButtonType.NoContainer)
        {
            // Nothing to do
            return;
        }

        // Border
        switch (color)
        {
            case ButtonColor.Brand:
                border.color = ColorManager.GetColorByToken("brand/primary/surface-border/dark/high");
                break;

            case ButtonColor.Warning:
                border.color = ColorManager.GetColorByToken("warning/surface-border/high");
                break;

            case ButtonColor.Danger:
                border.color = ColorManager.GetColorByToken("danger/surface-border/dark/high");
                break;

            case ButtonColor.Subtle:
                border.color = ColorManager.GetColorByToken("brand/subtle/surface-border/medium");
                break;
        }

        // Background
        switch (color, type)
        {
            case (ButtonColor.Brand, ButtonType.Filled):
                background.color = ColorManager.GetColorByToken("brand/primary/surface-container/light/main");
                break;

            case (ButtonColor.Brand, ButtonType.Tonal):
                background.color = ColorManager.GetColorByToken("brand/primary/surface-container/dark/main");
                break;

            case (ButtonColor.Brand, ButtonType.Outlined):
                background.color = ColorManager.GetColorByToken("brand/primary/surface-container/light/main").WithAlpha(0.05f);
                break;

            case (ButtonColor.Warning, ButtonType.Filled):
                background.color = ColorManager.GetColorByToken("warning/surface-container/light/low");
                break;

            case (ButtonColor.Warning, ButtonType.Tonal):
                background.color = ColorManager.GetColorByToken("warning/surface-container/dark/main");
                break;

            case (ButtonColor.Warning, ButtonType.Outlined):
                background.color = ColorManager.GetColorByToken("warning/surface-container/light/low").WithAlpha(0.05f);
                break;

            case (ButtonColor.Danger, ButtonType.Filled):
                background.color = ColorManager.GetColorByToken("danger/surface-container/light/low");
                break;

            case (ButtonColor.Danger, ButtonType.Tonal):
                background.color = ColorManager.GetColorByToken("danger/surface-container/dark/main");
                break;

            case (ButtonColor.Danger, ButtonType.Outlined):
                background.color = ColorManager.GetColorByToken("danger/surface-container/light/low").WithAlpha(0.05f);
                break;

            case (ButtonColor.Subtle, ButtonType.Filled):
                background.color = ColorManager.GetColorByToken("brand/subtle/surface-container/light");
                break;

            case (ButtonColor.Subtle, ButtonType.Tonal):
                background.color = ColorManager.GetColorByToken("brand/subtle/surface-container/main");
                break;

            case (ButtonColor.Subtle, ButtonType.Outlined):
                background.color = ColorManager.GetColorByToken("brand/subtle/surface-container/light").WithAlpha(0.05f);
                break;

            default:
                throw new System.ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// Set the target graphic for the button based on the button type.
    /// </summary>
    private void SetTargetGraphic()
    {
        button.targetGraphic = type == ButtonType.Outlined || type == ButtonType.NoContainer ? text : background;
    }

    public void SetBackgroundOnHover(bool show)
    {
        if (targetImage != null)
            targetImage.gameObject.SetActive(show);
    }

    [System.Serializable]
    private enum ButtonType
    {
        Filled,
        Outlined,
        Tonal,
        NoContainer,
    }

    [System.Serializable]
    private enum ButtonColor
    {
        Brand,
        Warning,
        Danger,
        Subtle,
        Custom,
    }
}
