using System.Collections.Generic;
using System.Collections.ObjectModel;

using Game.Common;

namespace Game.Quests
{
    public sealed class QuestReceiveAllResult
    {
        public bool IsAccepted { get; }
        public int ReceivedQuestCount { get; }
        public int FailedQuestCount { get; }
        public IReadOnlyDictionary<Currency, double> Rewards { get; }

        public QuestReceiveAllResult(bool isAccepted, int receivedQuestCount, int failedQuestCount,
            Dictionary<Currency, double> rewards = null)
        {
            IsAccepted = isAccepted;
            ReceivedQuestCount = receivedQuestCount;
            FailedQuestCount = failedQuestCount;
            Rewards = new ReadOnlyDictionary<Currency, double>(rewards == null
                ? new Dictionary<Currency, double>()
                : new Dictionary<Currency, double>(rewards));
        }
    }
}
