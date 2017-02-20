using UnityEngine;
using System.Collections;
using GestureInteraction;
using System.Collections.Generic;
using System;
using Leap;


public class EventModel : MonoBehaviour {

    public GestureInteractionController m_GIController;
    private List<m_Event>               m_EventList;
    private Int64                       m_EventID;
    private bool                        m_RayHit = false;

    private HandPair                    LastHands;


    // Event
    public event Action<Hand, IFuncType> Action_Navigation_HitRay;        // 命名方式： 交互类型 - 实现交互实例(- 函数功能)
    public event Action<Hand, IFuncType> Action_Navigation_Stroll;        // 命名方式： 交互类型 - 实现交互实例(- 函数功能)
    public event Action<Hand, IFuncType> Action_Navigation_Scale;         // 命名方式： 交互类型 - 实现交互实例(- 函数功能)

    // constant
    private float GraspThreshold = 0.8f;                // for detecting this hand is making a fist or not
    private float HandsCoressThreshold = 0.01f;         // metrix is meter

	// Use this for initialization
	void Start () {
        m_EventList = new List<m_Event>();
        m_EventID = 0;
	}

    //public void UpdateCurrentEvent(HandPair hands)
    //{
    //    //


    //    LastHands = hands;
    //}

    public IEventType UpdateCurrentEvent(IEventType CurrentEventType, IEventType LastEventType, HandPair hands)
    {
        // Check hands are empty to decide fix or hold on all interaction
        if(!CheckHandsData(hands))
            return IEventType.CancelAction;

        // If is wait event ,just wait.
        if (IsWaitEvent(CurrentEventType))
        {
            return CurrentEventType;
        }

        switch (CurrentEventType)
        {
            case IEventType.NoAction:
                {
                    // 啥也不干
                    if (CheckTwoHandsRelaxed(hands))
                        return IEventType.NoAction;

                    if (CheckHandFist(hands.L))
                    {
                        m_GIController.WaitToCheck();
                        return IEventType.Wait_NavigationOrSelectionOrCancel;
                    }
                    break;
                }
            case IEventType.CancelAction:
                {
                    // Do cancel Event
                    
                    //------//

                    // then 
                    return IEventType.NoAction;
                    break;
                }
            //-----------------------------------------------------------------------------------------
            // Navigation
            case IEventType.Navigation_Scaling:
                {
                    // Check scale event is over
                    if (CheckHandsAllFist(hands))
	                {
                        if (LastEventType != IEventType.Navigation_Scaling)
	                    	Action_Navigation_Scale(han, IFuncType.Init);
                        else
	                    	Action_Navigation_Scale(han, IFuncType.Update);
	                	return CurrentEventType;
	                }
                    else  // CheckTwoHandsRelaxed(hands)
	                {
                        Action_Navigation_Scale(han, IFuncType.Close);
                        return IEventType.NoAction;
	                }
                    break;
                }

            case IEventType.Navigation_RayHit:
                {
                    //if (LastEventType != IEventType.Navigation_RayHit)
                    //{
                    //    //if (LastEventType != IEventType.Wait... || LastEventType != IEventType.NoAction)
                    //    //{
	                    	 
                    //    //}

                    //    Navigation_HitRay(hands.R, IFuncType.Init);
                    //}
                    //else
                    //{

                    //    Navigation_HitRay(hands.R, IFuncType.Update);
                    //    Navigation_HitRay(hands.R, IFuncType.Close);
                    //}
                    Debug.Log("Error: Navigation_RayHit Event is not achieved! ");
                    return IEventType.CancelAction;
                    break;
                }
            case IEventType.Navigation_Stroll:
                {
                    if (LastEventType != IEventType.Navigation_Stroll)
                    {
                        Action_Navigation_Stroll(han, IFuncType.Init);
                        return CurrentEventType;                            // Continue
                    }
                    else if (CheckHandFist(hands.L))
                    {
                        Action_Navigation_Stroll(han, IFuncType.Update);
                        return CurrentEventType;                            // Continue
                    }
                    else
                    {
                        Action_Navigation_Stroll(han, IFuncType.Close);     // Close
                        return IEventType.NoAction;
                    }
                    break;
                }
            default:
                {
                    Debug.Log("Error: This Event is not achieved! ");
                    return IEventType.CancelAction;
                    break;
                }
        }
                // Change Frame
        Debug.Log("Error: Huge Bug 2017.2.20!!!!");
        return IEventType.CancelAction;
    }

