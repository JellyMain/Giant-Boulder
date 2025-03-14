using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


namespace Utils
{
    public static class DataUtility
    {
        public static T GetRandomEnumValue<T>(bool hasNoneEnum) where T : Enum
        {
            Array values = Enum.GetValues(typeof(T));
            
            int randomIndex = Random.Range(hasNoneEnum ? 1 : 0, values.Length);

            return (T)values.GetValue(randomIndex);
        }
        
        
        public static string ToJson(this object obj)
        {
            return JsonUtility.ToJson(obj);
        }


        public static T ToDeserialized<T>(this string json)
        {
            return JsonUtility.FromJson<T>(json);
        }
    }
}