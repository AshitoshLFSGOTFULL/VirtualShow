using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class TriggerHelper : MonoBehaviour
{
    public UnityEvent onTriggerEnter;
    public UnityEvent onTriggerExit;
    public Animator m_animator;
    public Button btn;
    public float m_distance;
    public float m_Threshhold;
    public bool isInside = false;
    public bool isPressed = false;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && isInside && !isPressed)
        {
            btn.onClick.Invoke();
            isPressed = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // This will now fire whenever ANY Rigidbody/Collider enters the trigger
        onTriggerEnter?.Invoke();
        isInside = true;
    }

    private void OnTriggerStay(Collider other)
    {
        m_distance = Vector3.Distance(gameObject.transform.position,other.transform.position);
        if(m_distance < m_Threshhold)
        {
            if (!m_animator.GetBool("Form")) m_animator.SetBool("Form",true);
        }
        else
        {
            if (m_animator.GetBool("Form")) m_animator.SetBool("Form", false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // This will now fire whenever ANY Rigidbody/Collider exits the trigger
        onTriggerExit?.Invoke();
        isInside = false;
        isPressed = false;
        m_distance = 0;
    }
}