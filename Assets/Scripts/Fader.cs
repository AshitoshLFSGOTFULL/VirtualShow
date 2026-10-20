using System.Collections;
using UnityEngine;

public class Fader : MonoBehaviour
{
    [SerializeField] CanvasGroup m_canvasGroup;
    [SerializeField] float m_Timer;
    Coroutine m_couroutine;

    public void FadeIn()
    {
        if (!gameObject.activeSelf) return;
        if(m_couroutine != null)
        {
            StopCoroutine(m_couroutine);
            m_couroutine = null;
        }

        StartCoroutine(Fade(0));
    }

    public void FadeOut()
    {
        if (m_couroutine != null)
        {
            StopCoroutine(m_couroutine);
            m_couroutine = null;
        }

        StartCoroutine(Fade(1));
    }
    IEnumerator Fade(int Target)
    {
        while (!Mathf.Approximately(m_canvasGroup.alpha, Target))
        {
            m_canvasGroup.alpha = Mathf.MoveTowards(m_canvasGroup.alpha, Target, Time.deltaTime / m_Timer);
            yield return null;
        }

        bool state = Target == 0 ? false: true;
        m_canvasGroup.interactable = state;
        m_canvasGroup.blocksRaycasts = state;

        m_couroutine = null;
    }

}
