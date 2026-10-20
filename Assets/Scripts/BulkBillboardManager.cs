using UnityEngine;
using UnityEngine.Jobs;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

public class BulkBillboardManager : MonoBehaviour
{
    [Header("Settings")]
    public Transform playerCamera;
    public bool lockYAxis = true;
    public bool invertDirection = true;
    [Header("Targets")]
    [Tooltip("Add Objects which will rotates towards the player here ")]public Transform[] billboardTargets;

    private TransformAccessArray transformAccessArray;
    private JobHandle billboardJobHandle;
    
    // Safety flag for WebGL memory lifecycle
    private bool isInitialized = false;

    void OnEnable()
    {
        if (playerCamera == null && Camera.main != null) 
        {
            playerCamera = Camera.main.transform;
        }

        // WebGL Safety: Prevent scheduling an empty array
        if (billboardTargets == null || billboardTargets.Length == 0)
        {
            Debug.LogWarning("BulkBillboardManager: No targets assigned.");
            return;
        }

        // Allocate native memory only when active
        transformAccessArray = new TransformAccessArray(billboardTargets);
        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized || playerCamera == null) return;

        billboardJobHandle.Complete();

        BillboardJob job = new BillboardJob
        {
            cameraPosition = playerCamera.position,
            lockYAxis = lockYAxis,
            invertDirection = invertDirection,
        };

        billboardJobHandle = job.Schedule(transformAccessArray);
    }

    void LateUpdate()
    {
        if (!isInitialized) return;
        
        billboardJobHandle.Complete();
    }

    void OnDisable()
    {
        if (!isInitialized) return;

        billboardJobHandle.Complete();
        
        if (transformAccessArray.isCreated)
        {
            transformAccessArray.Dispose();
        }
        
        isInitialized = false;
    }
}

[BurstCompile]
public struct BillboardJob : IJobParallelForTransform
{
    [ReadOnly] public float3 cameraPosition;
    [ReadOnly] public bool lockYAxis;
    [ReadOnly] public bool invertDirection;
    public void Execute(int index, TransformAccess transform)
    {
        float3 position = transform.position;
        float3 direction = cameraPosition - position;

        if (lockYAxis)
        {
            direction.y = 0;
        }

        if (math.lengthsq(direction) > 10f)
        {
            float3 forward = math.normalize(direction);
            
            if (invertDirection)
            {
                forward = -forward;
            }

            transform.rotation = quaternion.LookRotation(forward, math.up());
        }
    }
}