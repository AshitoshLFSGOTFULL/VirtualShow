using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] VirtualEventSession.LoginResponse m_LoginResponse;
    [SerializeField] List<Stall> m_Stalls;
    [SerializeField] private List<WebinarExtractedData> m_WebinarExtractedData;
    [SerializeField] List<EventStallData> m_Events;
    [SerializeField] bool IsDataLoaded = false;
    [SerializeField] bool IsStallsDataLoaded = false;
    public static GameManager s_Instance;
    private void Awake()
    {
        if(s_Instance == null)
        {
            s_Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
    }

    private void Start()
    {
        SceneFader fader = FindFirstObjectByType<SceneFader>();
        if (fader != null)
        {
            fader.FadeImmidietely();
            
        }
    }

    public void SetLoginResponse(VirtualEventSession.LoginData data)
    {
        m_LoginResponse.data = data;
    }

    public void SetEventData(string companyName, string description, string docUrl, string videoUrl, string docThumbnaiUrl, string videoThumbnailUrl,string logoSquare,string logoRectangle,Job[] jobs)
    {
        EventStallData eventData = new EventStallData
        {
            CompanyName = companyName,
            Description = description,
            DocumentURL = docUrl,
            VideoURL = videoUrl,
            DocumentThumbnailURL = docThumbnaiUrl,
            VideoThumbnailURL = videoThumbnailUrl,
            LogoSquare = logoSquare,
            LogoRectangle = logoRectangle
        };
        
        eventData.SetJobs(jobs);

        m_Events.Add(eventData);
    }

    public EventStallData GetEventData(int index)
    {
        Debug.Log($"Event data count: {m_Events.Count}, requested index: {index}");
        if (m_Events.Count >= 0 && index < m_Events.Count)
        {
            return m_Events[index];
        }
        else
        {
            Debug.LogWarning("No event data available.");
            return null;
        }
    }

    public void SetDataLoaded(bool isLoaded)
    {
        IsDataLoaded = isLoaded;
    }
    public bool GetDataLoaded()
    {
        return IsDataLoaded;
    }

    public bool GetStallsDataLoaded()
    {
        return IsStallsDataLoaded;
    }

    public List<Stall> GetStalls()
    {
        return m_Stalls;
    }

    public void SetStallData(string stallName, bool active)
    {
        Stall l_stall = new Stall();
        l_stall.m_StallData = new StallData
        {
            m_stallName = stallName,
            IsActive = active
        };
        
        m_Stalls.Add(l_stall);
    }

    public void SetWebinaData(string webinaName, string webinaUrl)
    {
        WebinarExtractedData webinarExtractedData = new WebinarExtractedData();
        webinarExtractedData.m_WebinarName = webinaName;
        webinarExtractedData.m_WebinarUrl = webinaUrl;
        m_WebinarExtractedData.Add(webinarExtractedData);
    }

    public WebinarExtractedData GetWebinarExtractedData(int index)
    {
        if(index < m_WebinarExtractedData.Count)  return m_WebinarExtractedData[index];
        return null;
    }
    
    public void LoadScene(int sceneIndex)
    {
        SceneManager.LoadSceneAsync(sceneIndex);
    }
}

[Serializable]
public class Stall
{
    public StallData m_StallData;
}

[Serializable]
public class StallData
{
    public string m_stallName;
    public bool IsActive;
}

[Serializable]
public class EventStallData
{
    public string CompanyName;
    public string Description;
    public string DocumentURL;
    public string VideoURL;

    public string DocumentThumbnailURL;
    public string VideoThumbnailURL;

    public string LogoSquare;
    public string LogoRectangle;

    public List<EventStallJobs> m_Jobs = new List<EventStallJobs>();

    public void SetJobs(Job[] jobs)
    {
        for (int i = 0; i < jobs.Length; i++)
        {
            EventStallJobs l_stalljobs = new EventStallJobs
            {
                jobname = jobs[i].title,
                joblink = jobs[i].apply_url
            };
            m_Jobs.Add(l_stalljobs);
        }
    }
}

[Serializable]
public class EventStallJobs
{
    public string jobname;
    public string joblink;
}

[Serializable]
public class WebinarExtractedData
{
    public string m_WebinarName;
    public string m_WebinarUrl;
}