using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class WebinarInteraction : MonoBehaviour
{
   [SerializeField] private UnityEvent startEvent;
   [SerializeField] private UnityEvent EndEvent;
   private void OnTriggerEnter(Collider other)
   {
      if (other.tag != "Player") return;
      startEvent.Invoke();
   }

   private void OnTriggerExit(Collider other)
   {
      if (other.tag != "Player") return;
      EndEvent.Invoke();
   }
}