    public IEventType CheckWaitEvent(IEventType CurrentEventType, HandPair hands)
    {
        if (!CheckHandsData(hands))
            return IEventType.CancelAction;

        if (!IsWaitEvent(CurrentEventType))
        {
            Debug.Log("Error: Current Event is not a Wait Event!");
        }

        switch (CurrentEventType)
        {
            case IEventType.Wait_CancelOrScale:
                {
                    // 双手握手
                    if (CheckHandsAllFist(hands))
                        // 手相向移动了没？ scale : wait
                        return CheckHandsMoveCross(hands) ? 
                            IEventType.Navigation_Scaling 
                            : IEventType.Wait_CancelOrScale;

                    // 双手张开
                    else if (CheckTwoHandsRelaxed(hands))
                        return IEventType.CancelAction;

                    // 一手张开一手握拳
                    else
                        return IEventType.Wait_CancelOrScale;

                    break;
                }
            case IEventType.Wait_NavigationOrSelectionOrCancel:
                {
                    if (CheckOnlyIndexPointting(hands.R))
                    {
                        return IEventType.Selection_Mutiple;
                    }
                    else
                    {
                        // check Index is pointer or not
                        if (CheckHandFist(hands.R))
                            return IEventType.Wait_CancelOrScale;
                        else
                            return IEventType.Navigation_Stroll;
                    }
                    break;
                }
            //case IEventType.Wait_ScaletoNoAction:
            //    {
            //        if (CheckTwoHandsRelaxed)
            //        {
	                	 
            //        }
            //        return 
            //        Wait_ScaletoNoAction
            //        break;
            //    }
            default:
                {
                    Debug.Log("Error: No Wait Event in WaitEvent Detect function!");
                    return IEventType.CancelAction;
                    break;
                }
        }
    }

	// if there occurs that emtpy hand while last time have hands, Fix hands with this function
    private void FixCurrentHand(HandPair hands)
    {
        hands.empty = false;
        hands.L.CopyFrom(LastHands.L);
        hands.R.CopyFrom(LastHands.R);
    }

    //--------------------------------------------------------------------------------------------------
    // Check Data
    private bool CheckHandsData(HandPair hands)
    {
        // Check hands are empty to decide fix or hold on all interaction
        if (hands.empty)
        {
            if (!LastHands.empty)
            {
                FixCurrentHand(hands);
                return true;
            }
            else
                return false;
        }
        else
            return true;
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
        if (hand.GrabStrength < GraspThreshold)
        {
            for (int i=0;  i < hand.Fingers.Count; i++)
                if (hand.Fingers[i].IsExtended)
                    return false; 
            return true;
        }
        else
            return false;
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

    // 检查单指指出
    private bool CheckOnlyIndexPointting(Hand hand)
    {
        bool OnlyIndexPointing = false;
        for (int i = 0; i < hand.Fingers.Count; i++)
		{
            if (i == 1) // index finger
            {
                OnlyIndexPointing = hand.Fingers[i].IsExtended ? true : false;
                if (!OnlyIndexPointing) break;
            }
            else
            {
                OnlyIndexPointing = hand.Fingers[i].IsExtended ? false : true;
                if (!OnlyIndexPointing) break;
            }
		}
        return OnlyIndexPointing;
    }

    // Check Is Wait Event
    public bool IsWaitEvent(IEventType eT)
    {
        if (eT == IEventType.Wait_NavigationOrSelectionOrCancel ||
            eT == IEventType.Wait_CancelOrScale ||
            eT == IEventType.Wait_ScaletoNoAction)
            return true;
        else
            return false;
    }
    //--------------------------------------------------------------------------------------------------

    //public bool checkFist(Hand hand){
    //   var sum = 0;
    //   for(var i=0;i<hand.Fingers.length;i++){
    //      var finger = hand.Fingers[i];
    //      var meta = finger.bones[0].direction();
    //      var proxi = finger.bones[1].direction();
    //      var inter = finger.bones[2].direction();
    //      var dMetaProxi = Leap.vec3.dot(meta,proxi);
    //      var dProxiInter = Leap.vec3.dot(proxi,inter);
    //      sum += dMetaProxi;
    //      sum += dProxiInter
    //   }
    //   sum = sum/10;
    
    //   if(sum<=minValue && getExtendedFingers(hand)==0){
    //       return true;
    //   }else{
    //       return false;
    //   }
    //}

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