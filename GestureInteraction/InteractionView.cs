using UnityEngine;
using System.Collections;
using UnityEditor;
using GestureInteraction;
using Leap;
using Leap.Unity;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Distributions;
using System.Collections.Generic;

public class InteractionView : MonoBehaviour
{

    public EventModel m_EventModel;
    // HitRay interaction operations
    GameObject m_pointer;
    Pointer m_pointer_comp;

	// Selection
	// selection with highlight
	public bool multiMode;
	GameObject pre_selected;
	List<GameObject> objSelected;

    // CameraTransition interaction operations
    public GameObject m_camera;
    float pre_speed;

    // Manipulation operations
    GameObject m_orthBall;
    public GameObject m_object;

    Vector3 originDirec = new Vector3();
    Vector3 nextDirec = new Vector3();
    Quaternion originQ = new Quaternion();
    Quaternion nextQ = new Quaternion();
    int orginRotationAxis = -1;      // 0 x   1 y  2 z
    // Use this for initialization
    void Start()
    {
        m_EventModel.Action_Navigation_HitRay += PointerHit;
        m_EventModel.Action_Selection_Single += PointerHit;

        m_EventModel.Action_Navigation_Stroll += CameraTransition;
        //m_EventModel.Navigation_HitRay_Close += initPointer;
        m_EventModel.Action_Manipulation_Rotation += RotateObject;
        // initialize the HitRay: initPointer();
    }

    // Update is called once per frame
    void Update()
    {
        // updatePointer(position, rotation)
        // hit object: m_pointer_comp.hit_out.collider
        // hit position: m_pointer_comp.hit_out.point

    }


    //------------------------HitRay interaction operations---------------------------

