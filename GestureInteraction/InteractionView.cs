using UnityEngine;
using System.Collections;
using UnityEditor;
using GestureInteraction;
using Leap;
using Leap.Unity;
public class InteractionView : MonoBehaviour {
    
    public EventModel m_EventModel;
	// HitRay interaction operations
    GameObject m_pointer;
    Pointer m_pointer_comp;

	// CameraTransition interaction operations
	GameObject m_camera;
	float pre_speed;

	// Use this for initialization
	void Start () {
        m_EventModel.Navigation_HitRay += initPointer;
        m_EventModel.Navigation_HitRay_Update += updatePointer;
        //m_EventModel.Navigation_HitRay_Close += initPointer;
        // initialize the HitRay: initPointer();
	}
	
	// Update is called once per frame
	void Update () {
        // updatePointer(position, rotation)
        // hit object: m_pointer_comp.hit_out.collider
        // hit position: m_pointer_comp.hit_out.point
	}


	//------------------------HitRay interaction operations---------------------------

	// change it into function-seleciton
	void PointerHit(Hand hand, IFuncType type_in){
		switch (type_in) {
		case Init:
			m_pointer.SetActive(true);
			m_pointer = new GameObject("MyPointer");
			m_pointer.transform.SetParent(transform);
			m_pointer.transform.localPosition = Vector3.zero;
			m_pointer.transform.localRotation = Quaternion.identity;
			// initialize the pointer component
			m_pointer_comp = m_pointer.AddComponent<Pointer>();
			UnityEngine.Object obj = AssetDatabase.LoadMainAssetAtPath("Assets/GestureInteraction/InteractionViewInstance/HitRay/MyMaterials/ArcArrows.mat");
			Material goodmat = obj as Material;
			m_pointer_comp.goodTeleMat = goodmat;
			obj = AssetDatabase.LoadMainAssetAtPath("Assets/GestureInteraction/InteractionViewInstance/HitRay/MyMaterials/ArcArrowsBad.mat");
			Material badmat = obj as Material;
			m_pointer_comp.badTeleMat = badmat;
			obj = AssetDatabase.LoadMainAssetAtPath("Assets/GestureInteraction/InteractionViewInstance/HitRay/MyMaterials/prebs/TeleportHighlightExample.prefab");
			GameObject telhigh = obj as GameObject;
			m_pointer_comp.teleportHighlight = telhigh;
			break;
		case Update:
			m_pointer.transform.position = GestureMethods.toVec3(hand.Fingers[1].bones[3].NextJoint);
			m_pointer.transform.rotation = UnityQuaternionExtension.ToQuaternion(hand.Fingers[1].bones[3].Rotation); 
			break;
		case Close:
			m_pointer.SetActive(false);
			break;
		default:
			break;
		}
	}

    void initPointer(Hand hand, IFuncType type_in)
    {
        m_pointer = new GameObject("MyPointer");
        m_pointer.transform.SetParent(transform);
        m_pointer.transform.localPosition = Vector3.zero;
        m_pointer.transform.localRotation = Quaternion.identity;
        // initialize the pointer component
        m_pointer_comp = m_pointer.AddComponent<Pointer>();
        UnityEngine.Object obj = AssetDatabase.LoadMainAssetAtPath("Assets/GestureInteraction/InteractionViewInstance/HitRay/MyMaterials/ArcArrows.mat");
        Material goodmat = obj as Material;
        m_pointer_comp.goodTeleMat = goodmat;
        obj = AssetDatabase.LoadMainAssetAtPath("Assets/GestureInteraction/InteractionViewInstance/HitRay/MyMaterials/ArcArrowsBad.mat");
        Material badmat = obj as Material;
        m_pointer_comp.badTeleMat = badmat;
        obj = AssetDatabase.LoadMainAssetAtPath("Assets/GestureInteraction/InteractionViewInstance/HitRay/MyMaterials/prebs/TeleportHighlightExample.prefab");
        GameObject telhigh = obj as GameObject;
        m_pointer_comp.teleportHighlight = telhigh;
    }

    void enablePointer()
    {
        m_pointer.SetActive(true);
    }

    void unablePointer()
    {
        m_pointer.SetActive(false);
    }

    void updatePointer(Hand hand)
    {
        m_pointer.transform.position = GestureMethods.toVec3(hand.Fingers[1].bones[3].NextJoint);
        m_pointer.transform.rotation = UnityQuaternionExtension.ToQuaternion(hand.Fingers[1].bones[3].Rotation); 
    }
	//------------------------HitRay interaction operations---------------------------

	//--------------------CameraTransition interaction operations---------------------
	void CameraTransition(float speed_in, Vector3 trans_in, IFuncType type_in){
		switch (type_in) {
		case Init:
			m_camera = GameObject.Find ("Camera (eye)");
			pre_speed = 0;
			break;
		case Update:
			if (m_camera != null) {
				// never change position in height
				trans_in.y = 0;
				// calculate mean changing speed
				int speed_cap_num = 20;
				float mean_s = (speed_in - pre_speed) / speed_cap_num;
				// uniform acceleration on speed
				for (int i = 0; i < speed_cap_num; i++) {
					m_camera.transform.position +=(pre_speed + i*mean_s) * trans_in;
				}
				pre_speed = trans_in;
			}
			break;
		case Close:
			m_camera = null;
			break;
		default:
			break;
		}
	}
	//--------------------CameraTransition interaction operations---------------------

    //---------------------------------------------------
    //---------------------------------------------------

}