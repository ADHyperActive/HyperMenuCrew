using System;
using UnityEngine;
using ModMenuCrew.UI.Styles;

namespace ModMenuCrew
{
    /// <summary>
    /// A draggable, minimizable IMGUI window with scrollable content, a title bar,
    /// minimize/close buttons, and a "SHOWCASE" badge.
    /// 
    /// <b>USAGE:</b>
    /// <code>
    /// var window = new DragWindow(new Rect(24, 24, 500, 0), "My Window", DrawContent);
    /// window.Enabled = true;
    /// 
    /// // In OnGUI:
    /// if (window.Enabled) window.OnGUI();
    /// </code>
    /// </summary>
    public class DragWindow
    {
        // --- Constants ---
        private const float MinWindowWidth = 200f;
        private const float MinWindowHeight = 100f;
        private const float MaxWindowHeight = 600f;
        private const float ContentPadding = 4f;

        // --- State ---
        private Rect _windowRect;
        private readonly Action _onGuiContent;
        private bool _isDragging;
        private Vector2 _dragOffset;
        private bool _isMinimized;
        private bool _heightInitialized;
        private Vector2 _scrollPosition;
        private float _minViewportHeight = 180f;

        // --- Public Properties ---
        public bool Enabled { get; set; }
        public string Title { get; set; }

        // --- Rect cache ---
        private Rect _cachedHeaderRect;
        private Rect _cachedContentRect;
        private Rect _cachedButtonArea;

        /// <summary>
        /// Initializes a new instance of the <see cref="DragWindow"/> class.
        /// </summary>
        /// <param name="initialRect">The initial rectangle of the window.</param>
        /// <param name="title">The title of the window.</param>
        /// <param name="onGuiContent">The action to invoke when drawing the window's content.</param>
        public DragWindow(Rect initialRect, string title, Action onGuiContent)
        {
            _windowRect = new Rect(
                initialRect.x,
                initialRect.y,
                Mathf.Max(initialRect.width, MinWindowWidth),
                Mathf.Clamp(initialRect.height, MinWindowHeight, MaxWindowHeight)
            );
            Title = title;
            _onGuiContent = onGuiContent ?? (() => { });
        }

        /// <summary>
        /// Draws the window's GUI.
        /// </summary>
        public void OnGUI()
        {
            if (!Enabled) return;
            GuiStyles.EnsureInitialized();

            if (!_heightInitialized)
            {
                float defaultHeight = Mathf.Min(Screen.height * 0.5f, 360f);
                _windowRect.height = Mathf.Clamp(defaultHeight, MinWindowHeight, MaxWindowHeight);
                _heightInitialized = true;
            }

            _windowRect.width = Mathf.Max(_windowRect.width, MinWindowWidth);
            _windowRect.height = Mathf.Clamp(_windowRect.height, MinWindowHeight, MaxWindowHeight);

            GUI.Box(_windowRect, GUIContent.none, GuiStyles.WindowStyle);

            float headerHeight = GuiStyles.TitleBarButtonStyle.fixedHeight + 4;
            _cachedHeaderRect = new Rect(_windowRect.x, _windowRect.y, _windowRect.width, headerHeight);
            GUI.Box(_cachedHeaderRect, GUIContent.none, GuiStyles.HeaderBackgroundStyle);

            float buttonWidth = GuiStyles.TitleBarButtonStyle.fixedWidth;
            float buttonHeight = GuiStyles.TitleBarButtonStyle.fixedHeight;
            float buttonMargin = GuiStyles.TitleBarButtonStyle.margin.left + GuiStyles.TitleBarButtonStyle.margin.right;
            float totalButtonWidth = (2 * buttonWidth) + (2 * buttonMargin);

            _cachedButtonArea = new Rect(
                _windowRect.x + _windowRect.width - totalButtonWidth - 4,
                _windowRect.y + 2,
                totalButtonWidth,
                headerHeight - 4
            );

            GUILayout.BeginArea(_cachedButtonArea);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(_isMinimized ? "▭" : "—", GuiStyles.TitleBarButtonStyle))
            {
                _isMinimized = !_isMinimized;
            }
            if (GUILayout.Button("✕", GuiStyles.TitleBarButtonStyle))
            {
                Enabled = false;
                GUILayout.EndHorizontal();
                GUILayout.EndArea();
                return;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            var titleRect = new Rect(
                _cachedHeaderRect.x + 4,
                _cachedHeaderRect.y,
                _cachedButtonArea.x - _cachedHeaderRect.x - 80,
                _cachedHeaderRect.height
            );
            GUI.Label(titleRect, Title, GuiStyles.TitleLabelStyle);

            var badgeRect = new Rect(
                titleRect.xMax + 4,
                _cachedHeaderRect.y + 2,
                75,
                _cachedHeaderRect.height - 4
            );
            var oldBgColor = GUI.backgroundColor;
            GUI.backgroundColor = GuiStyles.Theme.ShowcaseBadge;
            GUI.Box(badgeRect, "SHOWCASE", GuiStyles.TitleBarButtonStyle);
            GUI.backgroundColor = oldBgColor;

            if (!_isMinimized)
            {
                _cachedContentRect = new Rect(
                    _windowRect.x + ContentPadding,
                    _windowRect.y + headerHeight + ContentPadding,
                    _windowRect.width - (2 * ContentPadding),
                    _windowRect.height - headerHeight - (2 * ContentPadding)
                );

                GUILayout.BeginArea(_cachedContentRect);

                float maxViewportHeight = MaxWindowHeight - headerHeight - (2 * ContentPadding);
                float currentViewportHeight = Mathf.Clamp(
                    _windowRect.height - headerHeight - (2 * ContentPadding),
                    _minViewportHeight,
                    maxViewportHeight
                );

                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true, GUILayout.Height(currentViewportHeight));
                _onGuiContent?.Invoke();
                GUILayout.EndScrollView();

                GUILayout.EndArea();
            }

