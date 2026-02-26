using System;
using UnityEngine;

namespace ModMenuCrew.UI.Styles;

public static class GuiStyles
{
    /// <summary>
    /// Color definitions for the mod's visual theme.
    /// </summary>
    public static class Theme
    {
        // === SHOWCASE VERSION BADGE ===
        public static readonly Color ShowcaseBadge = new Color(1f, 0.5f, 0f, 1f); // Orange badge
        public static readonly Color ShowcaseGlow = new Color(1f, 0.6f, 0.1f, 0.3f); // Orange glow
        // Background Colors
        public static readonly Color BgDarkA = new Color(0.07f, 0.07f, 0.07f, 0.92f); // Main background darker
        public static readonly Color BgDarkB = new Color(0.06f, 0.06f, 0.06f, 0.92f); // Main background lighter
        public static readonly Color BgSection = new Color(0.05f, 0.05f, 0.06f, 0.85f); // Background for sections

        // Header Colors
        public static readonly Color HeaderTop = new Color(0.10f, 0.02f, 0.04f, 0.95f); // Gradient top of header
        public static readonly Color HeaderBottom = new Color(0.06f, 0.01f, 0.03f, 0.95f); // Gradient bottom of header

        // Accent Colors
        public static readonly Color Accent = new Color(1f, 0.5f, 0f, 1f); // Orange accent
        public static readonly Color AccentSoft = new Color(1f, 0.6f, 0.2f, 1f); // Soft orange accent
        public static readonly Color AccentDim = new Color(0.7f, 0.35f, 0f, 1f); // Dark orange accent
        public static readonly Color AccentHover = new Color(1f, 0.55f, 0.1f, 1f); // Orange hover accent
        public static readonly Color AccentActive = new Color(0.85f, 0.42f, 0f, 1f); // Active orange accent

        // Button Colors
        public static readonly Color ButtonTop = new Color(0.11f, 0.11f, 0.12f, 0.95f); // Gradient top of button
        public static readonly Color ButtonBottom = new Color(0.08f, 0.08f, 0.10f, 0.95f); // Gradient bottom of button
        public static readonly Color ButtonHoverTop = new Color(0.13f, 0.13f, 0.16f, 0.95f); // Gradient top of button on hover
        public static readonly Color ButtonHoverBottom = new Color(0.10f, 0.10f, 0.13f, 0.95f); // Gradient bottom of button on hover
        public static readonly Color ButtonActiveTop = new Color(0.12f, 0.02f, 0.05f, 0.95f); // Gradient top of active button
        public static readonly Color ButtonActiveBottom = new Color(0.09f, 0.02f, 0.04f, 0.95f); // Gradient bottom of active button

        // Text Colors
        public static readonly Color TextPrimary = new Color(0.96f, 0.96f, 0.98f, 1f); // Primary text color
        public static readonly Color TextMuted = new Color(0.78f, 0.78f, 0.82f, 1f); // Secondary text color
        public static readonly Color TextDisabled = new Color(0.5f, 0.5f, 0.55f, 1f); // Text color for disabled items

        // State & Feedback Colors
        public static readonly Color Error = new Color(1f, 0.15f, 0.15f, 1f); // Error color
        public static readonly Color Success = new Color(0.2f, 0.8f, 0.4f, 1f); // Success color
        public static readonly Color Warning = new Color(0.9f, 0.7f, 0.2f, 1f); // Warning color
    }

    #region Texture Helpers (Optimized)
    // Textures are generated once and reused, or created on demand and cached.
    // Using HideFlags.HideAndDontSave to prevent them from appearing in Unity's Hierarchy/Inspector.
    private static Texture2D _cachedPixelDarkTexture;
    private static Texture2D _cachedPixelAccentTexture;
    private static Texture2D _cachedPixelErrorTexture;

