using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public static class Extensions {
    public static void AddEventListener(this EventTrigger trigger, EventTriggerType type, System.Action<BaseEventData> callback){
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = type;
        entry.callback = new EventTrigger.TriggerEvent();
        entry.callback.AddListener(new UnityEngine.Events.UnityAction<BaseEventData>(callback));
        trigger.triggers.Add(entry);
    }
    public static void Deconstruct(this Vector2Int value, out int x, out int y){
        x = value.x;
        y = value.y;
    }
    public static void Fill<T>(this T[] array, T value){
        for(int i = 0; i < array.Length; i++) array[i] = value;
    }
    public static T First<T>(this IEnumerable<T> enumerable){
        if(enumerable is IList<T> list){
            if(list.Count > 0) return list[0];
        }else using(var iterator = enumerable.GetEnumerator()){
            if(iterator.MoveNext()) return iterator.Current;
        }
        return default(T);
    }
    public static int IndexOf<T>(this IEnumerable<T> enumerable, T value){
        int index = 0;
        foreach(var item in enumerable)
            if(EqualityComparer<T>.Default.Equals(item, value)) return index;
            else index++;
        return -1;
    }
}