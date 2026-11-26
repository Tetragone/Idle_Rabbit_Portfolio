using System;
using System.Globalization;

using Game.Common;
using Game.Data;

namespace Game.Events
{
    // 획득 기록은 보관하지 않고 사냥 설정과 지급량 계산만 담당한다.
    public sealed class EventHuntRule
    {
        public string CurrencyId { get; }
        public BattleType HuntType { get; }
        public float DropRate { get; }
        public int DropCount { get; }
        public int MaxDailyGet { get; }
        public int MaxGet { get; }
        public bool IsGetOffline { get; }

        private readonly decimal OfflineDropRate;

        public EventHuntRule(string currencyId, BattleType huntType, float dropRate, int dropCount,
            int maxDailyGet, int maxGet, bool isGetOffline)
        {
            if (string.IsNullOrEmpty(currencyId))
                throw new ArgumentException("이벤트 지급 재화 ID가 비어 있습니다.", nameof(currencyId));
            if (!Enum.IsDefined(typeof(BattleType), huntType))
                throw new ArgumentException("지원하지 않는 사냥 종류입니다.", nameof(huntType));
            if (float.IsNaN(dropRate) || float.IsInfinity(dropRate) || dropRate < 0f || dropRate > 1f)
                throw new ArgumentException("드롭 확률은 0부터 1 사이여야 합니다.", nameof(dropRate));
            if (dropCount <= 0)
                throw new ArgumentException("드롭 수량은 양수여야 합니다.", nameof(dropCount));
            if (maxDailyGet < -1 || maxGet < -1)
                throw new ArgumentException("획득 한도는 0 이상이거나 제한 없음을 뜻하는 -1이어야 합니다.");

            CurrencyId = currencyId;
            HuntType = huntType;
            DropRate = dropRate;
            DropCount = dropCount;
            MaxDailyGet = maxDailyGet;
            MaxGet = maxGet;
            IsGetOffline = isGetOffline;

            // 0.7 같은 설정값을 이진 실수의 근삿값으로 누적하지 않는다.
            OfflineDropRate = decimal.Parse(dropRate.ToString("R", CultureInfo.InvariantCulture),
                NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        public static EventHuntRule Load(string eventId)
        {
            // 사냥 설정이 없는 이벤트는 퀘스트 등 다른 획득 기능만 사용할 수 있다.
            if (!Table.IsValidKey(TableType.EventHunt, eventId)) return null;

            string currencyId = Table.GetString(TableType.EventHunt, eventId, "get_currency");
            if (!Table.IsValidKey(TableType.EventCurrency, currencyId))
                throw new ArgumentException(string.Format("이벤트 '{0}'의 재화 '{1}'가 존재하지 않습니다.", eventId, currencyId));

            string huntType = Table.GetString(TableType.EventHunt, eventId, "hunt_type");
            if (!Enum.TryParse(huntType, true, out BattleType battleType) || !Enum.IsDefined(typeof(BattleType), battleType))
                throw new ArgumentException(string.Format("이벤트 '{0}'의 사냥 종류 '{1}'가 잘못되었습니다.", eventId, huntType));

            string offline = Table.GetString(TableType.EventHunt, eventId, "is_get_offline");
            if (offline != "o" && offline != "x")
                throw new ArgumentException(string.Format("이벤트 '{0}'의 오프라인 허용 값은 o 또는 x여야 합니다.", eventId));

            return new EventHuntRule(currencyId, battleType,
                Table.GetFloat(TableType.EventHunt, eventId, "drop_rate"),
                Table.GetInt(TableType.EventHunt, eventId, "drop_count"),
                ReadLimit(eventId, "max_daily_get"), ReadLimit(eventId, "max_get"), offline == "o");
        }

        private static int ReadLimit(string eventId, string column)
        {
            string value = Table.GetString(TableType.EventHunt, eventId, column);
            if (value == "x") return -1;
            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int limit) || limit < 0)
                throw new ArgumentException(string.Format("이벤트 '{0}'의 {1}은 0 이상의 정수 또는 x여야 합니다.", eventId, column));
            return limit;
        }

        public int GetDropCount(int dailyCount, int totalCount, int currencyCount)
        {
            return Math.Min(DropCount, GetAvailableCount(dailyCount, totalCount, currencyCount));
        }

        public decimal GetOfflineExpectedCount(long seconds, double remainder)
        {
            decimal beforeRemainder = decimal.Parse(remainder.ToString("R", CultureInfo.InvariantCulture),
                NumberStyles.Float, CultureInfo.InvariantCulture);

            return seconds * OfflineDropRate * DropCount + beforeRemainder;
        }

        public int GetAvailableCount(int dailyCount, int totalCount, int currencyCount)
        {
            int count = int.MaxValue;
            if (MaxDailyGet >= 0) count = Math.Min(count, Math.Max(0, MaxDailyGet - dailyCount));
            if (MaxGet >= 0) count = Math.Min(count, Math.Max(0, MaxGet - totalCount));

            // 제한이 없어도 저장 수량이 정수 범위를 넘지 않도록 한다.
            count = Math.Min(count, int.MaxValue - currencyCount);
            count = Math.Min(count, int.MaxValue - dailyCount);
            return Math.Min(count, int.MaxValue - totalCount);
        }
    }
}
