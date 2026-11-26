using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Game.UI;

namespace Game.Tests.Editor
{
    public static class VirtualListChecks
    {
        [MenuItem("Tools/Virtual List Tests/Run Checks")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new InvalidOperationException("Play Mode를 종료한 뒤 실행하세요.");
            }

            using (var view = new TestView())
            {
                CheckCounts(view);
                CheckReuse(view);
                CheckRefresh(view);
                CheckResizeAndSubscriptions(view);
            }

            Debug.Log("가상 리스트 검사 통과: 경계 개수, 대량 목록, 셀 재사용, 상태 갱신, 크기 변경, 이벤트 구독·해제.");
        }

        private static void CheckCounts(TestView view)
        {
            foreach (int count in new[] { 0, 1, 4, 5 })
            {
                view.SetCount(count);
                Require(view.List.VisibleCellCount == count, "작은 목록에서 표시 개수가 다릅니다.");
            }

            Require(Mathf.Approximately(view.Content.rect.height, 400f), "5개 목록의 두 행 높이가 다릅니다.");
            view.SetCount(10000);
            Require(view.List.CreatedCellCount <= 24, "전체 데이터 수에 따라 셀이 생성되었습니다.");
            Require(view.IsVisible(0) && view.IsVisible(11), "첫 화면에 필요한 셀이 누락되었습니다.");

            view.ScrollTo(Mathf.Max(0f, view.Content.rect.height - view.Scroll.viewport.rect.height));
            Require(view.IsVisible(9999), "마지막 항목이 표시되지 않습니다.");
            view.SetCount(5, false);
            Require(Mathf.Approximately(view.Content.anchoredPosition.y, 0f), "개수 축소 후 위치가 보정되지 않았습니다.");

            view.SetCount(0);
            Require(view.List.VisibleCellCount == 0 && Mathf.Approximately(view.Content.rect.height, 0f),
                "빈 목록의 셀 또는 높이가 남았습니다.");
        }

        private static void CheckReuse(TestView view)
        {
            view.SetCount(10000);
            view.Selected.Add(0);
            view.List.RefreshItem(0);
            Require(view.FindCell(0).GetComponent<Image>().color == Color.green, "선택 표시가 갱신되지 않았습니다.");

            view.ScrollTo(20501f);
            Require(view.IsVisible(400) && !view.IsVisible(0), "스크롤 후 표시 범위가 바뀌지 않았습니다.");
            int created = view.List.CreatedCellCount;
            int binds = view.BindCount;

            view.ScrollTo(20502f);
            Require(view.BindCount == binds, "같은 표시 범위에서 데이터를 다시 연결했습니다.");

            for (int row = 101; row < 180; row++)
            {
                view.ScrollTo(row * 205f + 1f);
                Require(view.List.CreatedCellCount == created, "풀 확보 후 스크롤 중 셀을 추가 생성했습니다.");

                foreach (var cell in view.ActiveCells())
                {
                    Require(cell.GetComponent<Image>().color == Color.white, "이전 셀의 선택 표시가 남았습니다.");
                }
            }

            view.ScrollTo(0f);
            Require(view.FindCell(0).GetComponent<Image>().color == Color.green, "다시 표시된 항목의 선택이 복원되지 않았습니다.");
            Require(view.CreateCount == view.List.CreatedCellCount, "생성 콜백이 중복 실행되었습니다.");
        }

        private static void CheckRefresh(TestView view)
        {
            int binds = view.BindCount;
            view.List.RefreshItem(9999);
            Require(view.BindCount == binds, "화면 밖 항목을 즉시 갱신했습니다.");

            view.Selected.Clear();
            view.List.RefreshVisibleItems();
            Require(view.FindCell(0).GetComponent<Image>().color == Color.white, "전체 표시 갱신이 적용되지 않았습니다.");

            view.SetCount(10000);
            Require(view.BindCount > binds, "같은 개수의 데이터 교체를 반영하지 않았습니다.");

            bool rejected = false;

            try
            {
                view.List.SetItemCount(-1);
            }
            catch (ArgumentOutOfRangeException)
            {
                rejected = true;
            }

            Require(rejected, "음수 항목 개수를 허용했습니다.");
        }