    // change it into function-seleciton
    void RayHit_Navigation(Hand hand, HitBall m_ControllBall_R, IFuncType type_in)
    {
        switch (type_in)
        {

            case IFuncType.Init:
                if(m_pointer != null)
                {
                    m_pointer.SetActive(true);
                    m_pointer_comp.EnableTeleport();
                }
                else
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
                    obj = AssetDatabase.LoadMainAssetAtPath("Assets/GestureInteraction/InteractionViewInstance/HitRay/MyMaterials/TeleportHighlight.prefab");
                    GameObject telhigh = obj as GameObject;
					          m_pointer_comp.teleportHighlight = telhigh;
					          m_pointer_comp.pointerType = Pointer.PointerType.Parabola;
                }
                break;

            case IFuncType.Update:
                m_pointer.transform.position = m_ControllBall_R.transform.position;
                Debug.Log(m_ControllBall_R.transform.position);
                //m_pointer.transform.position = GMS.toVec3(hand.Fingers[1].bones[3].NextJoint);
                m_pointer.transform.rotation = Quaternion.FromToRotation(Vector3.forward, m_ControllBall_R.direc.normalized);
                //m_pointer.transform.rotation = UnityQuaternionExtension.ToQuaternion(hand.Fingers[1].bones[3].Rotation);
                break;

            case IFuncType.Close:
                if(m_pointer != null)
                {
                    m_pointer.SetActive(false);
                    m_pointer_comp.DisableTeleport();
					          // move current camera position
                    Vector3 cur_pos = m_camera.transform.position;
					          m_camera.transform.position = m_pointer_comp.hit_out.point;
                              m_camera.transform.position.Set(m_camera.transform.position.x, cur_pos.y, m_camera.transform.position.z);
                }
                break;

            default:
                break;
        }
    }
    //------------------------HitRay interaction operations---------------------------


    void RayHit_Selection(Hand hand, HitBall m_ControllBall_R, IFuncType type_in)
    {
        switch (type_in)
        {

            case IFuncType.Init:
                if (m_pointer != null)
                {
                    m_pointer.SetActive(true);
                    m_pointer_comp.EnableTeleport();
					// initialize selection
					GameObject m_camera = GameObject.Find("Main Camera");
					m_camera.AddComponent<HighlightingEffect>();
					objSelected = new List<GameObject>();
					multiMode = true;
					pre_selected = null;
                }
                else
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
                    obj = AssetDatabase.LoadMainAssetAtPath("Assets/GestureInteraction/InteractionViewInstance/HitRay/MyMaterials/TeleportHighlight.prefab");
                    GameObject telhigh = obj as GameObject;
                    m_pointer_comp.teleportHighlight = telhigh;
					m_pointer_comp.pointerType = Pointer.PointerType.Line;
                }
                break;

            case IFuncType.Update:
                m_pointer.transform.position = m_ControllBall_R.transform.position;
                //m_pointer.transform.position = GMS.toVec3(hand.Fingers[1].bones[3].NextJoint);
                m_pointer.transform.rotation = Quaternion.FromToRotation(Vector3.forward, m_ControllBall_R.direc);
                //m_pointer.transform.rotation = UnityQuaternionExtension.ToQuaternion(hand.Fingers[1].bones[3].Rotation);
				// selection
				// #1: add or change object(s) selected 
				if (m_pointer_comp.hit_out.collider != null)
				{
					GameObject cur_hit_obj = m_pointer_comp.hit_out.collider.gameObject;

					if (cur_hit_obj == pre_selected)
						return;

					if (cur_hit_obj.GetComponent<SpectrumController>() == null)
					{
						// if hit object is not selected before
						if (!multiMode)
						{
							removeAllHighlighting();
							objSelected.Clear();
						}
						objSelected.Add(cur_hit_obj);
						cur_hit_obj.AddComponent<SpectrumController>();
					}
					else
					{
						// if hit object is selected before, then we cancel the selection on it
						// ToDo: it should compared with the last obj. Since in continue-selection, one object can be hit constantly.
						if (objSelected.Count > 1)
						{
							if (cur_hit_obj != pre_selected)
							{
								removeHighlighting(cur_hit_obj);
								objSelected.Remove(cur_hit_obj);
							}
						}
					}
					pre_selected = cur_hit_obj;
				}
				else
					pre_selected = null;
				
                break;

            case IFuncType.Close:
                if (m_pointer != null)
                {
                    m_pointer.SetActive(false);
                    m_pointer_comp.DisableTeleport();
					removeAllHighlighting();
					objSelected.Clear();
					objSelected = null;
					Destroy(Camera.main.GetComponent<HighlightingEffect>());
                }
                break;

            default:
                break;
        }
    }

	void removeAllHighlighting()
	{
		foreach (var obj_item in objSelected)
		{
			removeHighlighting(obj_item);
		}
	}

	void removeHighlighting(GameObject obj_in)
	{
		if (obj_in.GetComponent<SpectrumController>() != null)
		{
			Destroy(obj_in.GetComponent<SpectrumController>());
		}

		if (obj_in.GetComponent<HighlightableObject>() != null)
		{
			Destroy(obj_in.GetComponent<HighlightableObject>());
		}
	}
  
    //--------------------CameraTransition interaction operations---------------------
    void CameraTransition(float angles_in, Vector3 trans_in, Vector3 referAxsis, IFuncType type_in)
    {
        switch (type_in)
        {
            case IFuncType.Init:
                m_camera = GameObject.Find("FPSController_Standard");
                pre_speed = 0;
                break;
            case IFuncType.Update:
                if (m_camera != null)
                {
                    // never change position in height
                    trans_in.y = 0;
                    // calculate mean changing speed
                    float speed_get = calcSpeed(angles_in);
                    speed_get /= 1000;
                    //int speed_cap_num = 20;
                    //float mean_s = (speed_get - pre_speed) / speed_cap_num;
                    //// uniform acceleration on speed
                    //for (int i = 0; i < speed_cap_num; i++)
                    //{
                    //    updateRotation(trans_in, referAxsis);
                    //    m_camera.transform.position += (pre_speed + i * mean_s) * trans_in;

                    //}
                    //StartCoroutine();
                    updateCamera(speed_get, trans_in, referAxsis);
                    pre_speed = speed_get;
                }
                break;
            case IFuncType.Close:
                m_camera = null;
                break;
            default:
                break;
        }
    }


    void updateCamera(float speed_in, Vector3 trans_in, Vector3 referAxsis)
    {

        int speed_cap_num = 20;
        float mean_s = (speed_in - pre_speed) / speed_cap_num;
        // uniform acceleration on speed
        for (int i = 0; i < speed_cap_num; i++)
        {
            updateRotation(trans_in, referAxsis);
            //m_camera.transform.position += (pre_speed + i * mean_s) * trans_in;
            //m_camera.GetComponent<Rigidbody>().MovePosition((pre_speed + i * mean_s) * trans_in);
            //yield return new WaitForSeconds(0.005f);
        }
    }
    public void updateRotation(Vector3 trans_in, Vector3 referAxsis)
    {
        float sensitivetyZ = 2f;
        if (m_camera == null)
        {
            m_camera = GameObject.Find("FPSController_Standard");            
        }
        if (Input.GetAxis("Horizontal") != 0)
        {
            float rotationZ = Input.GetAxis("Horizontal") * sensitivetyZ;
            m_camera.transform.Rotate(0, rotationZ/10, 0);
        }  
        //// rotate with transiting
        //trans_in.y = 0;
        //trans_in.Normalize();
        //Vector3 cur_r = m_camera.transform.rotation.eulerAngles;
        //// z axis is the base line
        //Vector3 r_cap_t = Quaternion.FromToRotation(referAxsis, trans_in).eulerAngles;
        //float angle_fixed = r_cap_t.y > 180 ? r_cap_t.y - 360 : r_cap_t.y;
        //r_cap_t.Set(0, angle_fixed, 0);
        //r_cap_t /= 1000;
        //r_cap_t += cur_r;
        ////float r_volexity = 0.0f;
        ////Debug.Log("Before: " + r_cap_t.y);
        ////r_cap_t.y = Mathf.SmoothDamp(cur_r.y, r_cap_t.y, ref r_volexity, 0.1f);
        ////Debug.Log("Afetr: " + r_cap_t.y);
        //Quaternion out_r = Quaternion.Euler(r_cap_t);
        //m_camera.transform.rotation = out_r;
    }
    void FixedUpdate()
    {

    }

    float calcSpeed(float angle_in)
    {

        // angle convert to speed
        // angle: 1.2 ~ 3
        // mean can set to 3
        // standard deviation set to 1.2 with 1.2~3 as 1.5theta
        Normal speed_pdf = new Normal(30, 2);
        // input given and get the probability density(PDF)

        double speed_out = speed_pdf.Density(10 * (double)angle_in);
        //Debug.Log("angle_in" + angle_in);
        //Debug.Log("speed_out: " + speed_out);
        return (float)speed_out;
    }
    //--------------------CameraTransition interaction operations---------------------


    //---------------------Manipulation interaction operations------------------------
    void ObjRotation(Transform LMC_in, IFuncType type_in)
    {
        switch (type_in)
        {
            case IFuncType.Init:
                if(m_orthBall == null)
                {
                    m_orthBall = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    //m_orthBall.AddComponent<Rigidbody>(); 
                    // set the name 
                    m_orthBall.name = "Rotation Ball";
                    // ToDo: set suitable material
                    // set the position
                    m_orthBall.transform.SetParent(LMC_in);
                    m_orthBall.transform.localPosition = new Vector3(0, 0.8f, 0);
                    m_orthBall.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                }
                m_orthBall.SetActive(true);
                break;
            case IFuncType.Update:
                if(m_object != null && m_orthBall != null)
                {   
                    // get the Quaternion-rotation
                    Quaternion rotate_get = Quaternion.Euler(0, 10, 0);
                    m_object.transform.rotation *= rotate_get;
                    m_orthBall.transform.rotation *= rotate_get;
                }
                break;
            case IFuncType.Close:
                if (m_orthBall != null)
                    m_orthBall.SetActive(false);
                break;
            default:
                break;
        }
    }
    //---------------------Manipulation interaction operations------------------------
    void RotateObject(Hand hand, HitBall m_ControllBall_L, IFuncType type_in)
    {
        switch (type_in)
        {
            case IFuncType.Init:
                originDirec = m_ControllBall_L.direc;
                originQ.SetFromToRotation(originDirec, originDirec);
                break;
            case IFuncType.Update:


                // check is too close to center
                if (m_ControllBall_L.radius < 0.05)
                {
                    originDirec = m_ControllBall_L.direc;
                    originQ.SetFromToRotation(originDirec, originDirec);
                    return;
                }

                nextDirec = m_ControllBall_L.direc;
                nextQ.SetFromToRotation(originDirec, nextDirec);

                //Debug.Log(Vector3.Dot(originDirec.normalized, nextDirec.normalized));

                // check if two vector is very similar
                if (Vector3.Dot(originDirec.normalized, nextDirec.normalized) > 0.999)
                {
                    return;
                }

                GameObject a = GameObject.Find("CubeTest");
                Quaternion q = Quaternion.Slerp(originQ, nextQ, Time.deltaTime*10);

                LineRender lr = a.GetComponent<LineRender>();
                if (Mathf.Abs(q.x) > Mathf.Abs(q.y) && Mathf.Abs(q.x) > Mathf.Abs(q.z))
                {
                    q.y = 0f; q.z = 0f;
                    lr.DrawXCycline();
                }
                else if (Mathf.Abs(q.y) > Mathf.Abs(q.z))
                {
                    q.x = 0f; q.z = 0f;
                    lr.DrawYCycline();
                }
                else
                {
                    q.x = 0f; q.y = 0f;
                    lr.DrawZCycline();
                }

                //if (orginRotationAxis != CurrentRotationAxis)
                //{
                //    orginRotationAxis = CurrentRotationAxis;
                //    originDirec = nextDirecl
                //}
                a.transform.rotation = q * a.transform.rotation;

                originDirec = nextDirec;
                originQ = nextQ;
                break;
            case IFuncType.Close:
                break;
            default:
                break;
        }
    }

    //---------------------------------------------------
    //---------------------------------------------------
    
    
}

                //Quaternion.SlerpUnclamped(a.transform.rotation, q, Time.deltaTime);
                //Vector3 eularAngles = q.eulerAngles;
