using System;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float m_speed = 2000.0f;

    public Transform m_Tip = null;
    private Rigidbody m_Rigidbody = null;

    public bool m_IsStopped = true;
    private Vector3 m_lastPosition = Vector3.zero;

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }
    //private void FixedUpdate()
    private void Update()
    {
        if (m_IsStopped)
            return;

        //Rotate
        m_Rigidbody.MoveRotation(Quaternion.LookRotation(m_Rigidbody.velocity, transform.up));

        //Collision
        if (Physics.Linecast(m_lastPosition, m_Tip.position))
        {
            Stop();

        }
        //store Position
        m_lastPosition = m_Tip.position;
    }

    private void Stop()
    {
        m_IsStopped = true;

        m_Rigidbody.isKinematic = true;
        m_Rigidbody.useGravity = false;

    }
    public void Fire(float pullValue)
    {
        m_IsStopped = false;
        transform.parent = null;

        m_Rigidbody.isKinematic = false;
        m_Rigidbody.useGravity = true;
        m_Rigidbody.AddForce(transform.forward * (pullValue * m_speed));

        Destroy(gameObject, 5.0f);

    }
}

