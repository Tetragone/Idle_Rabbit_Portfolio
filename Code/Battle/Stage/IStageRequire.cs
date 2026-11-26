using Game.Common;

namespace Game.Battle.Stage
{
    public interface IStageRequire
    {
        public Currency NeedTicket { get; }
        public int NeedCount { get; }
    }
}

