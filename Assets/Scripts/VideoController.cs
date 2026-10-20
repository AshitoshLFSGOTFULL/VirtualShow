using UnityEngine;
using UnityEngine.Video;
using System.IO;

public class WebGLVideoController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    
    [Tooltip("Exact file name, e.g., myvideo.mp4")]
    public string videoFileName = "myvideo.mp4";

    private void Start()
    {
        if (videoPlayer == null)
        {
            Debug.LogError("VideoPlayer component is missing or not assigned!");
            return;
        }

        videoPlayer.playOnAwake = false; 

        // Safely combine the path for both Windows/Mac Editor and WebGL
        string path = Path.Combine(Application.streamingAssetsPath, videoFileName);
        videoPlayer.url = path;
        
        Debug.Log("Trying to load video from: " + path);

#if UNITY_EDITOR
        // This checks if the file actually exists on your hard drive before trying to play it
        if (!File.Exists(path))
        {
            Debug.LogError("FILE NOT FOUND! I am looking for a folder named exactly 'StreamingAssets' inside your Assets folder, and a file named '" + videoFileName + "' inside it.");
            return;
        }
#endif

        videoPlayer.Prepare();
    }

    public void PlayVideo()
    {
        if (videoPlayer == null) return;

        if (!videoPlayer.isPrepared)
        {
            Debug.LogWarning("Video is not prepared yet! Forcing prepare and playing when ready...");
            videoPlayer.Prepare();
            videoPlayer.prepareCompleted += PlayOncePrepared;
        }
        else
        {
            videoPlayer.Play();
        }
    }

    private void PlayOncePrepared(VideoPlayer vp)
    {
        vp.prepareCompleted -= PlayOncePrepared; // Remove listener so it doesn't fire twice
        vp.Play();
    }

    public void StopVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Pause();
            videoPlayer.time = 0; 
            videoPlayer.frame = 0;
        }
    }
}