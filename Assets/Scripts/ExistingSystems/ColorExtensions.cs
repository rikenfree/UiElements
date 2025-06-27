using UnityEngine;

public static class ColorExtensions
{
    /// <summary>
    /// Returns the given color as a hex string.
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    public static string ToHex(this Color color)
    {
        return $"#{ColorUtility.ToHtmlStringRGBA(color)}";
    }

    /// <summary>
    /// Returns the given color with the given alpha value.
    /// </summary>
    /// <param name="color"></param>
    /// <param name="alpha"></param>
    /// <returns></returns>
    public static Color WithAlpha(this Color color, float alpha)
    {
        return new Color(color.r, color.g, color.b, alpha);
    }

    /// <summary>
    /// Returns the given color lerped with the surface color.
    /// </summary>
    /// <param name="color">The original color.</param>
    /// <param name="brightness">The amount of the original color to keep.</param>
    /// <returns></returns>
    public static Color LerpFromSurface(this Color color, float brightness)
    {
        ColorUtility.TryParseHtmlString("#121212", out Color surface);
        return Color.Lerp(surface, color, brightness);
    }

    /// <summary>
    /// Returns the given color lerped with white.
    /// </summary>
    /// <param name="color">The original color.</param>
    /// <param name="brightness">The amount of white to add to the original color.</param>
    /// <returns></returns>
    public static Color LerpToWhite(this Color color, float brightness)
    {
        return Color.Lerp(color, Color.white, brightness);
    }

    /// <summary>
    /// Returns the relative luminance of a color.
    /// Formula from https://www.w3.org/TR/WCAG20-TECHS/G17.html
    /// </summary>
    private static float GetRelativeLuminance(this Color color)
    {
        float[] rgb = { color.r, color.g, color.b };
        for (int i = 0; i < rgb.Length; i++)
        {
            if (rgb[i] <= 0.03928f)
            {
                rgb[i] = rgb[i] / 12.92f;
            }
            else
            {
                rgb[i] = Mathf.Pow((rgb[i] + 0.055f) / 1.055f, 2.4f);
            }
        }

        return 0.2126f * rgb[0] + 0.7152f * rgb[1] + 0.0722f * rgb[2];
    }

    /// <summary>
    /// Returns the contrast ratio between two colors.
    /// Formula from https://www.w3.org/TR/WCAG20-TECHS/G17.html
    /// </summary>
    public static float GetContrastRatio(this Color color, Color otherColor)
    {
        float l1 = color.GetRelativeLuminance() + 0.05f;
        float l2 = otherColor.GetRelativeLuminance() + 0.05f;

        return Mathf.Max(l1, l2) / Mathf.Min(l1, l2);
    }

    /// <summary>
    /// Returns true if the contrast ratio between two colors is at least 2.
    /// Ideally, the contrast should be at least 7.
    /// </summary>
    public static bool ContrastsWith(this Color color, Color otherColor)
    {
        return color.GetContrastRatio(otherColor) >= 2f;
    }

    /// <summary>
    /// Returns the color with the better contrast ratio.
    /// </summary>
    /// <param name="color"></param>
    /// <param name="option1"></param>
    /// <param name="option2"></param>
    /// <returns></returns>
    public static Color GetBetterContrast(this Color color, Color option1, Color option2)
    {
        return color.GetContrastRatio(option1) > color.GetContrastRatio(option2) ? option1 : option2;
    }
}
