using UnityEngine;

public class StallInteraction : MonoBehaviour
{
    [SerializeField] InteractionHandler m_InteractionHandler;
    [SerializeField] GameObject m_CloseSignMesh;
    [SerializeField] GameObject m_Receptionist;

    public void DisableStall()
    {
        m_InteractionHandler.gameObject.SetActive(false);
        m_Receptionist.SetActive(false);
        m_CloseSignMesh.SetActive(true);
    }
}
