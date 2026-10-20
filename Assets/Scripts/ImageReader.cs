using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ImageReader : MonoBehaviour
{
    [SerializeField] RawImage m_img;
    [SerializeField] string m_FolderName;
    [SerializeField] string m_DataFolderName;
    [SerializeField] string[] m_Images;
    [SerializeField] bool IsLoaded = false;
    [SerializeField] int m_currentIndex = 0;

    Coroutine m_Coroutine;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable()
    {
        if(!IsLoaded) ReadData();
        LoadPage(m_currentIndex);
    }

    public void Previous()
    {
        if(m_currentIndex > 0)
        {
            m_currentIndex--;
            LoadPage(m_currentIndex);
        }
    }


    public void Next()
    {
        if(m_currentIndex < m_Images.Length - 1)
        {
            m_currentIndex++;
            LoadPage(m_currentIndex);
        }
    }

    public void LoadPage(int index)
    {
        if(File.Exists(m_Images[index]))
        {
            Texture2D tex = new Texture2D(2, 2);
            byte[] data = File.ReadAllBytes(m_Images[index]);
            tex.LoadImage(data);
            m_img.texture = tex;
        }
        else
        {
            Debug.LogError($"Not Able to find Image at path {m_Images[index]}");
        }
    }

    private void ReadData()
    {
        string l_DirectoryPath = Path.Combine(Application.streamingAssetsPath, m_FolderName, m_DataFolderName);
        if(Directory.Exists(l_DirectoryPath))
        {
            m_Images = Directory.GetFiles(l_DirectoryPath, "*.png");
        }

        IsLoaded = true;

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
        m_currentIndex = 0;
    }
}
