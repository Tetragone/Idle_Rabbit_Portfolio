using System;
using System.Collections.Generic;

using Game.Common;
using Game.Core;

namespace Game.Data
{
    public class OptionDataMgr : SingletonAllTime<OptionDataMgr>
    {
        private OptionData MyData = new OptionData();

        private Dictionary<SoundType, float> SoundValue = new Dictionary<SoundType, float>();

        public void SetData(OptionData data)
        {
            MyData.SetData(data);
            SoundValue.Clear();

            foreach (var value in MyData.SoundValue)
            {
                SoundValue.Add((SoundType)Enum.Parse(typeof(SoundType), value.Key), value.Value);
            }
        }

        public OptionData GetData()
        {
            MyData.SoundValue.Clear();

            foreach (var key in SoundValue.Keys)
            {
                MyData.SoundValue.Add(new SerialKeyValue<string, float>(key.ToString(), SoundValue[key]));
            }

            return MyData;
        }

        public float GetSoundValue(SoundType type)
        {
            if (!SoundValue.ContainsKey(type))
            {
                return 1;
            }

            return SoundValue[type];
        }

        public void SetSoundValue(SoundType type, float value)
        {
            if (!SoundValue.ContainsKey(type))
            {
                SoundValue.Add(type, 1);
            }

            SoundValue[type] = value;
        }
    }

    public class OptionData : IBaseData<OptionData>
    {
        public List<SerialKeyValue<string, float>> SoundValue = new List<SerialKeyValue<string, float>>();

        public void SetData(OptionData beforeData)
        {
            SoundValue.Clear();
            SoundValue = new List<SerialKeyValue<string, float>>(beforeData.SoundValue);
        }

        public void SetToDefault()
        {
            SoundValue.Clear();
        }
    }
}
