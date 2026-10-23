using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public class VirtualEnvironmentResponse : MonoBehaviour
{
    private const string API_URL = "https://msp.virtualevents.my/api/v1/virtual-environment";
    private const string WEBINAR_API_URL = "https://msp.virtualevents.my/api/v1/virtual-environment/webinars";
    [SerializeField] private string bearerToken;
    public VirtualEnvironmentResponseWrapper data;
    public WebinarResponseWrapper webinarData;

    private void Start()
    {
        StartCoroutine(GetVirtualEnvironment());
        StartCoroutine(GetWebinarData());
    }

    public IEnumerator GetVirtualEnvironment()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(API_URL))
        {
            request.SetRequestHeader(
                "Authorization",
                "Bearer " + bearerToken
            );

            request.SetRequestHeader(
                "Accept",
                "application/json"
            );

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("API Response:");
                Debug.Log(request.downloadHandler.text);

                data = JsonUtility.FromJson<VirtualEnvironmentResponseWrapper>(
                    request.downloadHandler.text
                );

                OnDataReceived(data);

            }
            else
            {
                Debug.LogError(
                    $"API Error: {request.responseCode}\n{request.error}"
                );

                Debug.LogError(request.downloadHandler.text);
            }
        }
    }

    private IEnumerator GetWebinarData()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(WEBINAR_API_URL))
        {
            request.SetRequestHeader(
                "Authorization",
                "Bearer " + bearerToken
            );

            request.SetRequestHeader(
                "Accept",
                "application/json"
            );

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Webinar API Response:");
                Debug.Log(request.downloadHandler.text);

                webinarData = JsonUtility.FromJson<WebinarResponseWrapper>(
                    request.downloadHandler.text
                );

                OnWebinarRecieved(webinarData);
            }
            else
            {
                Debug.LogError(
                    $"API Error: {request.responseCode}\n{request.error}"
                );

                Debug.LogError(request.downloadHandler.text);
            }
        }
    }
    private void OnDataReceived(VirtualEnvironmentResponseWrapper response)
    {
        Debug.Log("Event: " + response.data.@event.title);

        Debug.Log("Booth Count: " + response.data.booth_count);
        foreach (Booth booth in response.data.booths)
        {
            Debug.Log($"Document URL {booth.documents[0].url}");
            Debug.Log($"Document Thumbnail URL {booth.documents[0].thumbnail_url}");

            Debug.Log($"Video URL {booth.videos[0].url}");
            Debug.Log($"Video Thumbnail URL {booth.videos[0].thumbnail_url}");
            GameManager.s_Instance.SetStallData(booth.company.name,booth.is_active);
            GameManager.s_Instance.SetEventData(booth.company.name,booth.description,booth.documents[0].url, booth.videos[0].url, booth.documents[0].thumbnail_url, booth.videos[0].thumbnail_url,booth.logo_square_url,booth.logo_rectangle_url);
        }

        GameManager.s_Instance.SetDataLoaded(true); 
    }

    private void OnWebinarRecieved(WebinarResponseWrapper response)
    {
        foreach (WebinarData l_data in response.data.webinars)
        {
            GameManager.s_Instance.SetWebinaData(l_data.title,l_data.registration_url);
        }
    }
}


[Serializable]
public class VirtualEnvironmentResponseWrapper
{
    public VirtualEnvironmentData data;
}

[Serializable]
public class VirtualEnvironmentData
{
    public EventData @event;
    public Booth[] booths;
    public int booth_count;
    public string connected_at;
    public string updated_at;
}

[Serializable]
public class EventData
{
    public int id;
    public string code;
    public string title;
    public string slug;
    public string short_summary;
    public string description;
    public string start_date;
    public string end_date;
    public string start_time;
    public string end_time;
    public string timezone;
    public string status;
    public string visibility;
    public string url;
    public string theme_color;
    public string logo_url;
    public string banner_url;
    public string cover_image_url;
    public Organizer organizer;
}

[Serializable]
public class Organizer
{
    public string name;
    public string email;
    public string phone;
}

[Serializable]
public class Booth
{
    public int id;
    public string name;
    public string slug;
    public bool is_active;
    public Company company;
    public string description;
    public string website_url;
    public string booth_url;
    public string logo_url;
    public string banner_url;
    public string logo_square_url;
    public string logo_rectangle_url;

    public Document[] documents;
    public Video[] videos;
    public Job[] jobs;
    public int sort_order;
}

[Serializable]
public class Company
{
    public int id;
    public string name;
}

[Serializable]
public class Document
{
    public string title;
    public string url;
    public string file_name;
    public string thumbnail_url;
}

[Serializable]
public class Video
{
    public string title;
    public string url;
    public string thumbnail_url;
}


[Serializable]
public class WebinarResponseWrapper
{
    public WebinarResponseData data;
}

[Serializable]
public class WebinarResponseData
{
    public WebinarEventData eventData;
    public WebinarData[] webinars;
    public int webinar_count;
}

[Serializable]
public class WebinarEventData
{
    public int id;
    public string title;
    public string timezone;
}

[Serializable]
public class WebinarData
{
    public int id;
    public string title;
    public string description;
    public string status;
    public string audience;
    public string starts_at;
    public string ends_at;
    public string timezone;
    public int duration_minutes;
    public string registration_url;
}

[Serializable]
public class Job
{
    public int id;
    public string title;
    public string department;
    public string location;
    public string employment_type;
    public string work_mode;
    public string experience;
    public string education;
    public string[] required_skills;
    public string salary_range;
    public int openings;
    public string short_description;
    public string application_deadline;
    public bool is_featured;
    public string apply_url;
}