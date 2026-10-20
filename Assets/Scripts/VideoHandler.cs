using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoHandler : MonoBehaviour
{
    [Header("Component")]
    [SerializeField] VideoPlayer m_videoPlayer;
    [SerializeField] RawImage m_rawImage;
    [SerializeField] Vector2Int m_res;

    [Header("File Handling")]
    [SerializeField] string m_FolderName;
    [SerializeField] string m_FileName;
    Coroutine m_Coroutine;
    private void OnEnable()
    {
        PrepareCanvas();
        PlayVideo();
    }
    private void PrepareCanvas()
    {
        RenderTexture rt = new RenderTexture(m_res.x, m_res.y, 0);
        m_videoPlayer.targetTexture = rt;
        m_rawImage.texture = rt;
    }

    private void PlayVideo()
    {
        string path = Path.Combine(Application.streamingAssetsPath, m_FolderName, m_FileName);
        m_videoPlayer.url = path;
        m_videoPlayer.Play();
    }

    public void Close(float time)
    {
        if (!gameObject.activeSelf) return;
        if (m_Coroutine != null) return;
        m_Coroutine = StartCoroutine(CloseVideo(time));
    }
    IEnumerator CloseVideo(float time)
    {
        yield return new WaitForSeconds(time);
        gameObject.SetActive(false);
    }
    private void OnDisable()
    {
        m_videoPlayer.Stop();
        
    }

}
