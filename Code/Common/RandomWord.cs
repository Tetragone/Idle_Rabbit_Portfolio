using UnityEngine;

namespace Game.Common
{
    public static class RandomWord
    {
        private static readonly string[] Starts =
        {
        "ka", "ra", "lo", "mi", "na", "sa", "ve", "tor"
    };

        private static readonly string[] Middles =
        {
        "lan", "ren", "dor", "mir", "val", "sha", "rin"
    };

        private static readonly string[] Ends =
        {
        "a", "on", "is", "el", "ar", "en", "ia"
    };

        public static string Generate()
        {
            string result =
                Starts[Random.Range(0, Starts.Length)] +
                Middles[Random.Range(0, Middles.Length)] +
                Ends[Random.Range(0, Ends.Length)];

            return char.ToUpper(result[0]) + result.Substring(1);
        }
    }
}