        private static void CheckResizeAndSubscriptions(TestView view)
        {
            view.Root.sizeDelta = new Vector2(1000f, 830f);
            view.Tick();
            Require(view.IsVisible(19), "높이 변경 후 표시 행이 늘어나지 않았습니다.");
            Require(Mathf.Approximately(view.FindCell(0).anchoredPosition.x, 88f), "폭 변경 후 가운데 정렬이 적용되지 않았습니다.");

            view.SetEnabled(false);
            view.ScrollTo(20501f);
            Require(view.IsVisible(0), "비활성 상태에서 스크롤 콜백이 실행되었습니다.");
            Require(!view.IsRefreshPending, "비활성 상태에서 스크롤 이벤트 구독이 남았습니다.");

            view.SetEnabled(true);
            view.Tick();
            Require(view.IsVisible(400), "재활성화 후 스크롤 위치가 반영되지 않았습니다.");

            for (int i = 0; i < 3; i++)
            {
                view.SetEnabled(false);
                view.SetEnabled(true);
            }

            view.ScrollTo(0f);
            Require(view.IsVisible(0), "반복 활성화 후 스크롤 갱신에 실패했습니다.");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private sealed class TestView : IDisposable
        {
            private static readonly MethodInfo EnableMethod = GetListMethod("OnEnable");
            private static readonly MethodInfo DisableMethod = GetListMethod("OnDisable");
            private static readonly MethodInfo UpdateMethod = GetListMethod("LateUpdate");

            public readonly RectTransform Root;
            public readonly RectTransform Content;
            public readonly ScrollRect Scroll;
            public readonly UI_VirtualList List;
            public readonly HashSet<int> Selected = new HashSet<int>();
            public int CreateCount;
            public int BindCount;

            private readonly UnityEngine.SceneManagement.Scene TestScene;
            private readonly Dictionary<RectTransform, int> Bindings = new Dictionary<RectTransform, int>();

            // 비활성 상태에서는 LateUpdate가 없으므로 예약 상태로 구독 해제를 확인한다.
            public bool IsRefreshPending => (bool)typeof(UI_VirtualList)
                .GetField("NeedsRefresh", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .GetValue(List);

            public TestView()
            {
                TestScene = EditorSceneManager.NewPreviewScene();

                try
                {
                    Root = new GameObject("Virtual List Checks", typeof(RectTransform)).GetComponent<RectTransform>();
                    Root.gameObject.SetActive(false);
                    SceneManager.MoveGameObjectToScene(Root.gameObject, TestScene);
                    Root.sizeDelta = new Vector2(856f, 420f);

                    RectTransform viewport = CreateRect("Viewport", Root);
                    viewport.anchorMin = Vector2.zero;
                    viewport.anchorMax = Vector2.one;
                    viewport.sizeDelta = Vector2.zero;

                    Content = CreateRect("Content", viewport);
                    RectTransform prefab = CreateRect("Cell Prefab", Root);
                    prefab.gameObject.SetActive(false);
                    prefab.gameObject.AddComponent<Image>();

                    Scroll = Root.gameObject.AddComponent<ScrollRect>();
                    Scroll.horizontal = false;
                    Scroll.vertical = true;
                    Scroll.viewport = viewport;
                    Scroll.content = Content;

                    List = Root.gameObject.AddComponent<UI_VirtualList>();
                    var settings = new SerializedObject(List);
                    settings.FindProperty("Scroll").objectReferenceValue = Scroll;
                    settings.FindProperty("CellPrefab").objectReferenceValue = prefab;
                    settings.FindProperty("ColumnCount").intValue = 4;
                    settings.ApplyModifiedPropertiesWithoutUndo();

                    List.Init(cell =>
                    {
                        Require(!cell.gameObject.activeSelf, "데이터 연결 전에 셀이 활성화되었습니다.");
                        CreateCount++;
                    }, (cell, index) =>
                    {
                        BindCount++;
                        Bindings[cell] = index;
                        cell.GetComponent<Image>().color = Selected.Contains(index) ? Color.green : Color.white;
                    });

                    Root.gameObject.SetActive(true);

                    // 일반 MonoBehaviour의 수명 주기는 편집 모드에서 자동 실행되지 않는다.
                    EnableMethod.Invoke(List, null);
                }
                catch
                {
                    EditorSceneManager.ClosePreviewScene(TestScene);
                    throw;
                }
            }

            public void SetCount(int count, bool reset = true)
            {
                List.SetItemCount(count, reset);
                List.RefreshLayout();
            }

            public void ScrollTo(float offset)
            {
                Content.anchoredPosition = new Vector2(0f, offset);
                Scroll.onValueChanged.Invoke(Vector2.zero);
                Tick();
            }

            public void Tick()
            {
                if (List.enabled)
                {
                    // SendMessage는 편집 모드에서 일반 MonoBehaviour 실행을 차단한다.
                    // 관리 코드의 갱신 메서드를 직접 호출하여 예약된 변경을 처리한다.
                    UpdateMethod.Invoke(List, null);
                }
            }

            public void SetEnabled(bool enabled)
            {
                List.enabled = enabled;

                // 편집 모드 검사에서는 구독과 해제도 명시적으로 실행한다.
                MethodInfo method = enabled ? EnableMethod : DisableMethod;
                method.Invoke(List, null);
            }

            private static MethodInfo GetListMethod(string name)
            {
                MethodInfo method = typeof(UI_VirtualList).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);

                if (method == null)
                {
                    throw new InvalidOperationException($"가상 리스트 검사에 필요한 메서드가 없습니다: {name}");
                }

                return method;
            }

            public bool IsVisible(int index) => FindCell(index) != null;

            public RectTransform FindCell(int index)
            {
                foreach (var pair in Bindings)
                {
                    if (pair.Key.gameObject.activeSelf && pair.Value == index)
                    {
                        return pair.Key;
                    }
                }

                return null;
            }

            public IEnumerable<RectTransform> ActiveCells()
            {
                foreach (var pair in Bindings)
                {
                    if (pair.Key.gameObject.activeSelf)
                    {
                        yield return pair.Key;
                    }
                }
            }

            private static RectTransform CreateRect(string name, Transform parent)
            {
                RectTransform rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
                rect.SetParent(parent, false);
                return rect;
            }

            public void Dispose()
            {
                DisableMethod.Invoke(List, null);
                EditorSceneManager.ClosePreviewScene(TestScene);
            }
        }
    }
}
