using System;
using System.Collections.Generic;
using ModMenuCrew.UI.Styles;
using UnityEngine;

namespace ModMenuCrew.UI.Controls
{
    /// <summary>
    /// A horizontal tab bar that switches between content panels.
    /// Each tab has a name, a draw callback, an optional tooltip, and an optional icon.
    /// 
    /// <b>USAGE:</b>
    /// <code>
    /// var tabs = new TabControl();
    /// tabs.AddTab("Game",     DrawGameContent,     "Game controls");
    /// tabs.AddTab("Movement", DrawMovementContent, "Movement cheats");
    /// 
    /// // In your OnGUI or draw callback:
    /// tabs.Draw();
    /// </code>
    /// 
    /// <b>HOW TO ADD A TAB AT RUNTIME:</b>
    /// <code>
    /// if (!tabs.HasTab("MyTab"))
    ///     tabs.AddTab("MyTab", MyDrawMethod, "My tooltip");
    /// </code>
    /// </summary>
    public class TabControl
    {
        private readonly List<TabItem> _tabs;
        private int _selectedTab;
        private Vector2 _mousePosition;
        private string _currentTooltip = string.Empty;

        public TabControl()
        {
            _tabs = new List<TabItem>();
            _selectedTab = 0;
        }

        /// <summary>
        /// Registers a new tab. The <paramref name="drawContent"/> callback is invoked
        /// every frame when the tab is selected.
        /// </summary>
        /// <param name="name">Label shown on the tab button.</param>
        /// <param name="drawContent">IMGUI draw callback for the tab's content.</param>
        /// <param name="tooltip">Text shown when hovering the tab button.</param>
        /// <param name="icon">Optional icon displayed before the tab name.</param>
        public void AddTab(string name, Action drawContent, string tooltip = "", Texture2D icon = null)
        {
            _tabs.Add(new TabItem(name, drawContent, tooltip, icon));
        }

        /// <summary>Renders the tab bar and the currently selected tab's content.</summary>
        public void Draw()
        {
            if (_tabs.Count == 0) return;

            GuiStyles.EnsureInitialized();
            _mousePosition = Event.current.mousePosition;
            _currentTooltip = string.Empty;

            GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            try
            {
                DrawTabHeaders();
                GUILayout.Space(2);
                DrawSelectedTabContent();
            }
            finally
            {
                GUILayout.EndVertical();
            }

            DrawTooltipOverlay();
        }

        private void DrawTabHeaders()
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            try
            {
                for (int i = 0; i < _tabs.Count; i++)
                {
                    var style = i == _selectedTab ? GuiStyles.SelectedTabStyle : GuiStyles.TabStyle;
                    if (DrawTabButton(_tabs[i], style))
                        _selectedTab = i;
                }
            }
            finally
            {
                GUILayout.EndHorizontal();
            }
        }

        private bool DrawTabButton(TabItem tab, GUIStyle style)
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            try
            {
                if (tab.Icon != null)
                    GUILayout.Box(tab.Icon, GuiStyles.IconStyle, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));

                bool clicked = GUILayout.Button(tab.Name, style, GUILayout.ExpandWidth(false));

                if (Event.current.type == EventType.Repaint)
                {
                    Rect buttonRect = GUILayoutUtility.GetLastRect();
                    if (buttonRect.Contains(_mousePosition) && !string.IsNullOrEmpty(tab.Tooltip))
                        _currentTooltip = tab.Tooltip;
                }

                return clicked;
            }
            finally
            {
                GUILayout.EndHorizontal();
            }
        }

        private void DrawSelectedTabContent()
        {
            if (_selectedTab >= 0 && _selectedTab < _tabs.Count)
                _tabs[_selectedTab].DrawContent?.Invoke();
        }

        /// <summary>Renders the tooltip overlay near the mouse cursor if a tab is hovered.</summary>
        private void DrawTooltipOverlay()
        {
            if (string.IsNullOrEmpty(_currentTooltip)) return;

            GUIContent content = new GUIContent(_currentTooltip);
            Vector2 size = GuiStyles.TooltipStyle.CalcSize(content);
            size.x += GuiStyles.TooltipStyle.padding.horizontal;
            size.y += GuiStyles.TooltipStyle.padding.vertical;

            float maxWidth = Mathf.Max(160f, Screen.width * 0.35f);
            float maxHeight = Mathf.Max(40f, Screen.height * 0.25f);
            size.x = Mathf.Clamp(size.x, 120f, maxWidth);
            size.y = Mathf.Clamp(size.y, 30f, maxHeight);

            float x = Mathf.Clamp(_mousePosition.x + 12f, 0, Screen.width - size.x);
            float y = Mathf.Clamp(_mousePosition.y - 24f, 0, Screen.height - size.y);

            GUI.Label(new Rect(x, y, size.x, size.y), content, GuiStyles.TooltipStyle);
        }

        // ═══════════════════════════════════════════════════════════════
        // PUBLIC API — Tab management
        // ═══════════════════════════════════════════════════════════════

        /// <summary>Removes all tabs and resets the selection.</summary>
        public void ClearTabs()
        {
            _tabs.Clear();
            _selectedTab = 0;
        }

        /// <summary>Returns true if a tab with the given name exists.</summary>
        public bool HasTab(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            for (int i = 0; i < _tabs.Count; i++)
                if (_tabs[i]?.Name == name) return true;
            return false;
        }

        /// <summary>Removes a tab by name. Adjusts selection index if needed.</summary>
        public void RemoveTab(string name)
        {
            if (string.IsNullOrEmpty(name)) return;
            for (int i = 0; i < _tabs.Count; i++)
            {
                if (_tabs[i]?.Name == name)
                {
                    _tabs.RemoveAt(i);
                    if (_selectedTab >= _tabs.Count)
                        _selectedTab = Mathf.Max(0, _tabs.Count - 1);
                    return;
                }
            }
        }

        /// <summary>Gets the zero-based index of the currently selected tab.</summary>
        public int GetSelectedTabIndex() => _selectedTab;

        /// <summary>Selects a tab by index (clamped to valid range).</summary>
        public void SetSelectedTab(int index)
        {
            if (index >= 0 && index < _tabs.Count)
                _selectedTab = index;
        }

        /// <summary>Moves a tab from one position to another.</summary>
        public void ReorderTabs(int fromIndex, int toIndex)
        {
            if (fromIndex < 0 || fromIndex >= _tabs.Count || toIndex < 0 || toIndex >= _tabs.Count || fromIndex == toIndex)
                return;

            var tab = _tabs[fromIndex];
            _tabs.RemoveAt(fromIndex);
            _tabs.Insert(toIndex, tab);

            if (_selectedTab == fromIndex)
                _selectedTab = toIndex;
        }
    }

    /// <summary>
    /// Represents a single tab with a name, content callback, tooltip, and optional icon.
    /// </summary>
    public class TabItem
    {
        public string Name { get; }
        public Action DrawContent { get; }
        public string Tooltip { get; }
        public Texture2D Icon { get; }

        public TabItem(string name, Action drawContent, string tooltip = "", Texture2D icon = null)
        {
            Name = name;
            DrawContent = drawContent ?? (() => { });
            Tooltip = tooltip;
            Icon = icon;
        }
    }
}