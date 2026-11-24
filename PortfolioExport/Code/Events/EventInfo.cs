namespace Game.Events
{
    // 화면은 구체 이벤트 구현 대신 표시용 설정과 상태만 조회한다.
    public sealed class EventInfo
    {
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public string Background { get; }
        public EventSchedule Schedule { get; }

        public EventInfo(string id, string name, string description, string background, EventSchedule schedule)
        {
            Id = id;
            Name = name;
            Description = description;
            Background = background;
            Schedule = schedule;
        }
    }

    public sealed class EventHuntInfo
    {
        public string CurrencyId { get; }
        public int DailyCount { get; }
        public int DailyLimit { get; }
        public int TotalCount { get; }
        public int TotalLimit { get; }

        public EventHuntInfo(string currencyId, int dailyCount, int dailyLimit, int totalCount, int totalLimit)
        {
            CurrencyId = currencyId;
            DailyCount = dailyCount;
            DailyLimit = dailyLimit;
            TotalCount = totalCount;
            TotalLimit = totalLimit;
        }
    }
}
