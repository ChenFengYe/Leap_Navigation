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

    private HandPair        LastHands;
    private IEventType      LastEventType;

    // Event
    public event Action<Hand, IFuncType> Navigation_HitRay;        // 命名方式： 交互类型 - 实现交互实例(- 函数功能)

    // constant
    private float GraspThreshold = 0.8f;                // for detecting this hand is making a fist or not
    private float HandsCoressThreshold = 0.01f;         // metrix is meter

	// Use this for initialization
	void Start () {
        m_EventList = new List<m_Event>();
        m_EventID = 0;
        LastEventType = IEventType.NoAction;
	}
	
    public IEventType CheckCurrentEventType(HandPair hands)
    {
        // Check hands are empty to decide fix or hold on all interaction
        if (hands.empty)
        {
            if (!LastHands.empty)
                FixCurrentHand(hands);
            else
                return IEventType.CancelAction;
        }

        switch (LastEventType)
        {
            case IEventType.NoAction:
                {
                    if (CheckTwoHandsRelaxed(hands))
                        return IEventType.NoAction;

                    if (CheckHandFist(hands.L))
                        return IEventType.
                    break;
                }

            case IEventType.WaitForCancelOrScale:
                {
                    // 双手握手
                    if (CheckHandsAllFist(hands))
                        // 手相向移动了没？ scale : wait
                        return CheckHandsMoveCross(hands) ? IEventType.Scaling : IEventType.WaitForCancelOrScale;

                    // 双手张开
                    else if (CheckTwoHandsRelaxed(hands))
                        return IEventType.CancelAction;

                    // 一手张开一手握拳
                    else
                        return IEventType.WaitForCancelOrScale;
                    break;
                }

            case IEventType.CancelAction:
                {
                    if ()
	                {
	                	 
	                }
                    break;
                }
            case IEventType.Scaling:
                {

                    break;
                }
            case IEventType.StrollingWithFingerDirection:
                {

                    break;
                }
            default:
                {

                    break;
                }
        }
        // Navigation
        if (true)
        {
            
        }
        
        // check Left hand is fistting
        LastEventType = 

        return;



        //// 此处触发事件
        //if (m_RayHit == false)
        //{
        //    Navigation_HitRay_Init(hands.hand_R, IFuncType.Init);
        //    //HitRay_Init(hands.hand_L);
        //    m_RayHit = true;
        //}
        //
        ////HitRay_Update(hands.hand_L);
        //Navigation_HitRay_Update(hands.hand_R);
    }

	// if there occurs that emtpy hand while last time have hands, Fix hands with this function
    private void FixCurrentHand(HandPair hands)
    {
        hands.empty = false;
        hands.L.CopyFrom(LastHands.L);
        hands.R.CopyFrom(LastHands.R);
    }

    //--------------------------------------------------------------------------------------------------
    // 检查状态的函数

    // 双手打开
    private bool CheckTwoHandsRelaxed(HandPair hands)
    {
        return !CheckHandFist(hands.L) && !CheckHandFist(hands.R)? true:false;
    }

    // 手握拳
    private bool CheckHandFist(Hand hand)
    {
        return hand.GrabStrength < GraspThreshold ?true : false;
    }

    // 双手握拳
    private bool CheckHandsAllFist(HandPair hands)
    {
        return CheckHandFist(hands.L) && CheckHandFist(hands.R) ? true : false;
    }

    // 检查两手相向移动
    private bool CheckHandsMoveCross(HandPair hands)
    {
        if (hands.empty)
            Debug.Log("There may exist an error or this is a pair fixed hands");

        float PalmDistance = hands.L.PalmPosition.DistanceTo(hands.R.PalmPosition);
        float LastPalmDistance = LastHands.L.PalmPosition.DistanceTo(LastHands.R.PalmPosition);

        return LastPalmDistance - PalmDistance > HandsCoressThreshold ? true : false;
    }


    //--------------------------------------------------------------------------------------------------
    public void UpdateEvent()
    {
    
    }

    public void UpdateLastEventType(IEventType eventType)
    {
        LastEventType = eventType;
    }

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