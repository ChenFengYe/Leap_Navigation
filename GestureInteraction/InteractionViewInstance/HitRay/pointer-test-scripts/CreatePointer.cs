using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CreatePointer : MonoBehaviour {
    GameObject m_pointer;
    Pointer m_pointer_comp;
	// Use this for initialization
	void Start () {
        initPointer();
	}
	
	// Update is called once per frame
	void Update () {
        // updatePointer(position, rotation)
        // hit object: m_pointer_comp.hit_out.collider
        // hit position: m_pointer_comp.hit_out.point
	}

    void initPointer()
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

    void enablePointer(){
        m_pointer.SetActive(true);
    }

    void unablePointer(){
        m_pointer.SetActive(false);
    }

    void updatePointer(Vector3 position_in, Quaternion rotation_in)
    {
        m_pointer.transform.position = position_in;
        m_pointer.transform.rotation = rotation_in;
    }
}
