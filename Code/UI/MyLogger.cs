using UnityEngine;
using Game.Common;

namespace Game.UI
{
    public static class MyLogger
    {
        public static void Log(LogType type, LogSituation situation, ProcessType process, string log)
        {
            switch (type)
            {
                case LogType.Log:
                    Debug.LogFormat("[{0}] {1} → {2}", situation.ToString(), process.ToString(), log);
                    break;
                case LogType.Warning:
                    Debug.LogWarningFormat("[{0}] {1} → {2}", situation.ToString(), process.ToString(), log);
                    break;
                case LogType.Error:
                    Debug.LogErrorFormat("[{0}] {1} → {2}", situation.ToString(), process.ToString(), log);
                    break;

            }

        }

        public static void Log(LogSituation situation, ProcessType process, string log)
        {
            Log(LogType.Log, situation, process, log);
        }
    }
}
