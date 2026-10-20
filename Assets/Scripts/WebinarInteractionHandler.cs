using System;
using UnityEngine;
using UnityEngine.UI;
public class WebinarInteractionHandler : MonoBehaviour
{
    [SerializeField] private int index;
    [SerializeField] Button m_InteractionButton;
    [SerializeField] Text m_InteractionText;

    private string m_WeniarName;
    private string m_WeniarUrl;

    private void Awake()
    {
        m_InteractionButton.onClick.AddListener(OpenWebinar);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        WebinarExtractedData l_data = GameManager.s_Instance.GetWebinarExtractedData(index);
        if (l_data != null)
        {
            m_WeniarName = l_data.m_WebinarName;
            m_WeniarUrl = l_data.m_WebinarUrl;
            
            m_InteractionText.text = m_WeniarName;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OpenWebinar()
    {
        Application.OpenURL(m_WeniarUrl);
    }
   
}
