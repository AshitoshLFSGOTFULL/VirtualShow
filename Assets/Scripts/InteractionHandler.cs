using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;
public class InteractionHandler : MonoBehaviour
{
    [Header("Stall Data")]
    [SerializeField] int StallIndex;

    [Header("Texts")]
    [SerializeField] Text m_CompanyNameText;
    [SerializeField] Text m_DescriptionText;

    [Header("Button Elements")]
    [SerializeField] Button m_PlayButton;
    [SerializeField] Button m_PDFButton;
    [Header("Thumbnail Elements")]
    [SerializeField] RawImage m_PlayButtonThumbnail;
    [SerializeField] RawImage m_PDFButtonThumbnail;
    [Header("Logo Elements")]
    [SerializeField] Material m_SqaureLogoMaterial;
    [SerializeField] Material m_RectangleLogoMaterial;
    [Header("Interaction Settings")]
    [SerializeField] float Threshold = 6;
    [SerializeField] ButtonAnimation m_btnAnim;
    [SerializeField] UnityEvent m_EntryEvent;
    [SerializeField] UnityEvent m_ExitEvent;
    [SerializeField] UnityEvent m_ButtonEvent;
    
    [Header("Jobs")]
    [SerializeField] Button[] m_listingJobs;
    [SerializeField] List<EventJobsData> m_eventJobs;

    string YoutubeLink;
    string YoutubeThumbnailLink;
    string BrochureLink;
    string BrochureThumbnailLink;
    string LogoSquare;
    string LogoRectangle;
    
    
    private void Awake()
    {
        if(m_PlayButton == null || m_PDFButton == null)
        {
            Debug.LogError("Buttons are not assigned in the inspector.");
            return;
        }
        m_PlayButton.onClick.AddListener(YouTubeButton);
        m_PDFButton.onClick.AddListener(PDFButton);
    }

    IEnumerator Start()
    {
        yield return new WaitUntil(() => GameManager.s_Instance.GetDataLoaded());
        EventStallData eventStallData = GameManager.s_Instance.GetEventData(StallIndex);
        if (eventStallData != null)
        {
            if(m_CompanyNameText != null) m_CompanyNameText.text = eventStallData.CompanyName;
            if(m_DescriptionText != null) m_DescriptionText.text = eventStallData.Description;
            if (m_SqaureLogoMaterial != null)
            {
                LogoSquare = eventStallData.LogoRectangle;
                LoadMaterialImage(m_SqaureLogoMaterial, LogoSquare);
            }

            if (m_RectangleLogoMaterial != null)
            {
                LogoRectangle = eventStallData.LogoRectangle;
                LoadMaterialImage(m_RectangleLogoMaterial, LogoRectangle);
            }
            SetLinks(eventStallData.VideoURL, eventStallData.DocumentURL, eventStallData.VideoThumbnailURL, eventStallData.DocumentThumbnailURL);

            if (m_listingJobs.Length > 0)
            {
                for (int i = 0; i < eventStallData.m_Jobs.Count; i++)
                {
                    SetJobsData(eventStallData.m_Jobs[i].jobname, eventStallData.m_Jobs[i].joblink);
                    m_listingJobs[i].gameObject.SetActive(true);
                    m_listingJobs[i].GetComponentInChildren<Text>().text = eventStallData.m_Jobs[i].jobname;
                }
            }
        }
    }
    
    private void YouTubeButton()
    {
        Application.OpenURL(YoutubeLink);
    }

    private void PDFButton()
    {
        Application.OpenURL(BrochureLink);
    }

    public void SetLinks(string youtubeLink, string brochureLink, string youtubeThumbnailLink, string brochureThumbnailLink)
    {
        YoutubeLink = youtubeLink;
        BrochureLink = brochureLink;
        YoutubeThumbnailLink = youtubeThumbnailLink;
        BrochureThumbnailLink = brochureThumbnailLink;
        if(m_PlayButtonThumbnail != null && m_PDFButtonThumbnail != null)
        {
            LoadImage(m_PlayButtonThumbnail, YoutubeThumbnailLink);
            LoadImage(m_PDFButtonThumbnail, BrochureThumbnailLink);
        }
    }

    public void SetJobsData(string jobname, string link)
    {
        EventJobsData jobdata = new EventJobsData();
        jobdata.name = jobname;
        jobdata.jobLink = link;
        m_eventJobs.Add(jobdata);
    }

    private void LoadImage(RawImage img, string url)
    {
        StartCoroutine(LoadImageCoroutine(img, url));
    }

    private IEnumerator LoadImageCoroutine(RawImage img, string url)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                img.texture = texture;
            }
            else
            {
                Debug.LogError($"Failed to load image from {url}: {request.error}");
            }
        }
    }

    private void LoadMaterialImage(Material mat, string url)
    {
        StartCoroutine(LoadMaterialImageCoroutine(mat, url));
    }

    private IEnumerator LoadMaterialImageCoroutine(Material mat, string url)
    {         
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                mat.mainTexture = texture;
            }
            else
            {
                Debug.LogError($"Failed to load image from {url}: {request.error}");
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        m_EntryEvent.Invoke();
       
    }

    private void OnTriggerStay(Collider other)
    {
        float distance = Vector3.Distance(other.transform.position,gameObject.transform.position);
        if(distance < Threshold)
        {
            m_btnAnim.ExpandButton();
        }
        else
        {
            m_btnAnim.ShrinkButton();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            m_ButtonEvent.Invoke();
        }

    }

    private void OnTriggerExit(Collider other)
    {
        m_ExitEvent.Invoke();
    }
}

[Serializable]
public class EventJobsData
{
    public string name;
    public string jobLink;
}
