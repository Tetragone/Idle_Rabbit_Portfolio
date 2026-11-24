using System.Collections.Generic;

using Game.Common;

namespace Game.Battle
{
    public class Buff
    {
        private Dictionary<BuffType, float> BuffTimer = new Dictionary<BuffType, float>();
        private Dictionary<DebuffType, float> DebuffTimer = new Dictionary<DebuffType, float>();

        private Dictionary<BuffType, float> BuffValue = new Dictionary<BuffType, float>();
        private Dictionary<DebuffType, float> DebuffValue = new Dictionary<DebuffType, float>();

        private List<BuffType> RemoveBuff = new List<BuffType>();
        private List<DebuffType> RemoveDebuff = new List<DebuffType>();

        private List<BuffType> NowBuffs = new List<BuffType>();
        private List<DebuffType> NowDebuff = new List<DebuffType>();

        public void Update(float time)
        {
            RemoveBuff.Clear();
            NowBuffs.Clear();

            foreach (var key in BuffTimer.Keys)
            {
                NowBuffs.Add(key);
            }

            foreach (var key in NowBuffs)
            {
                BuffTimer[key] -= time;

                if (BuffTimer[key] < 0f)
                {
                    RemoveBuff.Add(key);
                }
            }

            foreach (var key in RemoveBuff)
            {
                BuffTimer.Remove(key);
                BuffValue.Remove(key);
            }

            RemoveDebuff.Clear();
            NowDebuff.Clear();

            foreach (var key in DebuffTimer.Keys)
            {
                NowDebuff.Add(key);
            }

            foreach (var key in NowDebuff)
            {
                DebuffTimer[key] -= time;

                if (DebuffTimer[key] < 0f)
                {
                    RemoveDebuff.Add(key);
                }
            }

            foreach (var key in RemoveDebuff)
            {
                DebuffTimer.Remove(key);
                DebuffValue.Remove(key);
            }
        }

        public void Add(BuffType type, float time, float value)
        {
            if (IsContain(type))
            {
                if (GetValue(type) >= value - float.Epsilon)
                {
                    BuffTimer[type] = time;
                    BuffValue[type] = value;
                }
            }
            else
            {
                BuffTimer.Add(type, time);
                BuffValue.Add(type, value);
            }
        }

        public void Add(DebuffType type, float time, float value)
        {
            if (IsContain(type))
            {
                if (GetValue(type) >= value - float.Epsilon)
                {
                    DebuffTimer[type] = time;
                    DebuffValue[type] = value;
                }
            }
            else
            {
                DebuffTimer.Add(type, time);
                DebuffValue.Add(type, value);
            }
        }

        public bool IsContain(BuffType type)
        {
            return BuffTimer.ContainsKey(type);
        }

        public bool IsContain(DebuffType type)
        {
            return DebuffTimer.ContainsKey(type);
        }

        public float GetValue(BuffType type)
        {
            if (IsContain(type))
            {
                return BuffValue[type];
            }
            else
            {
                return 0f;
            }
        }

        public float GetValue(DebuffType type)
        {
            if (IsContain(type))
            {
                return DebuffValue[type];
            }
            else
            {
                return 0f;
            }
        }

        public void Reset()
        {
            RemoveBuff.Clear();
            RemoveDebuff.Clear();
        }
    }
}
