using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Searcher.Searcher.AnalyticsEvent;

public class EventCenter
{
    private static Dictionary<EventType,Delegate> m_EventTable = new Dictionary<EventType,Delegate>();





    //no parameters 无参数的监听添加
    public static void AddListener(EventType eventType,CallBack callBack)
    {
        OnListenerAdding(eventType, callBack);
        m_EventTable[eventType] = (CallBack)m_EventTable[eventType] + callBack;
    }

    //single parameters 单参数的监听添加
    public static void AddListener<T>(EventType eventType, CallBack<T> callBack)
    {
        OnListenerAdding(eventType, callBack);
        m_EventTable[eventType] = (CallBack<T>)m_EventTable[eventType] + callBack;
    }

    //two parameters 双参数的监听添加
    public static void AddListener<T, X>(EventType eventType, CallBack<T, X> callBack)
    {
        OnListenerAdding(eventType, callBack);
        m_EventTable[eventType] = (CallBack<T, X>)m_EventTable[eventType] + callBack;
    }





    //no parameters 无参数的监听移除
    public static void RemoveListener(EventType eventType, CallBack callBack)
    {
        OnListenerRemoving(eventType, callBack);
        m_EventTable[eventType] = (CallBack)m_EventTable[eventType] - callBack;
        OnListenerRemoved(eventType);
    }

    //single parameters 单参数的监听移除
    public static void RemoveListener<T>(EventType eventType, CallBack<T> callBack)
    {
        OnListenerRemoving(eventType, callBack);
        m_EventTable[eventType] = (CallBack<T>)m_EventTable[eventType] - callBack;
        OnListenerRemoved(eventType);
    }

    //two parameters 双参数的监听移除
    public static void RemoveListener<T, X>(EventType eventType, CallBack<T, X> callBack)
    {
        OnListenerRemoving(eventType, callBack);
        m_EventTable[eventType] = (CallBack<T, X>)m_EventTable[eventType] - callBack;
        OnListenerRemoved(eventType);
    }





    //no parameters 无参数的广播
    public static void Broadcast(EventType eventType)
    {
        Delegate d;
        if(m_EventTable.TryGetValue(eventType, out d))
        {
            CallBack callBack = d as CallBack;
            if(callBack != null)
            {
                callBack();
            }
            else
            {
                throw new Exception(string.Format("广播事件错误:事件{0}对应的委托具有不同的类型", eventType));
            }
        }
    }

    //single parameters 单参数的广播
    public static void Broadcast<T>(EventType eventType, T arg1)
    {
        Delegate d;
        if (m_EventTable.TryGetValue(eventType, out d))
        {
            CallBack<T> callBack = d as CallBack<T>;
            if (callBack != null)
            {
                callBack(arg1);
            }
            else
            {
                throw new Exception(string.Format("广播事件错误:事件{0}对应的委托具有不同的类型", eventType));
            }
        }
    }

    //single parameters 双参数的广播
    public static void Broadcast<T, X>(EventType eventType, T arg1, X arg2)
    {
        Delegate d;
        if (m_EventTable.TryGetValue(eventType, out d))
        {
            CallBack<T, X> callBack = d as CallBack<T, X>;
            if (callBack != null)
            {
                callBack(arg1, arg2);
            }
            else
            {
                throw new Exception(string.Format("广播事件错误:事件{0}对应的委托具有不同的类型", eventType));
            }
        }
    }





    private static void OnListenerAdding(EventType eventType, Delegate callBack)
    {
        if (!m_EventTable.ContainsKey(eventType))
        {
            m_EventTable.Add(eventType, null);
        }
        Delegate d = m_EventTable[eventType];
        if (d != null && d.GetType() != callBack.GetType())
        {
            throw new Exception(string.Format("尝试为事件{0}添加不同类型的委托，当前事件所对应的委托类型是{1}，当前要添加的委托类型为{2}", eventType, d.GetType(), callBack.GetType()));
        }
    }

    private static void OnListenerRemoving(EventType eventType, Delegate callBack)
    {
        if (m_EventTable.ContainsKey(eventType))
        {
            Delegate d = m_EventTable[eventType];
            if (d == null)
            {
                throw new Exception(string.Format("移除监听错误：事件{0}没有对应的委托", eventType));
            }
            else if (d.GetType() != callBack.GetType())
            {
                throw new Exception(string.Format("移除监听错误:尝试为事件{0}移除不同类型的委托，当前事件所对应的委托类型是{1}，当前要移除的委托类型为{2}", eventType, d.GetType(), callBack.GetType()));
            }
        }
        else
        {
            throw new Exception(string.Format("移除监听错误:没有事件码{0}", eventType));
        }
    }

    private static void OnListenerRemoved(EventType eventType)
    {
        if (m_EventTable[eventType] == null)
        {
            m_EventTable.Remove(eventType);
        }
    }
}