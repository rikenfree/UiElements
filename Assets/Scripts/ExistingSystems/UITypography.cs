using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// A typography theme that can be applied to text elements.
/// Only one typography theme should exist in the project.
/// Also honestly not a big fan of this whole typography thing.
/// </summary>
// [CreateAssetMenu(fileName = "typography", menuName = "Create typography theme")]
public class UITypography : ScriptableObject
{
    [Header("Json")]
    [SerializeField] private TextAsset jsonFile;

    [Header("Font Weights")]
    public TMP_FontAsset regularFont;
    public TMP_FontAsset mediumFont;
    public TMP_FontAsset semiBoldFont;
    public TMP_FontAsset boldFont;
    public TMP_FontAsset monospaceFont;

    [Header("Preset Colors")]
    public Color highEmphasisText;
    public Color mediumEmphasisText;
    public Color disabledText;

    [Space]

    [Header("Presets")]
    [ReadOnlyField] public TypographySettings headingLarge;
    [ReadOnlyField] public TypographySettings headingMedium;
    [ReadOnlyField] public TypographySettings headingSmall;
    [ReadOnlyField] public TypographySettings bodyLarge;
    [ReadOnlyField] public TypographySettings bodyMedium;
    [ReadOnlyField] public TypographySettings bodySmall;
    [ReadOnlyField] public TypographySettings bodyStrongLarge;
    [ReadOnlyField] public TypographySettings bodyStrongMedium;
    [ReadOnlyField] public TypographySettings bodyStrongSmall;
    [ReadOnlyField] public TypographySettings indicatorLarge;
    [ReadOnlyField] public TypographySettings indicatorMedium;
    [ReadOnlyField] public TypographySettings indicatorSmall;
    [ReadOnlyField] public TypographySettings indicatorStrongLarge;
    [ReadOnlyField] public TypographySettings indicatorStrongMedium;
    [ReadOnlyField] public TypographySettings indicatorStrongSmall;
    [ReadOnlyField] public TypographySettings indicatorNumeralLarge;
    [ReadOnlyField] public TypographySettings indicatorNumeralSmall;
    [ReadOnlyField] public TypographySettings monospace;

    public void LoadFromJson()
    {
        string json = jsonFile.text;
        TextStylesContainer textStylesContainer = JsonUtility.FromJson<TextStylesContainer>(json);

        foreach (TextStyle textStyle in textStylesContainer.textStyles)
        {
            TMP_FontAsset fontAsset =
                textStyle.fontWeight == "Regular" ? regularFont
                : textStyle.fontWeight == "Medium" ? mediumFont
                : textStyle.fontWeight == "SemiBold" ? semiBoldFont
                : textStyle.fontWeight == "Bold" ? boldFont
                : textStyle.fontWeight == "Monospace" ? monospaceFont
                : null;

            TypographySettings settings = new TypographySettings
            {
                font = fontAsset,
                fontSize = textStyle.fontSize * 2,
                lineHeight = 0,
                tracking = textStyle.letterSpacing.value * 2
            };

            switch (textStyle.name)
            {
                case "Heading/Large":
                    headingLarge = settings;
                    break;
                case "Heading/Medium":
                    headingMedium = settings;
                    break;
                case "Heading/Small":
                    headingSmall = settings;
                    break;
                case "Body/Large":
                    bodyLarge = settings;
                    break;
                case "Body/Medium":
                    bodyMedium = settings;
                    break;
                case "Body/Small":
                    bodySmall = settings;
                    break;
                case "Body-Strong/Large":
                    bodyStrongLarge = settings;
                    break;
                case "Body-Strong/Medium":
                    bodyStrongMedium = settings;
                    break;
                case "Body-Strong/Small":
                    bodyStrongSmall = settings;
                    break;
                case "Indicator/Large":
                    indicatorLarge = settings;
                    break;
                case "Indicator/Medium":
                    indicatorMedium = settings;
                    break;
                case "Indicator/Small":
                    indicatorSmall = settings;
                    break;
                case "Indicator-Strong/Large":
                    indicatorStrongLarge = settings;
                    break;
                case "Indicator-Strong/Medium":
                    indicatorStrongMedium = settings;
                    break;
                case "Indicator-Strong/Small":
                    indicatorStrongSmall = settings;
                    break;
                case "Indicator-Numeral/Large":
                    indicatorNumeralLarge = settings;
                    break;
                case "Indicator-Numeral/Small":
                    indicatorNumeralSmall = settings;
                    break;
                    // Add additional cases as necessary
            }
        }
    }
}

[Serializable]
public class LetterSpacing
{
    public string unit;
    public float value;
}

[Serializable]
public class TextStyle
{
    public string name;
    public string fontFamily;
    public string fontWeight;
    public float fontSize;
    public LetterSpacing letterSpacing;
    public string textCase;
}

[Serializable]
public class TextStylesContainer
{
    public string fileName;
    public List<TextStyle> textStyles;
}

public enum TypographyRole
{
    HeadingLarge,
    HeadingMedium,
    HeadingSmall,
    BodyLarge,
    BodyMedium,
    BodySmall,
    BodyStrongLarge,
    BodyStrongMedium,
    BodyStrongSmall,
    IndicatorLarge,
    IndicatorMedium,
    IndicatorSmall,
    IndicatorStrongLarge,
    IndicatorStrongMedium,
    IndicatorStrongSmall,
    IndicatorNumeralLarge,
    IndicatorNumeralSmall,
    Monospace,
}

public enum TextEmphasis
{
    High,
    Medium,
    Disabled,
    Custom,
    ColorManager
}

[Serializable]
public class TypographySettings
{
    public TMP_FontAsset font;
    public float fontSize;
    public float lineHeight;
    public float tracking;
}