using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GestureInteraction;

public class HitBall : MonoBehaviour {

    public EventModel m_EventModel;
    public InteractionView m_IV;
    public GestureInteractionController m_GIC;
    public bool IsLeft;

    private bool IsIndexTouched = false;
    private bool IsThumbTouched = false;

    GameObject IndexTip;
    GameObject ThumbTip;

    public Vector3 originPosi = new Vector3();
    public Vector3 curPosi = new Vector3();
    public float radius;
    public Vector3 direc = new Vector3();

    private bool IsCatchBall = false;
	// Use this for initialization
	void Start () {
		
	}
	
    void Update()
    {

    }

	// Update is called once per frame
	void FixedUpdate () {


        if (IsIndexTouched && IsThumbTouched)
        {
            // Catch Ball
            if (!IsCatchBall)
            {
                IsCatchBall = true;
                // right
                if (!IsLeft)
                {
                    m_GIC.SetEventType(IEventType.Navigation_RayHit);
                }

                // Left 
                else
                {
                    m_GIC.SetEventType(IEventType.Manipulation_Rotation);
                }
	        }
            UpdateBallPosition();
        }

        if (!IsIndexTouched || !IsThumbTouched)
        {
            if (IsCatchBall)
	        {
	        	IsCatchBall = false;
                m_GIC.SetEventType(IEventType.CancelAction);
	        }
            transform.position = transform.parent.position;
        }

        UpdateOriginPosiAndRadius();
	}

    private void UpdateOriginPosiAndRadius()
    {
        originPosi = transform.parent.position;
        curPosi = transform.position;
        radius = Vector3.Distance(originPosi, curPosi);
        direc = originPosi - curPosi;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "bone3" &&
            other.gameObject.transform.parent.gameObject.name == "index")
        {
            IsIndexTouched = true;
            IndexTip = other.gameObject;
        }

        if (other.gameObject.name == "bone3" &&
            other.gameObject.transform.parent.gameObject.name == "thumb")
        {
            IsThumbTouched = true;
            ThumbTip = other.gameObject;
        }
    }

    void UpdateBallPosition()
    {
        transform.position = 0.5f * ((IndexTip.transform.position + 0.02f * IndexTip.transform.forward) +
            (ThumbTip.transform.position + 0.02f * ThumbTip.transform.forward));
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "bone3" &&
            other.gameObject.transform.parent.gameObject.name == "index")
        {
            IsIndexTouched = false;
        }

        if (other.gameObject.name == "bone3" &&
            other.gameObject.transform.parent.gameObject.name == "thumb")
        {
            IsIndexTouched = false;
        }
    }

}
