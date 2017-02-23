using UnityEngine;
using System.Collections;
using System;
using Leap;
using GestureInteraction;
using MathNet.Numerics.Distributions;

namespace GestureInteraction
{
    public enum IEventType
    {
        // Null
        NoAction,                       // no action

        Wait_Stroll,

        Wait_CancelOrScale,             // 注意： 当再次加入Wait Event 时，去 IsWaitEvent 添加该 Wait Event
        Wait_NavigationOrSelectionOrCancel,
        //Wait_ScaletoNoAction,

        CancelAction,                   // cancel all current action to Init all Interaction

        // Navigation
        Navigation_Stroll,
        Navigation_RayHit,
        Navigation_Scaling,                        // may scale scence or object

        // Selection
        Selection_Mutiple,
        Selection_Single,

        // Manipulation
    }

    public enum IFuncType
    {
        Init = 0,
        Update = 1,
        Close = 2,
    }

    public struct m_Event
    {
        Int64 ID;
        IEventType eventType;

    }

    public struct HandPair
    {
        public Hand L;
        public Hand R;
        public bool empty;
    }
}

// GMS means Gesture methods 
public class GMS : MonoBehaviour
{
    static private float HandsCoressThreshold = 0.01f;          // metrix is meter
    static private float GraspThreshold = 0.8f;                // for detecting this hand is making a fist or not
    static private float GraspThreshold2 = 0.5f;                // for detecting this hand is making a fist or not

    /* This Function is used to caculate Averange Direction of
     *  Five Fingers*/
    //--------------------------------------------------------------------------------------------------
    // 计算交互量的函数
    public static Vector3 GetFingersDirection(Leap.Hand hand)
    {
        // Debug Check
        if (hand.Fingers == null || hand.Fingers.Count != 5)
            Debug.Log("Fingers Error: No Fingers or Fingers is not 5!");

        // Data Calculating
        Vector3 FingersDirection = Vector3.zero;
        for (int i = 0; i < hand.Fingers.Count; i++)
        {
            FingersDirection += toVec3(hand.Fingers[i].Direction);
        }
        FingersDirection /= hand.Fingers.Count;
        return FingersDirection;
    }

    public static Vector3 GetIndexFingersDirection(Leap.Hand hand)
    {
        // Debug Check
        if (hand.Fingers == null || hand.Fingers.Count != 5)
            Debug.Log("Fingers Error: No Fingers or Fingers is not 5!");

        // Data Calculating
        Vector3 FingersDirection = Vector3.zero;
        FingersDirection = toVec3(hand.Fingers[1].Direction);
        return FingersDirection;
    }

    public static Quaternion GetCameraRotation(Leap.Hand hand, Vector3 ReferAxis)
    {
        // 使用食指的掌故的方向作为 移动方向
        Vector3 MoveDirection = new Vector3();
        MoveDirection = GMS.toVec3(hand.Fingers[1].bones[0].Direction);

        // 投影到XZ平面
        MoveDirection.y = 0;
        ReferAxis.y = 0;

        // 计算夹角
        Quaternion q = new Quaternion();
        q.SetFromToRotation(ReferAxis, MoveDirection);
        return q;
    }

    public static float GetIndexCurve(Leap.Hand hand)
    {
        float speed;

        // 使用右手食指的弯曲程度表示移动速度
        var metal = toVec3(hand.Fingers[1].bones[0].Direction);
        var proxi = toVec3(hand.Fingers[1].bones[1].Direction);
        var inter = toVec3(hand.Fingers[1].bones[2].Direction);
        var tip = toVec3(hand.Fingers[1].bones[3].Direction);

        var theta1 = Vector3.Dot(metal.normalized, proxi.normalized);
        var theta2 = Vector3.Dot(proxi.normalized, inter.normalized);
        var theta3 = Vector3.Dot(inter.normalized, tip.normalized);

        speed = theta1 + theta2 + theta3;
        // 这个值一般范围为 1.2 —— 3
        return speed;

    }

    // Manipulate Rotation  计算rotation的旋转矩阵
    // 输入 LastTouchVector 为上次的 的旋转量  当整个旋转交互结束时（即手离开球的时候 初始 LastTouchVector=new Vector3(1, 1, 1) ）
    // qBall 即为物体和球的旋转矩阵
    // 算手与球接触点的时候需要用到的公式    Vector3 touchPoint = CurTouchVector*manipulatedBall.scale + manipulatedBall.transformate.position
    public static Quaternion GetManipulationRotation(Hand hand, Transform manipulatedBall, Vector3 LastTouchVector, out Vector3 CurTouchVector)
    {
        Quaternion qBall = new Quaternion();
        CurTouchVector = (manipulatedBall.position - toVec3(hand.PalmPosition)).normalized;
        if (LastTouchVector != new Vector3(1, 1, 1))
	    {
            qBall.SetFromToRotation(LastTouchVector, CurTouchVector);
            LastTouchVector = CurTouchVector;
	    }
        return qBall;
    }

