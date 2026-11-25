using Game.Common;
using Game.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Game.Data
{
    public class Table : SingletonAllTime<Table>
    {
        public static bool IsReadDone { get; private set; } = false;

        private Dictionary<TableType, Dictionary<string, Dictionary<string, string>>> Tables = new Dictionary<TableType, Dictionary<string, Dictionary<string, string>>>();
        private Dictionary<TableType, string> TableName = new Dictionary<TableType, string>()
        {
            { TableType.Currency, "Tables/c_currency" },
            { TableType.Dungeon, "Tables/c_dungeon" },
            { TableType.Enemy, "Tables/c_enemy" },
            { TableType.Equip, "Tables/c_equip" },
            { TableType.EquipGacha, "Tables/c_equipgacha" },
            { TableType.Gachas, "Tables/c_gachas" },
            { TableType.Shop, "Tables/c_shop" },
            { TableType.ShopCategory, "Tables/c_shopcategory" },
            { TableType.Skill, "Tables/c_skill" },
            { TableType.Soul, "Tables/c_soul" },
            { TableType.SoulSubOption, "Tables/c_soulsuboption" },
            { TableType.Sound, "Tables/c_sound" },
            { TableType.Stage, "Tables/c_stage" },
            { TableType.Stat, "Tables/c_stat" },
            { TableType.Upgrade, "Tables/c_upgrade" },
            { TableType.Event, "Tables/c_event" },
            { TableType.EventCurrency, "Tables/c_eventcurrency" },
            { TableType.EventHunt, "Tables/c_eventhunt" },
            { TableType.EventShop, "Tables/c_eventshop" },
            { TableType.Quest, "Tables/c_quest" },
            { TableType.RoguelikeWave, "Tables/c_roguelike_wave" },
            { TableType.RoguelikeEffect, "Tables/c_roguelike_effect" },
        };

        // 공개본에는 파서를 포함하지 않으며 외부에서 설정 공급 함수를 연결한다.
        public static Func<string, Dictionary<string, Dictionary<string, string>>> ReadSource;

        private int CorutineCount = 10;

        protected override void Awake()
        {
            base.Awake();
            StartCoroutine(ReadTables());
        }

        private IEnumerator ReadTables()
        {
            if (ReadSource == null)
            {
                throw new InvalidOperationException("공개본의 Table.ReadSource에 테이블 공급 함수를 연결해야 합니다.");
            }

            int cnt = 0;

            foreach (TableType key in TableName.Keys)
            {
                Tables.Add(key, ReadSource(TableName[key]));
                cnt++;

                if (cnt == CorutineCount)
                {
                    yield return null;
                }
            }

            IsReadDone = true;
        }

        public static Dictionary<string, Dictionary<string, string>> GetTable(TableType name)
        {
            if (Instance.Tables.ContainsKey(name))
            {
                return Instance.Tables[name];
            }
            else
            {
                Debug.LogErrorFormat("{0}은 없는 테이블입니다.", name);
                return null;
            }
        }

        public static bool IsValidKey(TableType name, string id)
        {
            return Instance.Tables[name].ContainsKey(id);
        }

        public static bool IsValidColumn(TableType name, string column)
        {
            string anyKey = Instance.Tables[name].Keys.First();
            return Instance.Tables[name][anyKey].ContainsKey(column);
        }

        public static string GetString(TableType name, string id, string column)
        {
            return Instance.Tables[name][id][column];
        }

        public static int GetInt(TableType name, string id, string column)
        {
            return Int32.Parse(Instance.Tables[name][id][column], CultureInfo.InvariantCulture);
        }

        public static float GetFloat(TableType name, string id, string column)
        {
            return float.Parse(Instance.Tables[name][id][column], CultureInfo.InvariantCulture);
        }

        public static long GetLong(TableType name, string id, string column)
        {
            return Int64.Parse(Instance.Tables[name][id][column], CultureInfo.InvariantCulture);
        }
    }
}
