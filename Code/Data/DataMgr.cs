using UnityEngine;

using Game.Common;
using Game.Core;
using Game.UI;

namespace Game.Data
{
    // 서버 저장이 필요하면 서버 저장과 로컬 저장을 나눌수 있도록 
    // 이 클래스에서 데이터 로딩을 해온다
    public class DataMgr : SingletonAllTime<DataMgr>
    {
        private LocalDataMgr LocalData;
        private float SaveTimer = 0f;
        private readonly float SaveTime = 300f;

        public void StartLoad()
        {
            LocalData = new LocalDataMgr();
            MyLogger.Log(LogSituation.Load, ProcessType.Start, "Strat Load");
            Load();
            MyLogger.Log(LogSituation.Load, ProcessType.Done, "Strat Load");
            SaveTimer = SaveTime;
        }

        private void OnApplicationFocus(bool focus)
        {
            if (!focus)
            {
                Save();
            }
        }

        private void OnApplicationQuit()
        {
            Save();
        }

        private void Update()
        {
            SaveTimer += Time.deltaTime;

            if (SaveTimer > SaveTime)
            {
                // SaveTimer -= SaveTime말고 0f로 선언해서 정확히 시간을 맞추지 않도록 하자
                // 즉 SaveTime + α 기준으로 저장하도록
                SaveTimer = 0f;
                MyLogger.Log(LogSituation.Save, ProcessType.Start, "Period Save");
                Save();
                MyLogger.Log(LogSituation.Save, ProcessType.Done, "Period Save");
            }
        }

        public void Load()
        {
            LocalData.LoadAllData();
        }

        public void Save()
        {
            if (LocalData == null) return;
            LocalData.SaveAllData();
        }
    }
}
