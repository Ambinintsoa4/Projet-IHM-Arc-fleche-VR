using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MouseInput : MonoBehaviour
{
    public Bow m_Bow = null;
    // public Input m_Pose = null;
    //public bool m_PullAction = false;   
    public CustomSphere m_Sphere = null;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        var startPulling = m_Sphere.StartPulling();
        //var startPulling = Input.GetMouseButtonDown(0);
        //var startPulling = Input.GetKeyDown(KeyCode.P);
        if (startPulling)
        {
            m_Sphere.StartTracking();
            Debug.Log($"m_PullingHand ={m_Sphere.gameObject?.transform?.localPosition.z}");

            m_Bow.Pull(m_Sphere.gameObject.transform);
        }


        //if (m_Sphere.TriggerRelease())
        //var release = Input.GetMouseButtonDown(1);
        //var release = Input.GetKeyDown(KeyCode.R);
        if (m_Sphere.TriggerRelease())
        {
            m_Bow.Release();
            m_Sphere.ResetPosition();
        }


    }
}

