using System;
using System.Text.RegularExpressions;

using Game.Common;
using Game.Data;

namespace Game.Events
{
    public interface IGameEvent
    {
        public event Action ChangedData;

        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public EventSchedule Schedule { get; }
        public EventPeriod Period { get; }
        public bool Refresh(long now);
    }

    public abstract class GameEvent<T> : IGameEvent where T : EventData, IBaseData<T>, new()
    {
        public event Action ChangedData;

        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public EventSchedule Schedule { get; }
        public EventPeriod Period { get; private set; } = EventPeriod.Upcoming;

        protected T Data { get; private set; }

        protected GameEvent(string id, string name, string description, EventSchedule schedule)
        {
            if (string.IsNullOrEmpty(id) || !Regex.IsMatch(id, @"\A[A-Za-z0-9_-]+\z"))
            {
                throw new ArgumentException("이벤트 ID에는 영문자, 숫자, '_'와 '-'만 사용할 수 있습니다.", nameof(id));
            }

            Id = id;
            Name = name;
            Description = description;
            Schedule = schedule;
            Data = new T();
            Data.SetToDefault();
        }

        public virtual T GetData()
        {
            T result = new T();
            result.SetData(Data);
            return result;
        }

        public virtual void SetData(T data)
        {
            T next = new T();
            next.SetData(data);
            Data = next;
        }

        public bool Refresh(long now)
        {
            bool reset = false;
            long startTicks = Schedule.StartTimeNum;

            if (now >= Schedule.StartTimeNum && Data.AppliedStartUtcTicks != startTicks)
            {
                // 새 회차는 이벤트별 기본 데이터로 교체하고 파일 저장은 저장 담당에 맡긴다.
                T next = new T();
                next.SetToDefault();
                next.EventId = Id;
                next.AppliedStartUtcTicks = startTicks;
                Data = next;
                reset = true;
            }

            EventPeriod nextPeriod = Schedule.GetPeriod(now);
            bool changed = reset || Period != nextPeriod;
            Period = nextPeriod;

            // 회차 데이터 교체 후 개별 상태까지 갱신한 다음 변경을 알린다.
            changed = RefreshData(now) || changed;

            if (changed)
            {
                NotifyChangedData();
            }

            return changed;
        }

        protected virtual bool RefreshData(long now)
        {
            return false;
        }

        protected void NotifyChangedData()
        {
            ChangedData?.Invoke();
        }

    }
}
