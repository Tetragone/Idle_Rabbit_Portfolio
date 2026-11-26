using System;
using System.Collections.Generic;
using System.Globalization;

using Game.Common;
using Game.Data;

namespace Game.Quests
{
    public sealed class QuestRule
    {
        public string Id { get; }
        public string Category { get; }
        public QuestType Type { get; }
        public QuestAction Action { get; }
        public long Goal { get; }
        public string RewardType { get; }
        public int RewardCount { get; }
        public string Name { get; }
        public string Description { get; }
        public string RewardResource { get; }

        public QuestRule(string id, string category, QuestType type, QuestAction action,
            long goal, string rewardType, int rewardCount, string name = "", string description = "{0} / {1}",
            string rewardResource = "")
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(category) || string.IsNullOrEmpty(rewardType))
                throw new ArgumentException("퀘스트 ID, 소속, 보상 종류는 비어 있을 수 없습니다.");
            if (!Enum.IsDefined(typeof(QuestType), type) || !Enum.IsDefined(typeof(QuestAction), action))
                throw new ArgumentException("지원하지 않는 퀘스트 종류 또는 행동입니다.");
            if (goal <= 0 || rewardCount <= 0)
                throw new ArgumentException("퀘스트 목표와 보상 수량은 양수여야 합니다.");

            Id = id;
            Category = category;
            Type = type;
            Action = action;
            Goal = goal;
            RewardType = rewardType;
            RewardCount = rewardCount;
            Name = string.IsNullOrEmpty(name) ? id : name;
            Description = description == "x" ? "" : description ?? "";
            RewardResource = rewardResource;

            // 잘못된 기획 문자열은 화면 표시 때까지 미루지 않고 설정 로드 시 확인한다.
            string.Format(CultureInfo.InvariantCulture, Description, 0L, Goal);
        }

        public string FormatDescription(long currentCount)
        {
            return string.Format(Description, currentCount, Goal);
        }

        public static List<QuestRule> LoadAll()
        {
            var result = new List<QuestRule>();
            foreach (string id in Table.GetTable(TableType.Quest).Keys)
            {
                string category = Table.GetString(TableType.Quest, id, "category");
                string type = Table.GetString(TableType.Quest, id, "type");
                string action = Table.GetString(TableType.Quest, id, "quest");
                if (category != "common" && !Table.IsValidKey(TableType.Event, category))
                    throw new ArgumentException(string.Format("퀘스트 '{0}'의 이벤트 '{1}'가 없습니다.", id, category));
                if (!Enum.TryParse(type, true, out QuestType questType) || !Enum.IsDefined(typeof(QuestType), questType))
                    throw new ArgumentException(string.Format("퀘스트 '{0}'의 초기화 종류가 잘못되었습니다.", id));
                if (!Enum.TryParse(action.Replace("_", ""), true, out QuestAction questAction)
                    || !Enum.IsDefined(typeof(QuestAction), questAction))
                    throw new ArgumentException(string.Format("퀘스트 '{0}'의 행동이 잘못되었습니다.", id));

                string reward = Table.GetString(TableType.Quest, id, "reward_type");
                bool normalCurrency = Enum.TryParse(reward, out Currency currency) && Enum.IsDefined(typeof(Currency), currency);
                if (!normalCurrency && (!Table.IsValidKey(TableType.EventCurrency, reward)
                    || Table.GetString(TableType.EventCurrency, reward, "event_id") != category))
                    throw new ArgumentException(string.Format("퀘스트 '{0}'의 보상 재화 또는 소속이 잘못되었습니다.", id));

                result.Add(new QuestRule(id, category, questType, questAction,
                    Table.GetLong(TableType.Quest, id, "value"), reward,
                    Table.GetInt(TableType.Quest, id, "reward_value"),
                    Table.GetString(TableType.Quest, id, "name"),
                    Table.GetString(TableType.Quest, id, "desc"),
                    Table.GetString(normalCurrency ? TableType.Currency : TableType.EventCurrency, reward, "res")));
            }
            return result;
        }
    }
}
