using UnityEngine;
using System.Collections;
using UnityEditor;
using GestureInteraction;
using Leap;
using Leap.Unity;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Distributions;

public class InteractionView : MonoBehaviour
{

    public EventModel m_EventModel;
    // HitRay interaction operations
    GameObject m_pointer;
    Pointer m_pointer_comp;

    // CameraTransition interaction operations
    GameObject m_camera;
    float pre_speed;

    // Manipulation operations
    GameObject m_orthBall;
    public GameObject m_object;

    // Use this for initialization
    void Start()
    {
        m_EventModel.Action_Navigation_HitRay += PointerHit;
        m_EventModel.Action_Navigation_Stroll += CameraTransition;
        //m_EventModel.Navigation_HitRay_Close += initPointer;
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
    void PointerHit(Hand hand, IFuncType type_in)
    {
        switch (type_in)
        {

            case IFuncType.Init:
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

            case IFuncType.Update:
                m_pointer.transform.position = GMS.toVec3(hand.Fingers[1].bones[3].NextJoint);
                m_pointer.transform.rotation = UnityQuaternionExtension.ToQuaternion(hand.Fingers[1].bones[3].Rotation);
                break;

            case IFuncType.Close:
                m_pointer.SetActive(false);
                break;

            default:
                break;
        }
    }
    //------------------------HitRay interaction operations---------------------------

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
                    StartCoroutine(updateCamera(speed_get, trans_in, referAxsis));

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


    IEnumerator updateCamera(float speed_in, Vector3 trans_in, Vector3 referAxsis)
    {

        int speed_cap_num = 20;
        float mean_s = (speed_in - pre_speed) / speed_cap_num;
        // uniform acceleration on speed
        for (int i = 0; i < speed_cap_num; i++)
        {
            updateRotation(trans_in, referAxsis);
            m_camera.transform.position += (pre_speed + i * mean_s) * trans_in;
            yield return new WaitForSeconds(0.005f);
        }
    }
    void updateRotation(Vector3 trans_in, Vector3 referAxsis)
    {
        // rotate with transiting
        trans_in.y = 0;
        trans_in.Normalize();
        Vector3 cur_r = m_camera.transform.rotation.eulerAngles;
        // z axis is the base line
        Vector3 r_cap_t = Quaternion.FromToRotation(referAxsis, trans_in).eulerAngles;
        float angle_fixed = r_cap_t.y > 180 ? r_cap_t.y - 360 : r_cap_t.y;
        r_cap_t.Set(0, angle_fixed, 0);
        Quaternion out_r = Quaternion.Euler(cur_r + r_cap_t / 800);
        m_camera.transform.rotation = out_r;
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
        return (float)speed_out * 10;
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



    //---------------------------------------------------
    //---------------------------------------------------

}