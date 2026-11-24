using System;
using System.Collections.Generic;

using UnityEngine;

namespace Game.Data
{
    // 공개본의 연결용 구현이다. 파일 대신 메모리에 JSON 복사본을 보관한다.
    public class SaveHelper<T> where T : IBaseData<T>, new()
    {
        private static readonly Dictionary<string, string> SavedJson = new Dictionary<string, string>();

        public static T Load(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("저장 Key가 비어 있습니다.", nameof(path));
            }

            if (SavedJson.TryGetValue(path, out string json))
            {
                return JsonUtility.FromJson<T>(json);
            }

            T data = new T();
            data.SetToDefault();
            return data;
        }

        public static void Save(T data, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("저장 Key가 비어 있습니다.", nameof(path));
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            SavedJson[path] = JsonUtility.ToJson(data, true);
        }
    }
}