    /// <summary>Creates a vertical gradient texture.</summary>
    private static Texture2D MakeVerticalGradientTexture(int width, int height, Color top, Color bottom)
    {
        if (width < 1) width = 1;
        if (height < 2) height = 2;
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        texture.hideFlags = HideFlags.HideAndDontSave;

        Color[] pixels = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            float t = (float)y / (height - 1);
            Color rowColor = Color.Lerp(top, bottom, t);
            for (int x = 0; x < width; x++)
            {
                pixels[y * width + x] = rowColor;
            }
        }
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    /// <summary>Creates a bordered frame texture with gradient fill.</summary>
    private static Texture2D MakeFrameTexture(int width, int height, Color innerTop, Color innerBottom, Color border, int borderThickness)
    {
        if (width < borderThickness * 2 + 1) width = borderThickness * 2 + 1;
        if (height < borderThickness * 2 + 2) height = borderThickness * 2 + 2;
        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        tex.hideFlags = HideFlags.HideAndDontSave;

        Color[] pixels = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            float t = (float)y / (height - 1);
            Color inner = Color.Lerp(innerTop, innerBottom, t);
            for (int x = 0; x < width; x++)
            {
                bool isBorder = x < borderThickness || x >= width - borderThickness || y < borderThickness || y >= height - borderThickness;
                pixels[y * width + x] = isBorder ? border : inner;
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    /// <summary>Creates a solid color texture of the specified size.</summary>
    private static Texture2D MakeTexture(int width, int height, Color color)
    {
        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        tex.hideFlags = HideFlags.HideAndDontSave;

        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    /// <summary>Creates a 1x1 solid color texture. Caches common colors for reuse.</summary>
    private static Texture2D MakeTexture(Color color)
    {
        // Cache common textures to avoid recreation
        if (color == Theme.BgDarkB) { if (_cachedPixelDarkTexture == null) _cachedPixelDarkTexture = MakeTexture(1, 1, color); return _cachedPixelDarkTexture; }
        if (color == Theme.Accent) { if (_cachedPixelAccentTexture == null) _cachedPixelAccentTexture = MakeTexture(1, 1, color); return _cachedPixelAccentTexture; }
        if (color == Theme.Error) { if (_cachedPixelErrorTexture == null) _cachedPixelErrorTexture = MakeTexture(1, 1, color); return _cachedPixelErrorTexture; }

        var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, color);
        texture.filterMode = FilterMode.Bilinear;
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.Apply();
        return texture;
    }

    /// <summary>Creates a reusable RectOffset.</summary>
    private static RectOffset CreateRectOffset(int left, int right, int top, int bottom)
    {
        var offset = new RectOffset();
        offset.left = left;
        offset.right = right;
        offset.top = top;
        offset.bottom = bottom;
        return offset;
    }
    #endregion

    #region Private Style Fields (Lazy-Initialized)
    // Styles are initialized on first access to avoid startup overhead.
    private static GUIStyle _headerStyle;
    private static GUIStyle _subHeaderStyle;
    private static GUIStyle _buttonStyle;
    private static GUIStyle _toggleStyle;
    private static GUIStyle _sliderStyle;
    private static GUIStyle _labelStyle;
    private static GUIStyle _tabStyle;
    private static GUIStyle _selectedTabStyle;
    private static GUIStyle _containerStyle;
    private static GUIStyle _sectionStyle;
    private static GUIStyle _errorStyle;
    private static GUIStyle _iconStyle;
    private static GUIStyle _tooltipStyle;
    private static GUIStyle _statusIndicatorStyle;
    private static GUIStyle _glowStyle;
    private static GUIStyle _shadowStyle;
    private static GUIStyle _highlightStyle;
    private static GUIStyle _separatorStyle;
    private static GUIStyle _betterToggleStyle;
    private static GUIStyle _windowStyle;
    private static GUIStyle _headerBackgroundStyle;
    private static GUIStyle _titleLabelStyle;
    private static GUIStyle _titleBarButtonStyle;
    private static GUIStyle _textFieldStyle;
    #endregion

    #region Public Style Properties (Lazy-Initialized)
    public static GUIStyle HeaderStyle
    {
        get
        {
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 18,                     fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft,
                    normal = { textColor = Theme.Accent },                     padding = CreateRectOffset(12, 12, 10, 10),                     margin = CreateRectOffset(8, 8, 4, 8)                 };
                _headerStyle.richText = true;
            }
            return _headerStyle;
        }
    }

