using System.Threading.Tasks;

using UnityEngine;

using Game.Battle;
using Game.Calculate;
using Game.Common;
using Game.Core;
using Game.Data;
using Game.UI.Popup;

namespace Game.UI.Battle
{
    public class ActionPopup : MonoBehaviour
    {
        private double BeforePower = 0d;

        private void Awake()
        {
            BattleMgr.StageEnded += StageEnded;
            PowerMgr.ChangedPower += PowerChange;
        }

        // Awake에서는 값이 정해지지 않았을수도 있기 때문
        private void Start()
        {
            BeforePower = PowerMgr.NowPowerNum;
        }

        private void OnDestroy()
        {
            BattleMgr.StageEnded -= StageEnded;
            PowerMgr.ChangedPower -= PowerChange;
        }

        private void StageEnded(BattleType type, bool isClear)
        {
            _ = OpenStageEndPopup(type, isClear);
        }

        private async Task OpenStageEndPopup(BattleType type, bool isClear)
        {
            var popup = await PopupMgr.Instance.Get<Popup_Common>(PopupType.Common);
            popup.IsUseYes(isClear);
            popup.TextTitle.text = isClear ? "스테이지 클리어" : "스테이지 실패";
            popup.TextContent.text = "";
            
            if (isClear)
            {
                bool isNormal = type == BattleType.Challenge || type == BattleType.ChapterBoss;
                int nowLv = isNormal ? PlayerDataMgr.Instance.GetMaxStage(BattleType.Normal) : PlayerDataMgr.Instance.GetMaxStage(type);
                int nextLv = isNormal ? StageCalculator.GetNextStageInt(nowLv) : nowLv + 1;
                popup.TextYes.text = "다음 스테이지 도전";
                popup.TextNo.text = "돌아가기";

                popup.ButtonYes.onClick.RemoveAllListeners();
                popup.ButtonYes.onClick.AddListener(() =>
                {
                    BattleMgr.Instance.ChangeBattleType(type, nextLv.ToString());
                    popup.Close();
                });

                popup.ButtonNo.onClick.RemoveAllListeners();
                popup.ButtonNo.onClick.AddListener(() =>
                {
                    if (type == BattleType.Challenge || type == BattleType.ChapterBoss)
                    {
                        BattleMgr.Instance.ChangeBattleType(BattleType.Normal, PlayerDataMgr.Instance.GetMaxStage(BattleType.Normal).ToString());
                    }
                    else
                    {
                        BattleMgr.Instance.ChangeBattleType(BattleType.Normal, PlayerDataMgr.Instance.GetCurStage(BattleType.Normal).ToString());
                    }
                    popup.Close();
                });

                var reward = DropCalculator.DungeonClearReward(type, nowLv);

                if (BattleMgr.Instance.StageRequire != null)
                {
                    reward.Add(BattleMgr.Instance.StageRequire.NeedTicket, BattleMgr.Instance.StageRequire.NeedCount);
                }

                if (reward.Count > 0)
                {
                    var getItemPopup = await PopupMgr.Instance.Get<Popup_GetItems>(PopupType.GetItem);
                    getItemPopup.Init(reward, "던전 클리어 보상");
                    getItemPopup.Open();
                }
            }
            else
            {
                popup.TextOk.text = "돌아가기";

                popup.ButtonOk.onClick.RemoveAllListeners();
                popup.ButtonOk.onClick.AddListener(() =>
                {
                    BattleMgr.Instance.ChangeBattleType(BattleType.Normal, PlayerDataMgr.Instance.GetCurStage(BattleType.Normal).ToString());
                    popup.Close();
                });

                // 클리어 실패했을때만 ticket을 사용하도록 하여 
                // 성공했을때는 재화와 ticket을 얻는 다는 느낌으로 해두기
                if (BattleMgr.Instance.StageRequire != null)
                {
                    PlayerDataMgr.Instance.UseCurrency(BattleMgr.Instance.StageRequire.NeedTicket, BattleMgr.Instance.StageRequire.NeedCount);
                }
            }

            popup.Open();
        }

        private void PowerChange()
        {
            _ = PopupPowerMessage();
        }

        private async Task PopupPowerMessage()
        {
            if (PowerMgr.NowPowerNum != BeforePower)
            {
                double diff = PowerMgr.NowPowerNum - BeforePower;
                string colorString = diff > 0 ? "009000" : "ff0000";
                string changed = NumberFormatter.ToCompact(diff);
                Popup_Message message = await PopupMgr.Instance.Get<Popup_Message>(PopupType.Message);
                message.Init(string.Format("현재 전투력 : {0}\n추가된 전투력 : <color=#{1}>{2}</color>", PowerMgr.NowPowerString, colorString, changed));
                message.Open();
            }
        }
    }
}
