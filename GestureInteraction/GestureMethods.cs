using UnityEngine;
using System.Collections;
using System;
using Leap;

namespace GestureInteraction
{
    public enum m_EventType
    {
        // Null
        NoAction = 0,

        // Navigation
        StrollingWithFingerDirection = 1,

        // Selection

        // Manipulation
    }

    public struct m_Event
    {
        Int64 ID;
        m_EventType eventType;

    }

    public struct m_Hands
    {
        public Hand hand_L;
        public Hand hand_R;
        public bool empty;
    }
}

public class GestureMethods : MonoBehaviour {

    
    /* This Function is used to caculate Averange Direction of
     *  Five Fingers*/
    static Vector3 GetFingersDirection(Leap.Hand hand)
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
