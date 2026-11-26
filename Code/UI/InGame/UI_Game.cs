using System.Collections.Generic;
using System.Text;

using TMPro;
using UnityEngine;

using Game.Battle;
using Game.Battle.Stage;
using Game.Common;
using Game.Data;
using Game.UI.Popup;

namespace Game.UI
{
    public class UI_Game : MonoBehaviour
    {
        [SerializeField] private UI_Gauge GaugePercent;
        [SerializeField] private GameObject ObjTimer;
        [SerializeField] private TextMeshProUGUI TextTimer;
        [SerializeField] private GameObject ObjWave;
        [SerializeField] private TextMeshProUGUI TextWave;
        [SerializeField] private GameObject ObjEffects;
        [SerializeField] private TextMeshProUGUI TextEffects;

        private bool IsOpeningSelect;
        private int OpenVersion;
        private Popup_RoguelikeSelect SelectPopup;

        private void OnEnable()
        {
            BattleMgr.StageChanged += Refresh;
            BattleMgr.RoguelikeChanged += Refresh;

            if (BattleMgr.Instance != null && BattleMgr.Instance.IsReadied)
            {
                Refresh();
            }
        }

        private void Start()
        {
            Refresh();
        }

        private void OnDisable()
        {
            BattleMgr.StageChanged -= Refresh;
            BattleMgr.RoguelikeChanged -= Refresh;
            OpenVersion++;

            if (SelectPopup != null)
            {
                SelectPopup.Close();
                SelectPopup = null;
            }
        }

        private void Refresh()
        {
            bool hasGoal = BattleMgr.Instance.StageProgress != null;
            GaugePercent.gameObject.SetActive(hasGoal);
            ObjTimer.SetActive(hasGoal && BattleMgr.Instance.StageTimer.MaxTime() > 0);

            bool isRoguelike = BattleMgr.Instance.TryGetRoguelike(out var stage);

            if (ObjWave != null)
            {
                ObjWave.gameObject.SetActive(isRoguelike);

                if (isRoguelike)
                {
                    string goal = stage.CurrentWave.ClearType == WaveClearType.KillAll ? "전멸" : "버티기";
                    string enemy = stage.CurrentWave.EnemyType == WaveEnemyType.Boss ? "보스전" : "일반전";
                    TextWave.text = stage.State == RoguelikeState.RewardPending
                        ? "보상 보유 한도 초과: 상점에서 재화를 사용해주세요."
                        : $"웨이브 {stage.WaveNumber} / {stage.WaveCount}\n{goal}  {enemy}";
                }
            }

            if (ObjEffects != null)
            {
                ObjEffects.gameObject.SetActive(isRoguelike);

                if (isRoguelike)
                {
                    RefreshEffects(stage.EffectIds);
                }
            }

            if (isRoguelike && stage.State == RoguelikeState.Selecting && !IsOpeningSelect &&
                (SelectPopup == null || !SelectPopup.gameObject.activeSelf))
            {
                OpenSelect(stage);
            }
        }

        private void RefreshEffects(IReadOnlyList<string> effectIds)
        {
            var counts = new Dictionary<string, int>();

            foreach (string id in effectIds)
            {
                counts.TryGetValue(id, out int count);
                counts[id] = count + 1;
            }

            var text = new StringBuilder();

            foreach (var pair in counts)
            {
                string stat = Table.GetString(TableType.RoguelikeEffect, pair.Key, "stat");
                float value = Table.GetFloat(TableType.RoguelikeEffect, pair.Key, "value");
                text.AppendFormat(Table.GetString(TableType.Stat, stat, "desc_format"), value);
                text.AppendLine($" × {pair.Value}");
            }

            TextEffects.text = text.ToString();
        }

        private async void OpenSelect(RoguelikeStage stage)
        {
            IsOpeningSelect = true;
            int version = OpenVersion;
            int wave = stage.WaveNumber;

            try
            {
                var popup = await PopupMgr.Instance.Get<Popup_RoguelikeSelect>(PopupType.RoguelikeSelect);

                if (popup == null)
                {
                    throw new System.InvalidOperationException("로그라이크 효과 선택 팝업을 등록해야 합니다.");
                }

                if (!isActiveAndEnabled || version != OpenVersion ||
                    !BattleMgr.Instance.TryGetRoguelike(out var current) || current != stage ||
                    stage.State != RoguelikeState.Selecting || stage.WaveNumber != wave)
                {
                    PopupMgr.Instance.Close(PopupType.RoguelikeSelect);
                    return;
                }

                SelectPopup = popup;
                popup.Init(stage);
                popup.Open();
            }
            finally
            {
                IsOpeningSelect = false;

                if (version != OpenVersion && isActiveAndEnabled)
                {
                    Refresh();
                }
            }
        }

        private void Update()
        {
            if (!BattleMgr.Instance.IsReadied || BattleMgr.Instance.StageProgress == null)
            {
                return;
            }

            TextTimer.text = BattleMgr.Instance.StageTimer.GetNowTime().ToString("F1");

            if (BattleMgr.Instance.StageProgress.IsCountableGoal())
            {
                GaugePercent.SetGauge(BattleMgr.Instance.StageProgress.Progress(), BattleMgr.Instance.StageProgress.Goal());
            }
            else
            {
                GaugePercent.SetGauge(BattleMgr.Instance.StageProgress.GoalPercent());
            }
        }
    }
}
