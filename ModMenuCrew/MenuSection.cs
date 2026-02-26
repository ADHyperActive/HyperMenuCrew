using System;
using ModMenuCrew.UI.Styles;
using UnityEngine;

namespace ModMenuCrew.UI.Controls
{
    /// <summary>
    /// A collapsible section with a gradient header and expand/collapse button.
    /// Used inside tabs to group related controls under a titled section.
    /// 
    /// <b>USAGE:</b>
    /// <code>
    /// new MenuSection("My Section", () =>
    /// {
    ///     if (GUILayout.Button("Do Something", GuiStyles.ButtonStyle))
    ///         Debug.Log("Clicked!");
    /// }).Draw();
    /// </code>
    /// </summary>
    public class MenuSection
    {
        // --- State ---
        private readonly string _title;
        private readonly Action _drawContent;
        private bool _isExpanded = true;

        // --- Rect cache (avoids allocations in OnGUI) ---
        private Rect _cachedHeaderRect;
        private Rect _cachedTitleRect;
        private Rect _cachedButtonRect;

        public MenuSection(string title, Action drawContent)
        {
            _title = title;
            _drawContent = drawContent ?? (() => { });
        }

        /// <summary>Renders the section header and content (if expanded).</summary>
        public void Draw()
        {
            GuiStyles.EnsureInitialized();
            GUILayout.BeginVertical(GuiStyles.SectionStyle);

            _cachedHeaderRect = GUILayoutUtility.GetRect(0, 26, GUILayout.ExpandWidth(true));
            GUI.Box(_cachedHeaderRect, GUIContent.none, GuiStyles.HeaderBackgroundStyle);

            _cachedTitleRect = new Rect(_cachedHeaderRect.x + 8, _cachedHeaderRect.y, _cachedHeaderRect.width - 56, _cachedHeaderRect.height);
            GUI.Label(_cachedTitleRect, _title, GuiStyles.TitleLabelStyle);

            _cachedButtonRect = new Rect(_cachedHeaderRect.xMax - 28, _cachedHeaderRect.y + 5, 20, _cachedHeaderRect.height - 10);
            if (GUI.Button(_cachedButtonRect, _isExpanded ? "▾" : "▸", GuiStyles.TitleBarButtonStyle))
                _isExpanded = !_isExpanded;

            if (_isExpanded)
            {
                GUILayout.BeginVertical(GuiStyles.HighlightStyle);
                _drawContent?.Invoke();
                GUILayout.EndVertical();
            }

            GUILayout.EndVertical();
            GUILayout.Space(10);
        }
    }
}