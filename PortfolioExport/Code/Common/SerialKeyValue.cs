using System;

namespace Game.Common
{
    [Serializable]
    public struct SerialKeyValue<Tkey, TValue>
    {
        public Tkey Key;
        public TValue Value;

        public SerialKeyValue(Tkey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }
}