            HandleDragging(_cachedHeaderRect);

            ClampToScreen();
        }

        /// <summary>
        /// Gets the rectangle of the window.
        /// </summary>
        /// <returns>The rectangle of the window.</returns>
        public Rect GetRect() => _windowRect;

        /// <summary>
        /// Sets the size of the window.
        /// </summary>
        /// <param name="width">The width of the window.</param>
        /// <param name="height">The height of the window.</param>
        public void SetSize(float width, float height)
        {
            _windowRect.width = Mathf.Max(width, MinWindowWidth);
            _windowRect.height = Mathf.Clamp(height, MinWindowHeight, MaxWindowHeight);
        }

        /// <summary>
        /// Sets the position of the window.
        /// </summary>
        /// <param name="x">The x-coordinate of the window.</param>
        /// <param name="y">The y-coordinate of the window.</param>
        public void SetPosition(float x, float y)
        {
            _windowRect.x = x;
            _windowRect.y = y;
        }

        /// <summary>
        /// Sets the minimum height of the scrollable content viewport.
        /// </summary>
        /// <param name="minHeight">The minimum height of the viewport.</param>
        public void SetViewportMinHeight(float minHeight)
        {
            _minViewportHeight = Mathf.Clamp(minHeight, 60f, 400f);
        }

        /// <summary>
        /// Handles the dragging of the window.
        /// </summary>
        /// <param name="dragArea">The area of the window that can be dragged.</param>
        private void HandleDragging(Rect dragArea)
        {
            Event e = Event.current;
            if (e == null) return;

            if (e.type == EventType.MouseDown && dragArea.Contains(e.mousePosition))
            {
                _isDragging = true;
                _dragOffset = e.mousePosition - new Vector2(_windowRect.x, _windowRect.y);
                e.Use();
            }
            else if (e.type == EventType.MouseDrag && _isDragging)
            {
                Vector2 newPos = e.mousePosition - _dragOffset;
                _windowRect.x = newPos.x;
                _windowRect.y = newPos.y;
                e.Use();
            }
            else if (e.type == EventType.MouseUp)
            {
                _isDragging = false;
            }
        }

        private void ClampToScreen()
        {
            _windowRect.x = Mathf.Clamp(_windowRect.x, 0, Screen.width - _windowRect.width);
            _windowRect.y = Mathf.Clamp(_windowRect.y, 0, Screen.height - _windowRect.height);
        }
    }
}