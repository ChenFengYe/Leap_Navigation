using UnityEngine;
using System.Collections;
using GestureInteraction;
using Leap;
using Leap.Unity;
using System.Collections.Generic;

public class GestureInteractionController : MonoBehaviour {

    public  RemoteFrameServer        m_RemoteHandServer;
    public  LeapServiceProvider      m_LocalHandServer;
    public  EventModel               m_EventModel;               // 事件定义
    public  InteractionView          m_InteractionView;          // 交互可视化
    private List<Frame>              m_FrameList;
    private Frame                    frame_;
    public  m_Hands                  hands_;

    private m_EventType              cureentEventType_;

	// Use this for initialization
	void Start () {
        
        // Init Hands
        hands_ = new m_Hands();
        hands_.hand_L = new Hand();
        hands_.hand_R = new Hand();
        hands_.empty = true;
	}
	
	// Update is called once per frame
	void Update () {
        
        // Updata Frame
        UpdateAndMergeFrame();

        // Check Current Event Types
        if (!hands_.empty)
        {
            m_EventModel.CheckCurrentEventType(hands_,out cureentEventType_);
        }
	}

    private void UpdateAndMergeFrame()
    {
        // Check Hands In Remote and Local Server
        if (m_RemoteHandServer.RemoteFrame.Hands.Count == 0 
            && m_LocalHandServer.CurrentFixedFrame.Hands.Count == 0)
        {
            ClearHandsInFrame();
            return;
        }

        for (int i = 0; i < m_LocalHandServer.CurrentFixedFrame.Hands.Count; i++)
        {
            if (m_LocalHandServer.CurrentFixedFrame.Hands[i].IsLeft)
            {
                hands_.hand_R.CopyFrom(m_LocalHandServer.CurrentFixedFrame.Hands[i]);
                hands_.empty = false;
                break;
            }
            // Check If Local Server has no left hand
            if (i == m_LocalHandServer.CurrentFixedFrame.Hands.Count - 1)
            {
                ClearHandsInFrame();
                return;
            }
        }

        for (int i = 0; i < m_RemoteHandServer.RemoteFrame.Hands.Count; i++)
        {
            if (!m_RemoteHandServer.RemoteFrame.Hands[i].IsLeft)
            {
                hands_.hand_L.CopyFrom(m_RemoteHandServer.RemoteFrame.Hands[i]);
                hands_.empty = false;
                break;
            }
            // Check If Remote Server has no right hand
            //if (i == m_RemoteHandServer.RemoteFrame.Hands.Count - 1)
            //{
            //    ClearHandsInFrame();
            //    return;
            //}
        }
    }

    private void ClearHandsInFrame()
    {
        Hand emptyHand = new Hand();
        hands_.hand_L.CopyFrom(emptyHand);
        hands_.hand_R.CopyFrom(emptyHand);
        hands_.empty = true;
    }
}
