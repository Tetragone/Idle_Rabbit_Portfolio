using System.Collections.Generic;

using UnityEngine;

using Game.Common;
using Game.Data;

namespace Game.Calculate
{
    public static class StageCalculator
    {
        private static List<int> MaxStage
        {
            get
            {
                if (StageCount.Count == 0)
                {
                    var table = Table.GetTable(TableType.Stage);
                    int maxChapter = 0;

                    foreach (string key in table.Keys)
                    {
                        string sChapter = Table.GetString(TableType.Stage, key, "chapter");

                        if (sChapter != "x")
                        {
                            int chapter = Table.GetInt(TableType.Stage, key, "chapter");
                            maxChapter = Mathf.Max(chapter, maxChapter);
                        }
                    }

                    StageCount.Add(0);
                    for (int i = 0; i < maxChapter; i++)
                    {
                        StageCount.Add(0);
                    }

                    foreach (string key in table.Keys)
                    {
                        string sChapter = Table.GetString(TableType.Stage, key, "chapter");


                        if (sChapter != "x")
                        {
                            int chapter = Table.GetInt(TableType.Stage, key, "chapter");
                            int stage = Table.GetInt(TableType.Stage, key, "stage");

                            StageCount[chapter] = Mathf.Max(stage, StageCount[chapter]);
                        }
                    }
                }

                return StageCount;
            }
        }
        private static List<int> StageCount = new List<int>();

        public static int GetNextStageInt(int sid)
        {
            return GetNextStageInt(sid.ToString());
        }

        public static int GetNextStageInt(string sid)
        {
            string result = GetNextStage(sid);

            if (result == "")
            {
                return 0;
            }
            else
            {
                return int.Parse(result);
            }
        }

        public static string GetNextStage(int sid)
        {
            return GetNextStage(sid.ToString());
        }

        public static string GetNextStage(string sid)
        {
            if (!Table.IsValidKey(TableType.Stage, sid))
            {
                return "";
            }

            int chapter = Table.GetInt(TableType.Stage, sid, "chapter");
            int stage = Table.GetInt(TableType.Stage, sid, "stage");
            int nextChapter = 0;
            int nextStage = 0;

            if (MaxStage[chapter] == stage)
            {
                nextChapter = chapter + 1;
                nextStage = 1;
            }
            else
            {
                nextChapter = chapter;
                nextStage = stage + 1;
            }

            foreach (string key in Table.GetTable(TableType.Stage).Keys)
            {
                int nowChapter = Table.GetInt(TableType.Stage, key, "chapter");
                int nowStage = Table.GetInt(TableType.Stage, key, "stage");

                if (nowChapter == nextChapter && nowStage == nextStage)
                {
                    return key;
                }
            }

            return "";
        }

        public static string GetBeforeStage(string sid)
        {
            if (!Table.IsValidKey(TableType.Stage, sid))
            {
                return "";
            }

            int chapter = Table.GetInt(TableType.Stage, sid, "chapter");
            int stage = Table.GetInt(TableType.Stage, sid, "stage");
            int beforeChapter = 0;
            int beforeStage = 0;

            if (stage == 1)
            {
                if (chapter == 1)
                {
                    return "";
                }

                beforeChapter = chapter - 1;
                beforeStage = MaxStage[beforeChapter];
            }
            else
            {
                beforeChapter = chapter;
                beforeStage = stage - 1;
            }

            foreach (string key in Table.GetTable(TableType.Stage).Keys)
            {
                int nowChapter = Table.GetInt(TableType.Stage, key, "chapter");
                int nowStage = Table.GetInt(TableType.Stage, key, "stage");

                if (nowChapter == beforeChapter && nowStage == beforeStage)
                {
                    return key;
                }
            }

            return "";
        }

        public static int GetFirstStage(BattleType type)
        {
            switch (type)
            {
                case BattleType.Normal:
                    foreach (string key in Table.GetTable(TableType.Stage).Keys)
                    {
                        int chapter = Table.GetInt(TableType.Stage, key, "chapter");
                        int stage = Table.GetInt(TableType.Stage, key, "stage");

                        if (chapter == 1 && stage == 1)
                        {
                            return int.Parse(key);
                        }
                    }
                    break;
                case BattleType.GoldDungeon:
                default:
                    return 0;
            }

            return 1;
        }

        public static bool IsFirstChapterStage(string sid)
        {
            if (!Table.IsValidKey(TableType.Stage, sid))
            {
                return false;
            }

            int stage = Table.GetInt(TableType.Stage, sid, "stage");

            return stage == 1;
        }

        public static bool IsLastChapterStage(string sid)
        {
            if (!Table.IsValidKey(TableType.Stage, sid))
            {
                return false;
            }

            int chapter = Table.GetInt(TableType.Stage, sid, "chapter");
            int stage = Table.GetInt(TableType.Stage, sid, "stage");

            return MaxStage[chapter] == stage;
        }
    }
}
