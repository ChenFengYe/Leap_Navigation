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
    public  HandPair                  hands_;

    private IEventType              currentEventType_;
    private IEventType              lastEventType_;

    private System.Threading.Timer m_timer;
	// Use this for initialization
	void Start () {
        
        // Init Hands
        hands_ = new HandPair();
        hands_.L = new Hand();
        hands_.R = new Hand();
        hands_.empty = true;
        
        // Init EventType
        lastEventType_ = IEventType.NoAction;
        currentEventType_ = IEventType.NoAction;
	}
	
	// Update is called once per frame
	void Update () {
        
        // Updata Frame
        UpdateAndMergeFrame();
        //Debug.Log(currentEventType_);

        // Check Current Event Types
        currentEventType_ = m_EventModel.UpdateCurrentEvent(currentEventType_, lastEventType_, hands_);

        // Change Frame
        lastEventType_ = currentEventType_;
	}

    private void UpdateAndMergeFrame()
    {
        // Check Hands In Remote and Local Server
        if (m_RemoteHandServer.RemoteFrame.Hands.Count == 0 
        || m_LocalHandServer.CurrentFixedFrame.Hands.Count == 0)
        {
            ClearHandsInFrame();
            return;
        }

        // Update Right hand, if no Right hand clear all hands
        for (int i = 0; i < m_LocalHandServer.CurrentFixedFrame.Hands.Count; i++)
        {
            if (!m_LocalHandServer.CurrentFixedFrame.Hands[i].IsLeft)
            {
                hands_.R.CopyFrom(m_LocalHandServer.CurrentFixedFrame.Hands[i]);
                hands_.empty = false;
                break;
            }
            // Check If Local Server has no right hand
            if (i == m_LocalHandServer.CurrentFixedFrame.Hands.Count - 1)
            {
                ClearHandsInFrame();
                return;
            }
        }

        // Update Left hand, if no left hand clear all hands
        for (int i = 0; i < m_RemoteHandServer.RemoteFrame.Hands.Count; i++)
        {
            if (m_RemoteHandServer.RemoteFrame.Hands[i].IsLeft)
            {
                hands_.L.CopyFrom(m_RemoteHandServer.RemoteFrame.Hands[i]);
                hands_.empty = false;
                break;
            }
            // Check If Remote Server has no left hand
            if (i == m_RemoteHandServer.RemoteFrame.Hands.Count - 1)
            {
                ClearHandsInFrame();
                return;
            }
        }
    }

    private void ClearHandsInFrame()
    {
        Hand emptyHand = new Hand();
        hands_.L.CopyFrom(emptyHand);
        hands_.R.CopyFrom(emptyHand);
        hands_.empty = true;
    }

    void InvokeCheckWaitEvent(object a)
    {
        m_timer.Dispose();// stop waiting and distroy the timer.

        IEventType eT = m_EventModel.CheckWaitEvent(currentEventType_, hands_);

        // If it still is a wait Event, Keep waitting
        if (m_EventModel.IsWaitEvent(eT))
        {
            WaitToCheck();
            lastEventType_ = eT;
        }

        currentEventType_ = eT;
    }

    public void WaitToCheck()
    {
        m_timer = new System.Threading.Timer(
            new System.Threading.TimerCallback(InvokeCheckWaitEvent), null,
            0, 1000);//1S定时器  
    }
}
