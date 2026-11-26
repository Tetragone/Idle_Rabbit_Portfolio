using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Game.Battle;
using Game.Common;
using Game.UI.Popup;

namespace Game.UI
{
    public class UI_Menu : MonoBehaviour
    {
        [SerializeField]
        private GameObject ObjStage;
        [SerializeField]
        private Button ButtonStage;
        [SerializeField]
        private TextMeshProUGUI TextStage;
        [SerializeField]
        private GameObject ObjShop;
        [SerializeField]
        private Button ButtonShop;
        [SerializeField]
        private TextMeshProUGUI TextShop;
        [SerializeField]
        private GameObject ObjDungeon;
        [SerializeField]
        private Button ButtonDungeon;
        [SerializeField]
        private TextMeshProUGUI TextDungeon;
        [SerializeField]
        private Button ButtonQuest;
        [SerializeField]
        private TextMeshProUGUI TextQuest;
        [SerializeField]
        private Button ButtonEvent;
        [SerializeField]
        private TextMeshProUGUI TextEvent;

        private void Awake()
        {
            InitCallback();
            InitText();
        }

        private void OnEnable()
        {
            BattleMgr.StageChanged += ChangeStage;
            BattleMgr.RoguelikeChanged += ChangeStage;

            if (BattleMgr.Instance != null && BattleMgr.Instance.IsReadied)
            {
                ChangeStage();
            }
        }

        private void Start()
        {
            ChangeStage();
        }

        private void OnDisable()
        {
            BattleMgr.StageChanged -= ChangeStage;
            BattleMgr.RoguelikeChanged -= ChangeStage;
        }

        private void InitCallback()
        {
            ButtonStage.onClick.AddListener(async () =>
            {
                UIMgr.Instance.CloseAllUis();
                var stage = await PopupMgr.Instance.Get<Popup_StageSelect>(PopupType.StageSelect);
                stage.Open();
            });

            ButtonShop.onClick.AddListener(() =>
            {
                UIMgr.Instance.CloseAllUis();
                UI_Shop shop = UIMgr.Instance.Get<UI_Shop>(MainUIType.Shop);
                shop.Open();
            });

            ButtonDungeon.onClick.AddListener(() =>
            {
                UIMgr.Instance.CloseAllUis();
                var dungeon = UIMgr.Instance.Get<UI_Dungeon>(MainUIType.Dungeon);
                dungeon.Open();
            });

            ButtonQuest.onClick.AddListener(() =>
            {
                UIMgr.Instance.CloseAllUis();
                var quest = UIMgr.Instance.Get<UI_Quest>(MainUIType.Quest);
                quest.Open();
            });

            ButtonEvent.onClick.AddListener(() =>
            {
                UIMgr.Instance.CloseAllUis();
                var @event = UIMgr.Instance.Get<UI_Event>(MainUIType.Event);
                @event.Open();
            });
        }

        private void InitText()
        {
            TextStage.text = "스테이지";
            TextShop.text = "상점";
            TextDungeon.text = "던전";
            TextQuest.text = "퀘스트";
            TextEvent.text = "이벤트";
        }

        private void ChangeStage()
        {
            bool showMenu = BattleMgr.Instance.Goal == null;
            ActiveButtons(showMenu);
            bool isRoguelike = BattleMgr.Instance.TryGetRoguelike(out var stage);
            ButtonQuest.gameObject.SetActive(!isRoguelike);
            ButtonEvent.gameObject.SetActive(!isRoguelike);

            // 보유 한도로 정산을 기다리는 경우 상점에서 재화를 사용할 수 있게 한다.
            if (isRoguelike && stage.State == Game.Battle.Stage.RoguelikeState.RewardPending)
            {
                ObjShop.SetActive(true);
            }
        }

        private void ActiveButtons(bool isActive)
        {
            ObjStage.SetActive(isActive);
            ObjShop.SetActive(isActive);
            ObjDungeon.SetActive(isActive);
        }

    }
}
