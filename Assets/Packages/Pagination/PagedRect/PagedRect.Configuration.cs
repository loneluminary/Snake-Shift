using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Pagination
{
    public partial class PagedRect
    {
        public int DefaultPage;

        [Tooltip("Optional Template for adding new pages dynamically at runtime.")]
        public Page NewPageTemplate;

        [Title("Pagination")]
        [Tooltip("If this is set to false, the page buttons will not be shown.")]
        public bool ShowPagination = true;
        [ShowIf("ShowPagination")] public bool ShowFirstAndLastButtons = true;
        [ShowIf("ShowPagination")] public bool ShowPreviousAndNextButtons = true;
        [Tooltip("If there are too many page buttons to show at once, use this field to limit the number of visible buttons. 0 == No Limit")]
        [ShowIf("ShowPagination")] public int MaximumNumberOfButtonsToShow = 15;
        [Tooltip("Set this to false to hide the button templates in edit mode")]
        [ShowIf("ShowPagination")] public bool ShowButtonTemplatesInEditor = true;
        [ShowIf("ShowPagination")] public bool ShowPageButtons = true;
        [ShowIf("ShowPagination")] public bool ShowNumbersOnButtons = true;
        [ShowIf("ShowPagination")] public bool ShowPageTitlesOnButtons = false;
        [ShowIf("ShowPagination")] public bool ShowPageIconsOnButtons = false;

        [Title("Performance")]
        [Tooltip("If this is set to true, PagedRect will check for changes to the page collection each frame. If it is set to false, PagedRect will only update its pagination/etc. when you call UpdatePagination() or UpdateDisplay(). ")]
        public bool MonitorPageCollectionForChanges = false;

        [Title("Animation")]
        [Range(0.1f, 5f)] public float AnimationSpeed = 1.0f;
        public eAnimationCurve AnimationCurve = eAnimationCurve.Linear;
        [Title("Legacy (Non-ScrollRect) Animation")]
        public eAnimationType AnimationType = eAnimationType.SlideHorizontal;
        protected eAnimationType previousAnimationTypeValue;

        [Title("Automation")]
        public bool AutoFlip = false;
        [ShowIf("AutoFlip")] public float DelayBetweenPages = 5f;
        public bool LoopEndlessly = true;
        protected float _timeSinceLastPage = 0f;

        [Title("Keyboard Input")]
        public bool UseKeyboardInput = false;
        [ShowIf("UseKeyboardInput")] public KeyCode PreviousPageKey = KeyCode.LeftArrow;
        [ShowIf("UseKeyboardInput")] public KeyCode NextPageKey = KeyCode.RightArrow;
        [ShowIf("UseKeyboardInput")] public KeyCode FirstPageKey = KeyCode.Home;
        [ShowIf("UseKeyboardInput")] public KeyCode LastPageKey = KeyCode.End;

        [Title("Dragging")]
        public bool LimitDraggingToOnePageAtATime = false;

        [Title("Legacy (Non-ScrollRect) Input")]
        public bool UseSwipeInput = true;
        [Title("ScrollRect")]
        public bool UseSwipeInputForScrollRect = true;
        public float SwipeDeltaThreshold = .1f;
        public float SpaceBetweenPages = 0f;
        public bool LoopSeamlessly = false;
        public bool ShowScrollBar = false;

        [Title("Scroll Wheel Input")]
        public bool UseScrollWheelInput = false;
        public bool OnlyUseScrollWheelInputWhenMouseIsOver = true;

        [Title("Highlight")]
        public bool HighlightOnHover = false;
        [ShowIf("HighlightOnHover")] public Color NormalColor = new(1f, 1f, 1f);
        [ShowIf("HighlightOnHover")] public Color HighlightColor = new(0.9f, 0.9f, 0.9f);
        protected bool mouseIsOverPagedRect = false;

        [Title("Events")]
        public PageChangedEventType PageChangedEvent = new();
        [Serializable] public class PageChangedEventType : UnityEngine.Events.UnityEvent<Page, Page> { }

        [Title("Page Previews")]
        public bool ShowPagePreviews = false;
        [ShowIf("ShowPagePreviews")] public float PagePreviewScale = 0.25f;
        [ShowIf("ShowPagePreviews")] public bool LockOneToOneScaleRatio = true;
        [ShowIf("ShowPagePreviews")] public bool EnablePagePreviewOverlays = true;
        [ShowIf("ShowPagePreviews")] public Sprite PagePreviewOverlayImage;
        [ShowIf("ShowPagePreviews")] public Color PagePreviewOverlayNormalColor;
        [ShowIf("ShowPagePreviews")] public Color PagePreviewOverlayHoverColor;
        [ShowIf("ShowPagePreviews")] public float PagePreviewOverlayScaleOverride = 1f;

        private Vector3 m_currentPageSize = Vector3.zero;
        private Vector3 m_otherPageSize = Vector3.zero;
        private Vector3 m_currentPageScale = Vector3.zero;
        private Vector3 m_otherPageScale = Vector3.zero;

        [Title("References")]
        public PagedRect_ScrollRect ScrollRect;
        public GameObject ScrollRectViewport;
        public GameObject Viewport;
        public GameObject Pagination;
        public PaginationButton ButtonTemplate_CurrentPage;
        public PaginationButton ButtonTemplate_OtherPages;
        public PaginationButton ButtonTemplate_DisabledPage;

        public Button Button_PreviousPage;
        public Button Button_NextPage;
        public Button Button_FirstPage;
        public Button Button_LastPage;

        public RuntimeAnimatorController AnimationControllerTemplate;
        public List<Page> Pages = new();
        public RectTransform sizingTransform;

        /// This is used to check for changes to the Page collection and avoid updating except where necessary
        /// If we do update unnecessarily, the scene gets marked as dirty without it actually needing to be
        protected List<Page> _pages = new();
        public bool IsDirty { get; set; }
    }
}
