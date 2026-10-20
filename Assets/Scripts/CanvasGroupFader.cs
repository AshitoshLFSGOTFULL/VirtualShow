using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasGroupFader : MonoBehaviour
{
    [Tooltip("The CanvasGroup to fade.")]
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Tooltip("How long the fade should take in seconds.")]
    [SerializeField] private float fadeDuration = 1.0f;

    private void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
    }

    /// <summary>
    /// Starts the fade-in process.
    /// </summary>
    public void StartFadeIn()
    {
        StopAllCoroutines(); 
        StartCoroutine(FadeInRoutine());
    }

    /// <summary>
    /// Starts the fade-out process.
    /// </summary>
    public void StartFadeOut()
    {
        StopAllCoroutines();
        StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeInRoutine()
    {
        // 1. Initial Setup: Ensure it is non-interactable during the fade
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        float startAlpha = canvasGroup.alpha;
        float elapsedTime = 0f;

        // 2. Perform the Fade
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            // Lerp smoothly from current alpha to 1
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, elapsedTime / fadeDuration);
            yield return null; 
        }

        // 3. Final State: Fully visible and interactable
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private IEnumerator FadeOutRoutine()
    {
        // 1. Initial Setup: Disable interaction IMMEDIATELY so buttons can't be clicked as it fades
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        float startAlpha = canvasGroup.alpha;
        float elapsedTime = 0f;

        // 2. Perform the Fade
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            // Lerp smoothly from current alpha to 0
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeDuration);
            yield return null;
        }

        // 3. Final State: Fully invisible and non-interactable
        canvasGroup.alpha = 0f;
    }
}