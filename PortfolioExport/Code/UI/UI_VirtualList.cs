using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(100)]
    public class UI_VirtualList : MonoBehaviour
    {
        [SerializeField] private ScrollRect Scroll;
        [SerializeField] private RectTransform CellPrefab;
        [SerializeField] private Vector2 CellSize = new Vector2(200f, 195f);
        [SerializeField] private Vector2 Spacing = new Vector2(8f, 10f);
        [SerializeField] private RectOffset Padding = new RectOffset();
        [SerializeField, Min(1)] private int ColumnCount = 1;
        [SerializeField, Min(0)] private int BufferRows = 1;

        private readonly Dictionary<int, RectTransform> VisibleCells = new Dictionary<int, RectTransform>();
        private readonly Stack<RectTransform> FreeCells = new Stack<RectTransform>();
        private readonly List<int> ReleaseIndexes = new List<int>();

        private Action<RectTransform> OnCreateCell;
        private Action<RectTransform, int> OnBindCell;
        private RectTransform InactiveParent;
        private Vector2 BeforeViewportSize;
        private float BeforeContentWidth;
        private int ItemCount;
        private bool IsInitialized;
        private bool NeedsRefresh;
        private bool NeedsLayout;

        public int CreatedCellCount => VisibleCells.Count + FreeCells.Count;
        public int VisibleCellCount => VisibleCells.Count;

        public void Init(Action<RectTransform> onCreateCell, Action<RectTransform, int> onBindCell)
        {
            if (IsInitialized)
            {
                throw new InvalidOperationException("가상 리스트는 한 번만 초기화할 수 있습니다.");
            }

            if (onBindCell == null)
            {
                throw new ArgumentNullException(nameof(onBindCell));
            }

            ValidateSettings();

            OnCreateCell = onCreateCell;
            OnBindCell = onBindCell;

            // 스크롤 위치와 셀 위치는 Content의 왼쪽 위를 기준으로 계산한다.
            Scroll.content.anchorMin = new Vector2(0f, 1f);
            Scroll.content.anchorMax = new Vector2(1f, 1f);
            Scroll.content.pivot = new Vector2(0f, 1f);
            Scroll.content.sizeDelta = new Vector2(0f, Scroll.content.sizeDelta.y);

            // 활성 프리팹도 최초 데이터 연결 전에 OnEnable이 실행되지 않게 한다.
            InactiveParent = new GameObject("Inactive Cells", typeof(RectTransform)).GetComponent<RectTransform>();
            InactiveParent.gameObject.SetActive(false);
            InactiveParent.SetParent(transform, false);

            IsInitialized = true;
            NeedsLayout = true;
            NeedsRefresh = true;

            if (isActiveAndEnabled)
            {
                Scroll.onValueChanged.AddListener(OnScrollChanged);
            }
        }

        private void ValidateSettings()
        {
            if (Scroll == null || Scroll.viewport == null || Scroll.content == null || CellPrefab == null)
            {
                throw new InvalidOperationException("Scroll, Viewport, Content, Cell Prefab을 연결해야 합니다.");
            }

            if (!Scroll.vertical || Scroll.horizontal || Scroll.content.parent != Scroll.viewport)
            {
                throw new InvalidOperationException("세로 스크롤을 사용하고 Content를 Viewport 바로 아래에 배치해야 합니다.");
            }

            if (ColumnCount < 1 || BufferRows < 0 || CellSize.x <= 0f || CellSize.y <= 0f
                || Spacing.x < 0f || Spacing.y < 0f || Padding == null
                || Padding.left < 0 || Padding.right < 0 || Padding.top < 0 || Padding.bottom < 0)
            {
                throw new InvalidOperationException("셀 크기는 양수, 열 개수는 1 이상, 간격과 여백은 0 이상이어야 합니다.");
            }

            LayoutGroup layout = Scroll.content.GetComponent<LayoutGroup>();
            ContentSizeFitter fitter = Scroll.content.GetComponent<ContentSizeFitter>();

            if ((layout != null && layout.enabled) || (fitter != null && fitter.enabled))
            {
                throw new InvalidOperationException("Content의 LayoutGroup과 ContentSizeFitter를 비활성화해야 합니다.");
            }

            if (Scroll.content.childCount != 0)
            {
                throw new InvalidOperationException("가상 리스트의 Content는 비워 두어야 합니다. 예시 아이콘과 프리팹 원본을 제거하세요.");
            }
        }

        public void SetItemCount(int count, bool resetPosition = false)
        {
            RequireInit();

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            // 같은 인덱스의 데이터가 바뀌어도 이전 표시가 남지 않게 한다.
            ReleaseAllCells();
            ItemCount = count;
            Scroll.StopMovement();

            if (resetPosition)
            {
                Scroll.content.anchoredPosition = Vector2.zero;
            }

            NeedsLayout = true;
            NeedsRefresh = true;
        }

        public void RefreshItem(int index)
        {
            RequireInit();

            if (VisibleCells.TryGetValue(index, out RectTransform cell))
            {
                OnBindCell(cell, index);
            }
        }

        public void RefreshVisibleItems()
        {
            RequireInit();

            foreach (var pair in VisibleCells)
            {
                OnBindCell(pair.Value, pair.Key);
            }
        }

        // 배치 값 변경 후 호출한다. Viewport 크기 변경은 자동 감지한다.
        public void RefreshLayout()
        {
            RequireInit();
            NeedsLayout = true;
            NeedsRefresh = true;
            UpdateVisible();
        }

        private void RequireInit()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("가상 리스트의 Init을 먼저 호출해야 합니다.");
            }
        }

        private void OnEnable()
        {
            if (!IsInitialized)
            {
                return;
            }

            // 다른 UI의 Awake에서 Init이 먼저 호출된 경우에도 중복 구독하지 않는다.
            Scroll.onValueChanged.RemoveListener(OnScrollChanged);
            Scroll.onValueChanged.AddListener(OnScrollChanged);
            NeedsLayout = true;
            NeedsRefresh = true;
        }

        private void OnDisable()
        {
            if (IsInitialized)
            {
                Scroll.onValueChanged.RemoveListener(OnScrollChanged);
            }
        }

        private void OnDestroy()
        {
            if (Scroll != null)
            {
                Scroll.onValueChanged.RemoveListener(OnScrollChanged);
            }

            // 컴포넌트만 제거하더라도 생성한 셀이 남지 않게 한다.
            foreach (var pair in VisibleCells)
            {
                DestroyCell(pair.Value);
            }

            foreach (var cell in FreeCells)
            {
                DestroyCell(cell);
            }

            if (InactiveParent != null)
            {
                DestroyCell(InactiveParent);
            }
        }

        private void DestroyCell(RectTransform cell)
        {
            if (cell == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(cell.gameObject);
            }
            else
            {
                DestroyImmediate(cell.gameObject);
            }
        }

        private void OnScrollChanged(Vector2 position)
        {
            NeedsRefresh = true;
        }

        private void LateUpdate()
        {
            if (IsInitialized)
            {
                UpdateVisible();
            }
        }

        private void UpdateVisible()
        {
            Vector2 viewportSize = Scroll.viewport.rect.size;

            if (viewportSize.x <= 0f || viewportSize.y <= 0f)
            {
                return;
            }

            float contentWidth = Scroll.content.rect.width;

            if (BeforeViewportSize != viewportSize || !Mathf.Approximately(BeforeContentWidth, contentWidth))
            {
                NeedsLayout = true;
                NeedsRefresh = true;
            }

            if (!NeedsRefresh)
            {
                return;
            }

            bool updatePositions = NeedsLayout;

            if (NeedsLayout)
            {
                UpdateContentHeight(viewportSize.y);
                BeforeViewportSize = viewportSize;
                BeforeContentWidth = contentWidth;
                NeedsLayout = false;
            }

            NeedsRefresh = false;

            int rows = Mathf.CeilToInt(ItemCount / (float)ColumnCount);
            float rowStep = CellSize.y + Spacing.y;
            float maxOffset = Mathf.Max(0f, Scroll.content.rect.height - viewportSize.y);
            float offset = Mathf.Clamp(Scroll.content.anchoredPosition.y, 0f, maxOffset);
            int topRow = Mathf.Max(0, Mathf.FloorToInt((offset - Padding.top) / rowStep));
            int firstRow = Mathf.Max(0, topRow - BufferRows);
            int lastRow = Mathf.Min(rows - 1,
                Mathf.FloorToInt(Mathf.Max(0f, offset + viewportSize.y - Padding.top) / rowStep) + BufferRows);
            int firstIndex = Mathf.Min(ItemCount, firstRow * ColumnCount);
            int endIndex = Mathf.Min(ItemCount, (lastRow + 1) * ColumnCount);

            ReleaseIndexes.Clear();

            foreach (var pair in VisibleCells)
            {
                if (pair.Key < firstIndex || pair.Key >= endIndex)
                {
                    ReleaseIndexes.Add(pair.Key);
                }
            }

            foreach (int index in ReleaseIndexes)
            {
                ReleaseCell(index);
            }

            for (int index = firstIndex; index < endIndex; index++)
            {
                if (VisibleCells.TryGetValue(index, out RectTransform current))
                {
                    if (updatePositions)
                    {
                        SetCellPosition(current, index, contentWidth);
                    }

                    continue;
                }

                RectTransform cell = GetCell();
                SetCellPosition(cell, index, contentWidth);
                OnBindCell(cell, index);
                cell.gameObject.SetActive(true);
                VisibleCells.Add(index, cell);
            }
        }

        private void UpdateContentHeight(float viewportHeight)
        {
            int rows = Mathf.CeilToInt(ItemCount / (float)ColumnCount);
            float height = Padding.top + Padding.bottom
                + rows * CellSize.y + Mathf.Max(0, rows - 1) * Spacing.y;

            Scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

            Vector2 position = Scroll.content.anchoredPosition;
            position.x = 0f;
            position.y = Mathf.Clamp(position.y, 0f, Mathf.Max(0f, height - viewportHeight));
            Scroll.content.anchoredPosition = position;
        }

        private void SetCellPosition(RectTransform cell, int index, float contentWidth)
        {
            float gridWidth = ColumnCount * CellSize.x + (ColumnCount - 1) * Spacing.x;
            float startX = Padding.left + (contentWidth - Padding.left - Padding.right - gridWidth) * 0.5f;

            cell.anchorMin = new Vector2(0f, 1f);
            cell.anchorMax = new Vector2(0f, 1f);
            cell.pivot = new Vector2(0f, 1f);
            cell.sizeDelta = CellSize;
            cell.anchoredPosition = new Vector2(
                startX + index % ColumnCount * (CellSize.x + Spacing.x),
                -(Padding.top + index / ColumnCount * (CellSize.y + Spacing.y)));
        }

        private RectTransform GetCell()
        {
            if (FreeCells.Count > 0)
            {
                return FreeCells.Pop();
            }

            RectTransform cell = Instantiate(CellPrefab, InactiveParent);
            cell.gameObject.SetActive(false);
            cell.SetParent(Scroll.content, false);
            OnCreateCell?.Invoke(cell);
            return cell;
        }

        private void ReleaseCell(int index)
        {
            RectTransform cell = VisibleCells[index];
            cell.gameObject.SetActive(false);
            FreeCells.Push(cell);
            VisibleCells.Remove(index);
        }

        private void ReleaseAllCells()
        {
            foreach (var pair in VisibleCells)
            {
                pair.Value.gameObject.SetActive(false);
                FreeCells.Push(pair.Value);
            }

            VisibleCells.Clear();
        }
    }
}
