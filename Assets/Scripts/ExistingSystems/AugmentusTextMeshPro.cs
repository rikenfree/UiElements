using TMPro;
using UnityEngine;

public class AugmentusTextMeshPro : TextMeshProUGUI
{
    public UITypography TypographyAsset
    {
        get => typographyAsset;
        set
        {
            typographyAsset = value;
            ApplyTypography(role, typographyAsset);
            m_havePropertiesChanged = true;
            SetVerticesDirty();
        }
    }

    [Tooltip("The typography asset to use for this text. Assign this on the MonoScript so that it will be applied on all components.")]
    [ReadOnlyField][SerializeField] private UITypography typographyAsset;

    public TypographyRole Role
    {
        get => role;
        set
        {
            role = value;
            ApplyTypography(role, typographyAsset);
            m_havePropertiesChanged = true;
            SetVerticesDirty();
        }
    }

    [Tooltip("Applies a preset typography to the text.")]
    [SerializeField] private TypographyRole role = TypographyRole.BodyMedium;

    public TextEmphasis Emphasis
    {
        get => textEmphasis;
        set
        {
            textEmphasis = value;
            ApplyEmphasis(textEmphasis, typographyAsset);
            m_havePropertiesChanged = true;
            SetVerticesDirty();
        }
    }

    [Tooltip("Applies a preset color to the text. If set to Custom, the color will not be changed.")]
    [SerializeField] private TextEmphasis textEmphasis = TextEmphasis.High;

    [SerializeField] private string colorManagerToken = "";

    public string ColorManagerToken
    {
        get => colorManagerToken;
        set
        {
            if (textEmphasis == TextEmphasis.ColorManager)
            {
                ApplyEmphasis(textEmphasis, typographyAsset);
                m_havePropertiesChanged = true;
                SetVerticesDirty();
            }
        }
    }

    public bool IsDisabled
    {
        get => _isDisabled;
        set
        {
            _isDisabled = value;
            if (_isDisabled)
                Emphasis = TextEmphasis.Disabled;
            else
                Emphasis = TextEmphasis.High;
            ApplyEmphasis(textEmphasis, typographyAsset);
            m_havePropertiesChanged = true;
            SetVerticesDirty();
            SetLayoutDirty();
        }
    }

    private bool _isDisabled;

#if UNITY_EDITOR
    /// <summary>
    /// Apply the typography settings when the component is first added.
    /// </summary>
    protected override void Reset()
    {
        base.Reset();

        ApplyTypography(Role, TypographyAsset);
        ApplyEmphasis(Emphasis, TypographyAsset);
    }
#endif

    public void ApplyTypography(TypographyRole settings, UITypography typo)
    {
        if (typo == null) return;

        switch (settings)
        {
            case TypographyRole.HeadingLarge:
                ApplyTypography(typo.headingLarge);
                break;
            case TypographyRole.HeadingMedium:
                ApplyTypography(typo.headingMedium);
                break;
            case TypographyRole.HeadingSmall:
                ApplyTypography(typo.headingSmall);
                break;
            case TypographyRole.BodyLarge:
                ApplyTypography(typo.bodyLarge);
                break;
            case TypographyRole.BodyMedium:
                ApplyTypography(typo.bodyMedium);
                break;
            case TypographyRole.BodySmall:
                ApplyTypography(typo.bodySmall);
                break;
            case TypographyRole.BodyStrongLarge:
                ApplyTypography(typo.bodyStrongLarge);
                break;
            case TypographyRole.BodyStrongMedium:
                ApplyTypography(typo.bodyStrongMedium);
                break;
            case TypographyRole.BodyStrongSmall:
                ApplyTypography(typo.bodyStrongSmall);
                break;
            case TypographyRole.IndicatorLarge:
                ApplyTypography(typo.indicatorLarge);
                break;
            case TypographyRole.IndicatorMedium:
                ApplyTypography(typo.indicatorMedium);
                break;
            case TypographyRole.IndicatorSmall:
                ApplyTypography(typo.indicatorSmall);
                break;
            case TypographyRole.IndicatorStrongLarge:
                ApplyTypography(typo.indicatorStrongLarge);
                break;
            case TypographyRole.IndicatorStrongMedium:
                ApplyTypography(typo.indicatorStrongMedium);
                break;
            case TypographyRole.IndicatorStrongSmall:
                ApplyTypography(typo.indicatorStrongSmall);
                break;
            case TypographyRole.IndicatorNumeralLarge:
                ApplyTypography(typo.indicatorNumeralLarge);
                break;
            case TypographyRole.IndicatorNumeralSmall:
                ApplyTypography(typo.indicatorNumeralSmall);
                break;
            case TypographyRole.Monospace:
                ApplyTypography(typo.monospace);
                break;
        }
    }

    private void ApplyTypography(TypographySettings settings)
    {
        if (settings == null || settings.font == null) return;

        font = settings.font;
        fontSize = settings.fontSize;
        m_lineHeight = settings.lineHeight;
        characterSpacing = settings.tracking;
    }

    public void ApplyEmphasis(TextEmphasis emphasis, UITypography typo)
    {
        if (typo == null) return;

        switch (emphasis)
        {
            case TextEmphasis.High:
                color = typo.highEmphasisText;
                break;
            case TextEmphasis.Medium:
                color = typo.mediumEmphasisText;
                break;
            case TextEmphasis.Disabled:
                color = typo.disabledText;
                break;
            case TextEmphasis.Custom:
                break;
            case TextEmphasis.ColorManager:
                color = ColorManager.GetColorByToken(colorManagerToken);
                break;
        }
    }

    protected override void OnDestroy()
    {
        typographyAsset = null;
        base.OnDestroy();
    }

    /* 
    // Uncomment this if you want to use TypographySetter (don't do it)
    protected override void Awake()
    {
        base.Awake();

        if (TypographySetter.Instance != null
            && TypographySetter.Instance.TypographyAsset != null)
        {
            TypographyAsset = TypographySetter.Instance.TypographyAsset;
        }
    }
    */
}