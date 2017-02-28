using UnityEngine;
using System.Collections;
using GestureInteraction;
using System.Collections.Generic;
using System;
using Leap;

public class EventModel : MonoBehaviour
{

    public GestureInteractionController m_GIController;
    public GameObject                   m_RightLMReference;
    public bool                         m_OpenFixeFrameData = false;
    private List<m_Event>               m_EventList;
    private Int64                       m_EventID;
    private bool                        m_RayHit = false;
    private HandPair                    LastHands;


    // Event
    public event Action<Hand, HitBall, IFuncType>           Action_Navigation_HitRay;              // 命名方式： 交互类型 - 实现交互实例(- 函数功能)
    public event Action<HandPair, IFuncType>                Action_Navigation_Scale;           //
    public event Action<float, Vector3, Vector3,IFuncType>  Action_Navigation_Stroll;    // Speed direction 

    public event Action<Hand, HitBall, IFuncType>           Action_Selection_Single;              // 命名方式： 交互类型 - 实现交互实例(- 函数功能)

    public event Action<Hand, HitBall, IFuncType>           Action_Manipulation_Rotation;
    // Manipulater Event
    public event Action<Vector3> CatchBall;
    public event Action<Vector3> ReleaseBall;

    // constant

    // Use this for initialization
    void Start()
    {
        m_EventList = new List<m_Event>();
        m_EventID = 0;
    }

    public void CheckCurrentEvent(IEventType CurrentEventType, IEventType LastEventType, HandPair hands, 
        HitBall m_ControllBall_L, HitBall m_ControllBall_R)
    {
        switch (CurrentEventType)
        {
            case IEventType.NoAction:
                break;
            case IEventType.Navigation_RayHit:
                if (LastEventType == CurrentEventType)
                {
                    CurrentEventType = m_ControllBall_R.radius < 0.15 ?
                        IEventType.Navigation_RayHit : IEventType.Selection_Single;
                }
                break;
            case IEventType.Selection_Single:
                if (LastEventType == CurrentEventType)
                {
                    CurrentEventType = m_ControllBall_R.radius < 0.15 ?
                        IEventType.Navigation_RayHit : IEventType.Selection_Single;
                }
                break;
            case IEventType.Manipulation_Rotation:
                if (LastEventType == CurrentEventType)
                {
                    CurrentEventType = m_ControllBall_L.radius < 0.15 ?
                        IEventType.Manipulation_Rotation : IEventType.Manipulation_Stroll;
                }
                break;
            case IEventType.Manipulation_Stroll:
                if (LastEventType == CurrentEventType)
                {
                    CurrentEventType = m_ControllBall_L.radius < 0.15 ?
                        IEventType.Manipulation_Rotation : IEventType.Manipulation_Stroll;
                }
                break;
            default:
                break;
        }
    }

    public void DoCurrentEvent(IEventType CurrentEventType, IEventType LastEventType, HandPair hands, 
        HitBall m_ControllBall_L, HitBall m_ControllBall_R)
    {
        // Check hands are empty to decide fix or hold on all interaction
        //if (!GMS.CheckHandsData(hands, LastHands, m_OpenFixeFrameData)) return;

        switch (CurrentEventType)
        {
            case IEventType.NoAction:
                return;
                break;
            case IEventType.CancelAction:
                if (LastEventType != IEventType.NoAction)
                {
                    if (LastEventType == IEventType.Navigation_RayHit)
                    {
                        Action_Navigation_HitRay(hands.R,m_ControllBall_R, IFuncType.Close);
                    }

                    if (LastEventType == IEventType.Selection_Single)
                    {
                        Action_Selection_Single(hands.R, m_ControllBall_R, IFuncType.Close);
                    }
                }
                break;
            case IEventType.Navigation_RayHit:
                if (LastEventType != CurrentEventType)
                {
                    Action_Navigation_HitRay(hands.R, m_ControllBall_R, IFuncType.Init);
                }
                else
                {
                    Action_Navigation_HitRay(hands.R, m_ControllBall_R, IFuncType.Update);
                }
                break;
            case IEventType.Selection_Single:
                if (LastEventType != CurrentEventType)
                {
                    Action_Selection_Single(hands.R, m_ControllBall_R, IFuncType.Init);
                }
                else
                {
                    Action_Selection_Single(hands.R, m_ControllBall_R, IFuncType.Update);
                }
                break;
            case IEventType.Manipulation_Rotation:
                if (LastEventType != CurrentEventType)
                {
                    Action_Manipulation_Rotation(hands.L, m_ControllBall_L, IFuncType.Init);
                }
                else
                {
                    Action_Manipulation_Rotation(hands.L, m_ControllBall_L, IFuncType.Update);
                }
                break;
            default:
                break;
        }

    }

