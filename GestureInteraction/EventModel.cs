using UnityEngine;
using System.Collections;
using GestureInteraction;
using System.Collections.Generic;
using System;
using Leap;


public class EventModel : MonoBehaviour {

    private List<m_Event>   m_EventList;
    private Int64           m_EventID;
    private bool            m_RayHit = false;

	// Use this for initialization
	void Start () {
        m_EventList = new List<m_Event>();
        m_EventID = 0;

        // 此处设置订阅事件 ?
	}
	
    public void CheckCurrentEventType(m_Hands hands,out m_EventType eventType)
    {

        eventType = m_EventType.StrollingWithFingerDirection ;

        // 此处触发事件
        if (m_RayHit == false)
        {
            Navigation_HitRay_Init(hands.hand_R);
            //HitRay_Init(hands.hand_L);
            m_RayHit = true;
        }

        //HitRay_Update(hands.hand_L);
        Navigation_HitRay_Update(hands.hand_R);
    }

    //public void 
    // 修改此事件为真正的事件函数而不是一个简单的函数
    // 见LeapProvider 如何构建事件控制
    // 见LeapHandController 如何构建事件监听 订阅

    public event Action<Hand>   Navigation_HitRay_Init;        // 命名方式： 交互类型 - 实现交互实例 - 函数功能
    public event Action<Hand> Navigation_HitRay_Update;        // 命名方式： 交互类型 - 实现交互实例 - 函数功能
    public event Action<Hand> Navigation_HitRay_Close;       // 命名方式： 交互类型 - 实现交互实例 - 函数功能

    //protected void HitRay_Init(Hand hand)
    //{
    //    if (Navigation_HitRay_Init != null)
    //    {
    //        Navigation_HitRay_Init(hand);
    //    }
    //}

    //protected void HitRay_Update(Hand hand)
    //{
    //    if (Navigation_HitRay_Update != null)
    //    {
    //        Navigation_HitRay_Update(hand);
    //    }
    //}

    //protected void HitRay_Close(Hand hand)
    //{
    //    if (Navigation_HitRay_Close != null)
    //    {
    //        Navigation_HitRay_Close(hand);
    //    }
    //}
}