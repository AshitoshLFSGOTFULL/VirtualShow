using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CrosshairInteractor : MonoBehaviour
{
    void Update()
    {
        // When the player left-clicks
        if (Input.GetMouseButtonDown(0))
        {
            //Debug.Log("Click");
            InteractWithUI();
        }
    }

    void InteractWithUI()
    {
        if (EventSystem.current == null) return;
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = new Vector2(Screen.width / 2f, Screen.height / 2f)
        };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        Debug.Log($"[Crosshair] Raycast found {results.Count} UI objects under the center of the screen.");

        if (results.Count > 0)
        {
            GameObject clickedObject = results[0].gameObject;
            Debug.Log($"[Crosshair] Interacting with topmost UI element: {clickedObject.name}");
            
            ExecuteEvents.Execute(clickedObject, pointerData, ExecuteEvents.pointerClickHandler);
            ExecuteEvents.Execute(clickedObject, pointerData, ExecuteEvents.pointerDownHandler);
        }
        else
        {
            Debug.Log("[Crosshair] Raycast hit nothing. Clearing Event System selection.");
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}