    public void SetCatchBallEvent(string name, Transform v)
    {

    }

    public void Navigation(string name, Vector3 v)
    {
        
    }

    // Check Is Wait Event
    public bool IsWaitEvent(IEventType eT)
    {
        if (eT == IEventType.Wait_NavigationOrSelectionOrCancel ||
            eT == IEventType.Wait_CancelOrScale)
            return true;
        else
            return false;
    }
}
    //public IEventType UpdateCurrentEvent(IEventType CurrentEventType, IEventType LastEventType, HandPair hands)
    //{
    //    // Check hands are empty to decide fix or hold on all interaction
    //    if (!GMS.CheckHandsData(hands, LastHands, m_OpenFixeFrameData))
    //        return IEventType.NoAction;

    //    // If is wait event ,just wait.
    //    if (IsWaitEvent(CurrentEventType))
    //    {
    //        return CurrentEventType;
    //    }
        
    //    Vector3 MoveDirection = new Vector3();
    //    MoveDirection = GMS.toVec3(hands.R.Fingers[1].bones[0].Direction);

    //    //-------------------------------------------------------------
    //    // Debug part

    //    //Debug.Log(GMS.GetIndexCurve(hands.R));
    //    //Debug.Log(GMS.checkFullFist(hands.L));

    //    //-------------------------------------------------------------

    //    switch (CurrentEventType)
    //    {
    //        case IEventType.NoAction:
    //            //if (GMS.CheckTwoHandsRelaxed(hands))
    //            //    return IEventType.NoAction;

    //            if (GMS.CheckOnlyIndex_NoThumb_NoMiddle(hands.R) 
    //                && !GMS.checkFullFist(hands.L))
    //            {
    //                m_GIController.WaitToCheck();
    //                return IEventType.Wait_Stroll;
    //            }
    //            //if (GMS.CheckHandFist(hands.L))
    //            //{
    //            //    m_GIController.WaitToCheck();
    //            //    return IEventType.Wait_NavigationOrSelectionOrCancel;
    //            //}
    //            return IEventType.NoAction;
    //            break;
    //        case IEventType.CancelAction:
    //            // Do cancel Event

    //            //------//

    //            // then 
    //            return IEventType.NoAction;
    //            break;
    //        //-----------------------------------------------------------------------------------------
    //        // Navigation
    //        case IEventType.Navigation_Scaling:
    //            // Check scale event is over
    //            if (GMS.CheckHandsAllFist(hands))
    //            {
    //                if (LastEventType != IEventType.Navigation_Scaling)
    //                    Action_Navigation_Scale(hands, IFuncType.Init);
    //                else
    //                    Action_Navigation_Scale(hands, IFuncType.Update);
    //                return CurrentEventType;
    //            }
    //            else  // CheckTwoHandsRelaxed(hands)
    //            {
    //                Action_Navigation_Scale(hands, IFuncType.Close);
    //                return IEventType.NoAction;
    //            }
    //            break;

    //        case IEventType.Navigation_RayHit:
    //            if (LastEventType != CurrentEventType)
    //            {
    //                Action_Navigation_HitRay(hands.R, IFuncType.Init);
    //                return IEventType.Navigation_RayHit;
    //            }
    //            else if (GMS.CheckHandFist(hands.L))
    //            {
    //                Action_Navigation_HitRay(hands.R, IFuncType.Update);
    //                return IEventType.Navigation_RayHit;
    //            }
    //            else
    //            {
    //                Action_Navigation_HitRay(hands.R, IFuncType.Close);
    //                return IEventType.NoAction;
    //            }
    //            break;

    //        case IEventType.Navigation_Stroll:
    //            if (LastEventType != CurrentEventType)
    //            {
    //                Action_Navigation_Stroll(
    //                    GMS.GetIndexCurve(hands.R),
    //                    //GMS.GetFingersDirection(hands.R),
    //                    // 使用食指的掌故的方向作为 移动方向
    //                    GMS.toVec3(hands.R.Fingers[1].bones[0].Direction),
    //                    m_RightLMReference.transform.forward,
    //                    IFuncType.Init);
    //                return CurrentEventType;                            // Continue
    //            }
    //            else if (GMS.CheckOnlyIndex_NoThumb_NoMiddle(hands.R))
    //            {
    //                Action_Navigation_Stroll(
    //                    GMS.GetIndexCurve(hands.R),
    //                    //GMS.GetFingersDirection(hands.R),
    //                    // 使用食指的掌故的方向作为 移动方向
    //                    GMS.toVec3(hands.R.Fingers[1].bones[0].Direction),
    //                    m_RightLMReference.transform.forward,
    //                    IFuncType.Update);
    //                return CurrentEventType;                            // Continue
    //            }
    //            else
    //            {
    //                Action_Navigation_Stroll(
    //                    GMS.GetIndexCurve(hands.R),
    //                    //GMS.GetFingersDirection(hands.R),
    //                    // 使用食指的掌故的方向作为 移动方向
    //                    GMS.toVec3(hands.R.Fingers[1].bones[0].Direction),
    //                    m_RightLMReference.transform.forward,
    //                    IFuncType.Close);                               // Close
    //                return IEventType.NoAction;
    //            }
    //            break;
    //        case IEventType.Selection_Mutiple:
    //            if (LastEventType != CurrentEventType)
    //            {
                    
    //            }
    //            break;
    //        default:
    //            Debug.Log("Error: This Event is not achieved! ");
    //            return IEventType.CancelAction;
    //            break;
    //    }
    //    // Change Frame
    //    Debug.Log("Error: Huge Bug 2017.2.20!!!!");
    //    return IEventType.CancelAction;
    //}

    //public IEventType CheckWaitEvent(IEventType CurrentEventType, HandPair hands)
    //{
    //    if (!GMS.CheckHandsData(hands, LastHands, m_OpenFixeFrameData))
    //        return IEventType.CancelAction;

    //    if (!IsWaitEvent(CurrentEventType))
    //    {
    //        Debug.Log("Error: Current Event is not a Wait Event!");
    //    }

    //    switch (CurrentEventType)
    //    {
    //        case IEventType.Wait_Stroll:
    //            if (GMS.CheckOnlyIndex_NoThumb_NoMiddle(hands.R)
    //            && !GMS.checkFullFist(hands.L))
    //            //if(GMS.checkFullFist(hands.L))
    //            {
    //                return IEventType.Navigation_Stroll;
    //            }
    //            return IEventType.NoAction;
    //            break;
    //        case IEventType.Wait_CancelOrScale:
    //            // 双手握手
    //            if (GMS.CheckHandsAllFist(hands))
    //                // 手相向移动了没？ scale : wait
    //                return GMS.CheckHandsMoveCross(hands, LastHands) ?
    //                    IEventType.Navigation_Scaling
    //                    : IEventType.Wait_CancelOrScale;

    //            // 双手张开
    //            else if (GMS.CheckTwoHandsRelaxed(hands))
    //                return IEventType.CancelAction;

    //            // 一手张开一手握拳
    //            else
    //                return IEventType.Wait_CancelOrScale;

    //            break;
    //        case IEventType.Wait_NavigationOrSelectionOrCancel:
    //            if (GMS.CheckOnlyIndex_NoThumb_NoMiddle(hands.R))
    //                 return IEventType.Navigation_RayHit;
    //                 //return IEventType.Selection_Mutiple;
    //            else
    //            {
    //                // check Index is pointer or not
    //                if (GMS.CheckHandFist(hands.R))
    //                    return IEventType.Wait_CancelOrScale;
    //                else
    //                    return IEventType.Navigation_Stroll;
    //            }
    //            break;
    //        //case IEventType.Wait_ScaletoNoAction:
    //        //    {
    //        //        if (GestureMethods.CheckTwoHandsRelaxed)
    //        //        {

    //        //        }
    //        //        return 
    //        //        Wait_ScaletoNoAction
    //        //        break;
    //        //    }
    //        default:
    //            Debug.Log("Error: No Wait Event in WaitEvent Detect function!");
    //            return IEventType.CancelAction;
    //            break;
    //    }
    //}

    // Add Event
    //--------------------------------------------------------------------------------------------------
    //protected void HitRay_Close(Hand hand)
    //{
    //    if (Navigation_HitRay_Close != null)
    //    {
    //        Navigation_HitRay_Close(hand);
    //    }
    //}