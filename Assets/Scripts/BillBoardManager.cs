using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using Unity.VisualScripting;
public class BillBoardManager : MonoBehaviour
{

    public enum BillBoardType { LookAtCamera, CameraForward};
    [Header("Bill Board Type")]
    [SerializeField] BillBoardType m_billBoardType;
    [SerializeField] bool IsBillBoarding = false;

    [Header("Rotation Attrib")]
    [SerializeField] bool X_Lock = false;
    [SerializeField] bool Y_Lock = false;
    [SerializeField] bool Z_Lock = false;
    [SerializeField] Vector3 OriginalRotation;

    private void Awake()
    {
        OriginalRotation = transform.rotation.eulerAngles;
    }
    public void StartBillBoarding()
    {
        IsBillBoarding = true;
    }

    public void StopBillBoarding()
    {
        IsBillBoarding = false;
    }

    private void Update()
    {
        if (!IsBillBoarding) return;

        switch (m_billBoardType)
        {
            case BillBoardType.LookAtCamera:
                transform.LookAt(Camera.main.transform.position, Vector3.up);
                break;
            case BillBoardType.CameraForward:
                transform.forward = Camera.main.transform.forward;
                break;

            default: break;
        }

        Vector3 rotation = transform.rotation.eulerAngles;
        if(X_Lock) { rotation.x = OriginalRotation.x; }
        if(Y_Lock) { rotation.y = OriginalRotation.y; }
        if(Z_Lock) { rotation.z = OriginalRotation.z; }

        transform.rotation = Quaternion.Euler(rotation);
    }
   
   
}
