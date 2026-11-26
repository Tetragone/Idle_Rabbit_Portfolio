using Game.Common;
using Game.Core;
using Game.Events;
using Game.UI;
using System;
using System.Collections.Generic;
using Game.Quests;

namespace Game.Data
{
    public class LocalDataMgr
    {
        private readonly string PPath = "pd";
        private readonly string EPath = "ed";
        private readonly string SPath = "sd";
        private readonly string UPath = "ud";
        private readonly string OPath = "od";
        private readonly string SoPath = "sod";
        private readonly string QPath = "qd";
        private readonly Dictionary<string, Action> EventLoadActions = new Dictionary<string, Action>();
        private readonly Dictionary<string, Action> EventSaveActions = new Dictionary<string, Action>();

        private bool IsInit = false;

        public void LoadAllData()
        {
            MyLogger.Log(LogSituation.Load, ProcessType.Start, "Local All Data");

            if (!IsInit)
            {
                IsInit = true;
                InitDatas();
            }

            LoadPlayerData();
            LoadEquipData();
            LoadSkillData();
            LoadUpgradeData();
            LoadOptionData();
            LoadSoulData();
            LoadQuestData();
            LoadEventData();
            MyLogger.Log(LogSituation.Load, ProcessType.Done, "Local All Data");
        }

        public void InitDatas()
        {
            QuestMgr.Instance.Initialize();
            EventMgr.Instance.Initialize();
            EventLoadActions.Clear();
            EventSaveActions.Clear();

            EventMgr.Instance.RegisterDataHandlers<CurrencyExchangeEventData>(RegisterEvent<CurrencyExchangeEventData>);
        }

        public void LoadPlayerData()
        {
            MyLogger.Log(LogSituation.Load, ProcessType.Start, "Local Player Data");
            PlayerData pData = SaveHelper<PlayerData>.Load(PPath);
            PlayerDataMgr.Instance.SetData(pData);
            MyLogger.Log(LogSituation.Load, ProcessType.Done, "Local Player Data");
        }

        public void LoadEquipData()
        {
            MyLogger.Log(LogSituation.Load, ProcessType.Start, "Local Equip Data");
            EquipData eData = SaveHelper<EquipData>.Load(EPath);
            EquipDataMgr.Instance.SetData(eData);
            MyLogger.Log(LogSituation.Load, ProcessType.Done, "Local Equip Data");
        }

        public void LoadSkillData()
        {
            MyLogger.Log(LogSituation.Load, ProcessType.Start, "Local Skill Data");
            SkillData sData = SaveHelper<SkillData>.Load(SPath);
            SkillDataMgr.Instance.SetData(sData);
            MyLogger.Log(LogSituation.Load, ProcessType.Done, "Local Skill Data");
        }

        public void LoadUpgradeData()
        {
            MyLogger.Log(LogSituation.Load, ProcessType.Start, "Local Upgrade Data");
            UpgradeData uData = SaveHelper<UpgradeData>.Load(UPath);
            UpgradeDataMgr.Instance.SetData(uData);
            MyLogger.Log(LogSituation.Load, ProcessType.Done, "Local Upgrade Data");
        }

        public void LoadOptionData()
        {
            MyLogger.Log(LogSituation.Load, ProcessType.Start, "Local Option Data");
            OptionData oData = SaveHelper<OptionData>.Load(OPath);
            OptionDataMgr.Instance.SetData(oData);
            MyLogger.Log(LogSituation.Load, ProcessType.Done, "Local Option Data");
        }

        public void LoadSoulData()
        {
            MyLogger.Log(LogSituation.Load, ProcessType.Start, "Local Soul Data");
            SoulData soData = SaveHelper<SoulData>.Load(SoPath);
            SoulDataMgr.Instance.SetData(soData);
            MyLogger.Log(LogSituation.Load, ProcessType.Done, "Local Soul Data");
        }

        public void LoadEventData()
        {
            MyLogger.Log(LogSituation.Load, ProcessType.Start, "Local Event Data");
            foreach (Action load in EventLoadActions.Values)
            {
                load();
            }

            // 저장 데이터를 모두 적용한 후 현재 회차의 초기화 여부를 판단한다.
            EventMgr.Instance.Refresh(TimeMgr.GetNowTimeNum());
            MyLogger.Log(LogSituation.Load, ProcessType.Done, "Local Event Data");
        }

