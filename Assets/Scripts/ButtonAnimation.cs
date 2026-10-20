using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ButtonAnimation : MonoBehaviour
{
    [Header("Anim component")]
    [SerializeField] RectTransform m_buttonRectransform;
    [SerializeField] float m_Original;
    [SerializeField] float m_Expand;

    [Header("UI")]
    [SerializeField] Text m_Small;
    [SerializeField] Text m_Big;

    [Header("Attrib")]
    [SerializeField] float m_Time;

    Coroutine m_Coroutine;

    private void Start()
    {
        Vector2 newDelta = new Vector2(m_Expand, m_buttonRectransform.sizeDelta.y);
        m_buttonRectransform.sizeDelta = newDelta;
        m_Small.text = "Click to Interact";
    }
    public void ExpandButton()
    {
        if(m_Coroutine != null)
        {
            StopCoroutine(m_Coroutine);
            m_Coroutine = null;
        }

        //m_Coroutine = StartCoroutine(Resize(m_Expand));
    }

    public void ShrinkButton()
    {
        if (m_Coroutine != null)
        {
            StopCoroutine(m_Coroutine);
            m_Coroutine = null;
        }

        //m_Coroutine = StartCoroutine(Resize(m_Original));
    }
    IEnumerator Resize(float target)
    {
        if (target == m_Original) m_Small.text = "[F]";
        while (!Mathf.Approximately(m_buttonRectransform.sizeDelta.x,target))
        {
            float Delta = Mathf.MoveTowards(m_buttonRectransform.sizeDelta.x,target,m_Time/Time.deltaTime);
            Vector2 newDelta = new Vector2(Delta, m_buttonRectransform.sizeDelta.y);
            m_buttonRectransform.sizeDelta = newDelta;
            yield return null;
        }
        if (target == m_Expand) m_Small.text = "[F] to interact";
        
    }


    

}
