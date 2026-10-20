using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class PDFReader : MonoBehaviour
{
    [Tooltip("The name of the folder inside StreamingAssets/PDF/ (e.g., 'PDF1')")]
    public string pdfFolderName = "PDF1"; 
    
    [Tooltip("Drag a RawImage from your Canvas here to display the JPG")]
    public RawImage pdfDisplay;

    private int currentPage = 1;

    public void OpenPDF()
    {
        currentPage = 1;
        StartCoroutine(LoadPageCoroutine(currentPage));
    }

    public void NextPage()
    {
        currentPage++;
        StartCoroutine(LoadPageCoroutine(currentPage));
    }

    public void PreviousPage()
    {
        if (currentPage > 1)
        {
            currentPage--;
            StartCoroutine(LoadPageCoroutine(currentPage));
        }
    }

    private IEnumerator LoadPageCoroutine(int pageNumber)
    {
        // Format the page number to have a leading zero (e.g., 1 becomes "01", 10 stays "10")
        string formattedFileName = pageNumber.ToString("00") + ".jpg";
        
        // Build the final URL
        string url = Application.streamingAssetsPath + "/PDF/" + pdfFolderName + "/" + formattedFileName;

        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                Texture2D tex = DownloadHandlerTexture.GetContent(uwr);
                
                if (pdfDisplay != null)
                {
                    pdfDisplay.texture = tex;
                }
            }
            else
            {
                Debug.LogWarning("Could not load page " + formattedFileName + ". End of document? Error: " + uwr.error);
                
                if (pageNumber > 1) 
                {
                    currentPage--;
                }
            }
        }
    }
}