    public static GUIStyle SubHeaderStyle
    {
        get
        {
            if (_subHeaderStyle == null)
            {
                _subHeaderStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft,
                    normal = { textColor = Theme.TextMuted },
                    padding = CreateRectOffset(10, 10, 7, 7),                     margin = CreateRectOffset(8, 8, 2, 6)                 };
                _subHeaderStyle.richText = true;
            }
            return _subHeaderStyle;
        }
    }

    public static GUIStyle ButtonStyle
    {
        get
        {
            if (_buttonStyle == null)
            {
                _buttonStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 14,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Theme.TextPrimary },
                    padding = CreateRectOffset(14, 14, 8, 8),
                    margin = CreateRectOffset(6, 6, 3, 3),
                    fixedHeight = 36
                };
                _buttonStyle.normal.background = MakeFrameTexture(16, 64, Theme.ButtonTop, Theme.ButtonBottom, Theme.AccentDim, 1);
                _buttonStyle.hover.background = MakeFrameTexture(16, 64, Theme.ButtonHoverTop, Theme.ButtonHoverBottom, Theme.AccentHover, 1);
                _buttonStyle.active.background = MakeFrameTexture(16, 64, Theme.ButtonActiveTop, Theme.ButtonActiveBottom, Theme.AccentActive, 1);
                _buttonStyle.focused.background = _buttonStyle.hover.background;                 _buttonStyle.richText = true;
            }
            else if (_buttonStyle.normal?.background == null || _buttonStyle.hover?.background == null || _buttonStyle.active?.background == null)
            {
                _buttonStyle.normal.background = MakeFrameTexture(16, 64, Theme.ButtonTop, Theme.ButtonBottom, Theme.AccentDim, 1);
                _buttonStyle.hover.background = MakeFrameTexture(16, 64, Theme.ButtonHoverTop, Theme.ButtonHoverBottom, Theme.AccentHover, 1);
                _buttonStyle.active.background = MakeFrameTexture(16, 64, Theme.ButtonActiveTop, Theme.ButtonActiveBottom, Theme.AccentActive, 1);
                _buttonStyle.focused.background = _buttonStyle.hover.background;
            }
            return _buttonStyle;
        }
    }

    public static GUIStyle ToggleStyle
    {
        get
        {
            if (_toggleStyle == null)
            {
                _toggleStyle = new GUIStyle(GUI.skin.toggle)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Normal,
                    alignment = TextAnchor.MiddleLeft,
                    normal = { textColor = Theme.TextMuted, background = MakeTexture(new Color(0.09f, 0.09f, 0.11f, 0.95f)) },
                    onNormal = { textColor = Theme.TextPrimary, background = MakeTexture(new Color(0.18f, 0.09f, 0.02f, 0.95f)) },
                    hover = { textColor = Theme.TextPrimary, background = MakeTexture(new Color(0.11f, 0.11f, 0.14f, 0.95f)) },
                    onHover = { textColor = Theme.TextPrimary, background = MakeTexture(new Color(0.22f, 0.11f, 0.03f, 0.95f)) },
                    active = { textColor = Theme.TextMuted, background = MakeTexture(new Color(0.09f, 0.09f, 0.11f, 0.95f)) },
                    onActive = { textColor = Theme.TextPrimary, background = MakeTexture(new Color(0.25f, 0.13f, 0.04f, 0.95f)) },
                    padding = CreateRectOffset(16, 16, 9, 9),
                    margin = CreateRectOffset(6, 6, 4, 4),
                    fixedHeight = 34,
                    stretchWidth = true
                };
                _toggleStyle.richText = true;
            }
            else if (_toggleStyle.normal?.background == null || _toggleStyle.onNormal?.background == null || _toggleStyle.hover?.background == null || _toggleStyle.onHover?.background == null || _toggleStyle.active?.background == null || _toggleStyle.onActive?.background == null)
            {
                _toggleStyle.normal.background = MakeTexture(new Color(0.09f, 0.09f, 0.11f, 0.95f));
                _toggleStyle.onNormal.background = MakeTexture(new Color(0.12f, 0.03f, 0.06f, 0.95f));
                _toggleStyle.hover.background = MakeTexture(new Color(0.11f, 0.11f, 0.14f, 0.95f));
                _toggleStyle.onHover.background = MakeTexture(new Color(0.14f, 0.04f, 0.08f, 0.95f));
                _toggleStyle.active.background = MakeTexture(new Color(0.09f, 0.09f, 0.11f, 0.95f));
                _toggleStyle.onActive.background = MakeTexture(new Color(0.15f, 0.05f, 0.10f, 0.95f));
            }
            return _toggleStyle;
        }
    }

    public static GUIStyle ContainerStyle
    {
        get
        {
            if (_containerStyle == null)
            {
                _containerStyle = new GUIStyle(GUI.skin.box)
                {
                    padding = CreateRectOffset(8, 8, 8, 8),
                    margin = CreateRectOffset(4, 4, 4, 4)
                };
                _containerStyle.normal.background = MakeTexture(2, 2, new Color(0.07f, 0.07f, 0.09f, 0.80f));
            }
            else if (_containerStyle.normal?.background == null)
            {
                _containerStyle.normal.background = MakeTexture(2, 2, new Color(0.07f, 0.07f, 0.09f, 0.80f));
            }
            return _containerStyle;
        }
    }

    public static GUIStyle IconStyle
    {
        get
        {
            if (_iconStyle == null)
            {
                _iconStyle = new GUIStyle(GUI.skin.box)
                {
                    padding = CreateRectOffset(3, 3, 3, 3),
                    margin = CreateRectOffset(3, 3, 3, 3),
                    fixedWidth = 28,
                    fixedHeight = 28
                };
                _iconStyle.normal.background = MakeFrameTexture(8, 8, new Color(0.11f, 0.11f, 0.13f, 0.95f), new Color(0.09f, 0.09f, 0.11f, 0.95f), Theme.AccentDim, 1);
            }
            else if (_iconStyle.normal?.background == null)
            {
                _iconStyle.normal.background = MakeFrameTexture(8, 8, new Color(0.11f, 0.11f, 0.13f, 0.95f), new Color(0.09f, 0.09f, 0.11f, 0.95f), Theme.AccentDim, 1);
            }
            return _iconStyle;
        }
    }

    public static GUIStyle SeparatorStyle
    {
        get
        {
            if (_separatorStyle == null)
            {
                _separatorStyle = new GUIStyle(GUI.skin.box)
                {
                    normal = { background = MakeTexture(1, 1, Theme.Accent) },
                    margin = CreateRectOffset(8, 8, 6, 6),
                    fixedHeight = 2
                };
            }
            return _separatorStyle;
        }
    }

    public static GUIStyle BetterToggleStyle
    {
        get
        {
            if (_betterToggleStyle == null)
            {
                _betterToggleStyle = new GUIStyle(GUI.skin.toggle)
                {
                    fontSize = 15,
                    fontStyle = FontStyle.Normal,
                    alignment = TextAnchor.MiddleLeft,
                    padding = CreateRectOffset(18, 18, 11, 11),
                    margin = CreateRectOffset(10, 10, 8, 8),
                    fixedHeight = 40,
                    stretchWidth = true
                };
                _betterToggleStyle.normal.background = MakeFrameTexture(16, 64, new Color(0.10f, 0.10f, 0.12f, 0.95f), new Color(0.08f, 0.08f, 0.10f, 0.95f), Theme.AccentDim, 1);
                _betterToggleStyle.onNormal.background = MakeFrameTexture(16, 64, new Color(0.15f, 0.03f, 0.07f, 0.95f), new Color(0.12f, 0.02f, 0.05f, 0.95f), Theme.Accent, 1);
                _betterToggleStyle.normal.textColor = Theme.TextMuted;
                _betterToggleStyle.onNormal.textColor = Theme.TextPrimary;
                _betterToggleStyle.hover.background = MakeFrameTexture(16, 64, new Color(0.12f, 0.12f, 0.15f, 0.95f), new Color(0.10f, 0.10f, 0.13f, 0.95f), Theme.AccentHover, 1);
                _betterToggleStyle.onHover.background = MakeFrameTexture(16, 64, new Color(0.17f, 0.04f, 0.09f, 0.95f), new Color(0.14f, 0.03f, 0.07f, 0.95f), Theme.AccentHover, 1);
                _betterToggleStyle.active.background = MakeFrameTexture(16, 64, new Color(0.11f, 0.11f, 0.13f, 0.95f), new Color(0.09f, 0.09f, 0.11f, 0.95f), Theme.AccentActive, 1);
                _betterToggleStyle.onActive.background = MakeFrameTexture(16, 64, new Color(0.19f, 0.05f, 0.11f, 0.95f), new Color(0.16f, 0.04f, 0.09f, 0.95f), Theme.AccentActive, 1);
                _betterToggleStyle.active.textColor = Theme.TextMuted;
                _betterToggleStyle.onActive.textColor = Theme.TextPrimary;
            }
            return _betterToggleStyle;
        }
    }

    public static GUIStyle WindowStyle
    {
        get
        {
            if (_windowStyle == null)
            {
                _windowStyle = new GUIStyle(GUI.skin.box)
                {
                    padding = CreateRectOffset(10, 10, 10, 10),
                    margin = CreateRectOffset(2, 2, 2, 2)
                };
                var top = new Color(Theme.BgDarkA.r, Theme.BgDarkA.g, Theme.BgDarkA.b, 0.80f);
                var bottom = new Color(Theme.BgDarkB.r, Theme.BgDarkB.g, Theme.BgDarkB.b, 0.80f);
                _windowStyle.normal.background = MakeVerticalGradientTexture(2, 128, top, bottom);
            }
            else if (_windowStyle.normal?.background == null)
            {
                var top = new Color(Theme.BgDarkA.r, Theme.BgDarkA.g, Theme.BgDarkA.b, 0.80f);
                var bottom = new Color(Theme.BgDarkB.r, Theme.BgDarkB.g, Theme.BgDarkB.b, 0.80f);
                _windowStyle.normal.background = MakeVerticalGradientTexture(2, 128, top, bottom);
            }
            return _windowStyle;
        }
    }

    public static GUIStyle HeaderBackgroundStyle
    {
        get
        {
            if (_headerBackgroundStyle == null)
            {
                _headerBackgroundStyle = new GUIStyle(GUI.skin.box)
                {
                    padding = CreateRectOffset(0, 0, 0, 0),
                    margin = CreateRectOffset(0, 0, 0, 0)
                };
                var hTop = new Color(Theme.HeaderTop.r, Theme.HeaderTop.g, Theme.HeaderTop.b, 0.80f);
                var hBottom = new Color(Theme.HeaderBottom.r, Theme.HeaderBottom.g, Theme.HeaderBottom.b, 0.80f);
                _headerBackgroundStyle.normal.background = MakeVerticalGradientTexture(2, 32, hTop, hBottom);
            }
            else if (_headerBackgroundStyle.normal?.background == null)
            {
                var hTop = new Color(Theme.HeaderTop.r, Theme.HeaderTop.g, Theme.HeaderTop.b, 0.80f);
                var hBottom = new Color(Theme.HeaderBottom.r, Theme.HeaderBottom.g, Theme.HeaderBottom.b, 0.80f);
                _headerBackgroundStyle.normal.background = MakeVerticalGradientTexture(2, 32, hTop, hBottom);
            }
            return _headerBackgroundStyle;
        }
    }

    public static GUIStyle TitleLabelStyle
    {
        get
        {
            if (_titleLabelStyle == null)
            {
                _titleLabelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 15,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Theme.TextPrimary }
                };
            }
            return _titleLabelStyle;
        }
    }

    public static GUIStyle TitleBarButtonStyle
    {
        get
        {
            if (_titleBarButtonStyle == null)
            {
                _titleBarButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 13,
                    alignment = TextAnchor.MiddleCenter,
                    padding = CreateRectOffset(2, 2, 2, 2),
                    margin = CreateRectOffset(4, 4, 4, 4),
                    fixedWidth = 24,
                    fixedHeight = 20
                };
                _titleBarButtonStyle.normal.textColor = Theme.TextPrimary;
                _titleBarButtonStyle.normal.background = MakeFrameTexture(8, 32, new Color(0.13f, 0.13f, 0.15f, 0.95f), new Color(0.11f, 0.11f, 0.13f, 0.95f), Theme.AccentDim, 1);
                _titleBarButtonStyle.hover.background = MakeFrameTexture(8, 32, new Color(0.15f, 0.05f, 0.10f, 0.95f), new Color(0.13f, 0.04f, 0.08f, 0.95f), Theme.AccentHover, 1);
                _titleBarButtonStyle.active.background = MakeFrameTexture(8, 32, new Color(0.17f, 0.06f, 0.12f, 0.95f), new Color(0.15f, 0.05f, 0.10f, 0.95f), Theme.AccentActive, 1);
            }
            else if (_titleBarButtonStyle.normal?.background == null || _titleBarButtonStyle.hover?.background == null || _titleBarButtonStyle.active?.background == null)
            {
                _titleBarButtonStyle.normal.background = MakeFrameTexture(8, 32, new Color(0.13f, 0.13f, 0.15f, 0.95f), new Color(0.11f, 0.11f, 0.13f, 0.95f), Theme.AccentDim, 1);
                _titleBarButtonStyle.hover.background = MakeFrameTexture(8, 32, new Color(0.15f, 0.05f, 0.10f, 0.95f), new Color(0.13f, 0.04f, 0.08f, 0.95f), Theme.AccentHover, 1);
                _titleBarButtonStyle.active.background = MakeFrameTexture(8, 32, new Color(0.17f, 0.06f, 0.12f, 0.95f), new Color(0.15f, 0.05f, 0.10f, 0.95f), Theme.AccentActive, 1);
            }
            return _titleBarButtonStyle;
        }
    }

    public static GUIStyle TextFieldStyle
    {
        get
        {
            if (_textFieldStyle == null)
            {
                _textFieldStyle = new GUIStyle(GUI.skin.textField)
                {
                    fontSize = 14,
                    alignment = TextAnchor.MiddleLeft,
                    padding = CreateRectOffset(10, 10, 8, 8),                     margin = CreateRectOffset(6, 6, 6, 8)                 };
                _textFieldStyle.normal.textColor = Theme.TextPrimary;
                _textFieldStyle.normal.background = MakeFrameTexture(16, 48, new Color(0.08f, 0.08f, 0.10f, 0.95f), new Color(0.06f, 0.06f, 0.08f, 0.95f), Theme.AccentDim, 1);
                _textFieldStyle.hover.background = MakeFrameTexture(16, 48, new Color(0.10f, 0.10f, 0.12f, 0.95f), new Color(0.08f, 0.08f, 0.10f, 0.95f), Theme.AccentHover, 1);
                _textFieldStyle.focused.background = MakeFrameTexture(16, 48, new Color(0.11f, 0.02f, 0.05f, 0.95f), new Color(0.09f, 0.01f, 0.04f, 0.95f), Theme.Accent, 1);
                _textFieldStyle.richText = true;
            }
            else if (_textFieldStyle.normal?.background == null || _textFieldStyle.hover?.background == null || _textFieldStyle.focused?.background == null)
            {
                _textFieldStyle.normal.background = MakeFrameTexture(16, 48, new Color(0.08f, 0.08f, 0.10f, 0.95f), new Color(0.06f, 0.06f, 0.08f, 0.95f), Theme.AccentDim, 1);
                _textFieldStyle.hover.background = MakeFrameTexture(16, 48, new Color(0.10f, 0.10f, 0.12f, 0.95f), new Color(0.08f, 0.08f, 0.10f, 0.95f), Theme.AccentHover, 1);
                _textFieldStyle.focused.background = MakeFrameTexture(16, 48, new Color(0.11f, 0.02f, 0.05f, 0.95f), new Color(0.09f, 0.01f, 0.04f, 0.95f), Theme.Accent, 1);
            }
            return _textFieldStyle;
        }
    }
    #endregion

    #region Public Utility Functions

    /// <summary>
    /// Forces all lazy-initialized styles to be created.
    /// Call this once early (e.g., in DragWindow.OnGUI or TabControl.Draw) to avoid
    /// first-frame lag when styles are accessed for the first time.
    /// </summary>
    public static void EnsureInitialized()
    {
        _ = WindowStyle;
        _ = HeaderBackgroundStyle;
        _ = TitleLabelStyle;
        _ = TitleBarButtonStyle;
        _ = HeaderStyle;
        _ = SubHeaderStyle;
        _ = ButtonStyle;
        _ = ToggleStyle;
        _ = SliderStyle;
        _ = LabelStyle;
        _ = TabStyle;
        _ = SelectedTabStyle;
        _ = ContainerStyle;
        _ = SectionStyle;
        _ = ErrorStyle;
        _ = IconStyle;
        _ = TooltipStyle;
        _ = StatusIndicatorStyle;
        _ = GlowStyle;
        _ = ShadowStyle;
        _ = HighlightStyle;
        _ = SeparatorStyle;
        _ = BetterToggleStyle;
        _ = TextFieldStyle;
    }

    /// <summary>
    /// Draws a tooltip at the mouse position if the given rect is hovered.
    /// 
    /// <b>USAGE:</b>
    /// <code>
    /// if (GUILayout.Button("My Button", GuiStyles.ButtonStyle)) { ... }
    /// GuiStyles.DrawTooltip("What this button does", GUILayoutUtility.GetLastRect());
    /// </code>
    /// </summary>
    public static void DrawTooltip(string tooltip, Rect rect)
    {
        if (!string.IsNullOrEmpty(tooltip) && rect.Contains(Event.current.mousePosition))
        {
            Vector2 size = GUI.skin.box.CalcSize(new GUIContent(tooltip));
            Rect tooltipRect = new Rect(Event.current.mousePosition.x + 15, Event.current.mousePosition.y, size.x + 16, size.y + 12);
            GUI.Label(tooltipRect, tooltip, TooltipStyle);
        }
    }

    /// <summary>
    /// Draws a small colored circle indicator (green = active, red = inactive).
    /// Place it next to a toggle to give visual feedback.
    /// 
    /// <b>USAGE:</b>
    /// <code>
    /// GUILayout.BeginHorizontal();
    /// myToggle = GuiStyles.DrawBetterToggle(myToggle, "My Feature", "Tooltip text");
    /// GuiStyles.DrawStatusIndicator(myToggle);
    /// GUILayout.EndHorizontal();
    /// </code>
    /// </summary>
    public static void DrawStatusIndicator(bool isActive)
    {
        Color color = isActive ? Theme.Success : Theme.Error;
        GUIStyle style = new GUIStyle(StatusIndicatorStyle);
        style.normal.background = MakeTexture(1, 1, color);
        GUILayout.Box(GUIContent.none, style);
    }

    /// <summary>
    /// Draws a horizontal accent-colored separator line.
    /// Use between sections or groups of controls for visual separation.
    /// </summary>
    public static void DrawSeparator()
    {
        GUILayout.Box(GUIContent.none, SeparatorStyle, GUILayout.ExpandWidth(true), GUILayout.Height(2));
    }

    /// <summary>
    /// Draws a styled tab button. Returns true if the tab is selected.
    /// Prefer using <see cref="TabControl"/> instead of calling this directly.
    /// </summary>
    public static bool DrawTab(string label, bool selected)
    {
        return GUILayout.Toggle(selected, label, selected ? SelectedTabStyle : TabStyle);
    }

    /// <summary>
    /// Draws a large styled toggle with an optional hover tooltip.
    /// This is the recommended way to create toggle controls in the menu.
    /// 
    /// <b>USAGE:</b>
    /// <code>
    /// myFeature = GuiStyles.DrawBetterToggle(myFeature, "My Feature", "Description of what this does");
    /// </code>
    /// </summary>
    public static bool DrawBetterToggle(bool value, string label, string tooltip = null)
    {
        bool result = GUILayout.Toggle(value, label, BetterToggleStyle);
        Rect lastRect = GUILayoutUtility.GetLastRect();
        if (!string.IsNullOrEmpty(tooltip))
            DrawTooltip(tooltip, lastRect);
        return result;
    }

    /// <summary>Appends the current time (HH:mm:ss) to a header text string.</summary>
    public static string GetHeaderText(string text)
    {
        return $"{text} - {DateTime.Now:HH:mm:ss}";
    }

    /// <summary>
    /// Returns a copy of HeaderStyle with an animated accent color (ping-pong lerp).
    /// Useful for drawing attention to important headers.
    /// </summary>
    public static GUIStyle GetAnimatedHeaderStyle()
    {
        var style = new GUIStyle(HeaderStyle);
        style.normal.textColor = Color.Lerp(Theme.Accent, Theme.AccentSoft, Mathf.PingPong(Time.time * 2f, 1f));
        return style;
    }

    /// <summary>Returns the current time formatted as HH:mm:ss.</summary>
    public static string GetCurrentTime()
    {
        return DateTime.Now.ToString("HH:mm:ss");
    }
    #endregion
}