using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Webinar : MonoBehaviour
{
    [SerializeField] int SceneIndex = 0;

    [Header("Scene Transition")]
    [SerializeField] float FadeOutTime = 1.5f;
    [SerializeField] float FadeInTime = 1.5f;
    [SerializeField] float FadeWaitTime = 0.5f;

    [SerializeField] private UnityEvent startEvent;
    [SerializeField] private UnityEvent EndEvent;

    private void Start()
    {
       
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;
       //StartCoroutine(SceneTransition());
       startEvent.Invoke();

    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        EndEvent.Invoke();
    }
    public void TransitionToScene()
    {
        EndEvent.Invoke();
        StartCoroutine(SceneTransition());
    }
    IEnumerator SceneTransition()
    {
        SceneFader fader = FindFirstObjectByType<SceneFader>();
        DontDestroyOnLoad(gameObject);
        yield return fader.FadeOut(FadeOutTime);    
        yield return SceneManager.LoadSceneAsync(SceneIndex);
        yield return new WaitForSeconds(FadeWaitTime);
        yield return fader.FadeIn(FadeInTime);
        Destroy(gameObject);
    }
}
