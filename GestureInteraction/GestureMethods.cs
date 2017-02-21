using UnityEngine;
using System.Collections;
using System;
using Leap;

namespace GestureInteraction
{
    public enum IEventType
    {
        // Null
        NoAction,                       // no action

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

public class GestureMethods : MonoBehaviour {

    
    /* This Function is used to caculate Averange Direction of
     *  Five Fingers*/
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

    public static Vector3 toVec3(Leap.Vector v)
    {
        Vector3 v_3 = new Vector3(v.x, v.y, v.z);
        return v_3;
    }
}