    // Manipulate Rotation 检查手与球有无接触
    public static bool CheckHandIsTouchedBall(Hand hand, Transform manipulatedBall)
    {
        bool IsTouch = false;
        for (int i = 0; i < hand.Fingers.Count; i++)
		{
		    for (int j = 0; j < hand.Fingers[i].bones.Length; j++)
			{
			    float d = Vector3.Distance(
                    toVec3(hand.Fingers[i].bones[j].NextJoint),
                    manipulatedBall.position);
                if (d < manipulatedBall.localScale[0])
	            {
                    IsTouch = true;
	            	break;
	            }
			}
		}
        return IsTouch;
    }

    //--------------------------------------------------------------------------------------------------
    // Fix Data 的函数
    static public bool CheckHandsData(HandPair hands, HandPair LastHands, bool m_OpenFixeFrameData)
    {
        // Check hands are empty to decide fix or hold on all interaction
        if (hands.empty)
        {
            if (m_OpenFixeFrameData)
            {
                //---------------------------
                // fix current hand
                if (!LastHands.empty)
                {
                    FixCurrentHand(hands, LastHands);
                    return true;
                }
                else
                    return false;
                //---------------------------
            }
            else
                return false;
        }
        else
            return true;
    }

    // if there occurs that emtpy hand while last time have hands, Fix hands with this function
    static private void FixCurrentHand(HandPair hands, HandPair LastHands)
    {
        hands.empty = false;
        hands.L.CopyFrom(LastHands.L);
        hands.R.CopyFrom(LastHands.R);
    }

    //--------------------------------------------------------------------------------------------------
    // 检查状态的函数
    static public bool checkFullFist(Hand hand)
    {
        float sum = 0.0f;

        for (int i = 0; i < hand.Fingers.Count; i++)
        {
            var finger = hand.Fingers[i];
            var meta = finger.bones[0].Direction;
            var proxi = finger.bones[1].Direction;
            var inter = finger.bones[2].Direction;
            var dMetaProxi = meta.Dot(proxi);
            var dProxiInter = proxi.Dot(inter);
            sum += dMetaProxi;
            sum += dProxiInter;
        }
        sum = sum / 10;

        if (sum <= GraspThreshold2 && getExtendedFingers_NoThumb(hand) == 0)
            return true;
        else
            return false;
    }

    static public int getExtendedFingers_NoThumb(Hand hand)
    {
        var f = 0;
        // Check Finger is extended except thumb
        for (var i = 1; i < hand.Fingers.Count; i++)
        {
            if (hand.Fingers[i].IsExtended)
            {
                f++;
            }
        }
        return f;
    }
    static public bool CheckHandFist(Hand hand)
    {
        if (hand.GrabStrength > GraspThreshold)
        {
            // Check Finger is extended except thumb
            for (int i = 1; i < hand.Fingers.Count; i++)
                if (hand.Fingers[i].IsExtended)
                    return false;
            return true;
        }
        else
            return false;
    }

    // 双手打开
    static public bool CheckTwoHandsRelaxed(HandPair hands)
    {
        return !CheckHandFist(hands.L) && !CheckHandFist(hands.R) ? true : false;
    }

    // 手握拳

    // 双手握拳
    static public bool CheckHandsAllFist(HandPair hands)
    {
        return CheckHandFist(hands.L) && CheckHandFist(hands.R) ? true : false;
    }

    // 检查两手相向移动
    static public bool CheckHandsMoveCross(HandPair hands, HandPair LastHands)
    {
        if (hands.empty)
            Debug.Log("There may exist an error or this is a pair fixed hands");

        float PalmDistance = hands.L.PalmPosition.DistanceTo(hands.R.PalmPosition);
        float LastPalmDistance = LastHands.L.PalmPosition.DistanceTo(LastHands.R.PalmPosition);
        return LastPalmDistance - PalmDistance > HandsCoressThreshold ? true : false;
    }

    // 检查单指指出
    static public bool CheckOnlyIndex_NoThumb_NoMiddle(Hand hand)
    {
        bool OnlyIndexPointing = false;
        for (int i = 1; i < hand.Fingers.Count; i++)
        {
            // except Middle Finger
            if (i == 2)
                continue;
            if (i == 1) // index finger
            {
                OnlyIndexPointing = GetIndexCurve(hand) > 1.2 ? true : false;
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

    static public Vector3 toVec3(Leap.Vector v)
    {
        Vector3 v_3 = new Vector3(v.x, v.y, v.z);
        return v_3;
    }
}