        public void LoadQuestData()
        {
            MyLogger.Log(LogSituation.Load, ProcessType.Start, "Local Quest Data");
            QuestMgr.Instance.SetData(SaveHelper<QuestData>.Load(QPath));
            QuestMgr.Instance.Refresh(TimeMgr.GetNowTimeNum());
            MyLogger.Log(LogSituation.Load, ProcessType.Done, "Local Quest Data");
        }

        private void RegisterEvent<T>(string eventId, Func<T> getData, Action<T> setData)
            where T : IBaseData<T>, new()
        {
            string path = string.Format("evd_{0}", eventId);

            // 등록 시 구체적인 타입을 확정하여 문자열로 조회할 때도 같은 타입으로 처리한다.
            EventLoadActions.Add(eventId, () => setData(SaveHelper<T>.Load(path)));
            EventSaveActions.Add(eventId, () => SaveHelper<T>.Save(getData(), path));
        }

        public void SaveAllData()
        {
            MyLogger.Log(LogSituation.Save, ProcessType.Start, "Local All Data");
            SavePlayerData();
            SaveEquipData();
            SaveSkillData();
            SaveUpgradeData();
            SaveOptionData();
            SaveSoulData();
            SaveEventData();
            SaveQuestData();
            MyLogger.Log(LogSituation.Save, ProcessType.Done, "Local All Data");
        }

        public void SavePlayerData()
        {
            MyLogger.Log(LogSituation.Save, ProcessType.Start, "Local Player Data");
            PlayerData pData = PlayerDataMgr.Instance.GetData();
            SaveHelper<PlayerData>.Save(pData, PPath);
            MyLogger.Log(LogSituation.Save, ProcessType.Done, "Local Player Data");
        }

        public void SaveEquipData()
        {
            MyLogger.Log(LogSituation.Save, ProcessType.Start, "Local Equip Data");
            EquipData eData = EquipDataMgr.Instance.GetData();
            SaveHelper<EquipData>.Save(eData, EPath);
            MyLogger.Log(LogSituation.Save, ProcessType.Done, "Local Equip Data");
        }

        public void SaveSkillData()
        {
            MyLogger.Log(LogSituation.Save, ProcessType.Start, "Local Skill Data");
            SkillData sData = SkillDataMgr.Instance.GetData();
            SaveHelper<SkillData>.Save(sData, SPath);
            MyLogger.Log(LogSituation.Save, ProcessType.Done, "Local Skill Data");
        }

        public void SaveUpgradeData()
        {
            MyLogger.Log(LogSituation.Save, ProcessType.Start, "Local Upgrade Data");
            UpgradeData uData = UpgradeDataMgr.Instance.GetData();
            SaveHelper<UpgradeData>.Save(uData, UPath);
            MyLogger.Log(LogSituation.Save, ProcessType.Done, "Local Upgrade Data");
        }

        public void SaveOptionData()
        {
            MyLogger.Log(LogSituation.Save, ProcessType.Start, "Local Option Data");
            OptionData oData = OptionDataMgr.Instance.GetData();
            SaveHelper<OptionData>.Save(oData, OPath);
            MyLogger.Log(LogSituation.Save, ProcessType.Done, "Local Option Data");
        }

        public void SaveSoulData()
        {
            MyLogger.Log(LogSituation.Save, ProcessType.Start, "Local Soul Data");
            SoulData soData = SoulDataMgr.Instance.GetData();
            SaveHelper<SoulData>.Save(soData, SoPath);
            MyLogger.Log(LogSituation.Save, ProcessType.Done, "Local Soul Data");
        }

        public void SaveEventData()
        {
            MyLogger.Log(LogSituation.Save, ProcessType.Start, "Local Event Data");
            foreach (Action save in EventSaveActions.Values)
            {
                save();
            }
            MyLogger.Log(LogSituation.Save, ProcessType.Done, "Local Event Data");
        }

        public void SaveQuestData()
        {
            if (!QuestMgr.Instance.IsInitialized) return;
            MyLogger.Log(LogSituation.Save, ProcessType.Start, "Local Quest Data");
            SaveHelper<QuestData>.Save(QuestMgr.Instance.GetData(), QPath);
            MyLogger.Log(LogSituation.Save, ProcessType.Done, "Local Quest Data");
        }
    }
}
