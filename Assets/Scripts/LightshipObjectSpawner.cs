// using System;
// using System.Collections.Generic;
// using UnityEngine;
// using Niantic.Lightship.AR;
// using Unity.Netcode;

// public class LightshipObjectSpawner : MonoBehaviour
// {
//     [SerializeField]
//     [Tooltip("The camera that objects will face when spawned. If not set, defaults to the main camera.")]
//     private Camera m_CameraToFace;

//     public Camera cameraToFace
//     {
//         get
//         {
//             EnsureFacingCamera();
//             return m_CameraToFace;
//         }
//         set => m_CameraToFace = value;
//     }

//     [SerializeField]
//     [Tooltip("The list of prefabs available to spawn.")]
//     private List<GameObject> m_ObjectPrefabs = new List<GameObject>();

//     public List<GameObject> objectPrefabs
//     {
//         get => m_ObjectPrefabs;
//         set => m_ObjectPrefabs = value;
//     }

//     [SerializeField]
//     [Tooltip("The index of the prefab to spawn. If outside the range, a random object will be selected.")]
//     private int m_SpawnOptionIndex = -1;

//     public int spawnOptionIndex
//     {
//         get => m_SpawnOptionIndex;
//         set => m_SpawnOptionIndex = value;
//     }

//     [SerializeField]
//     [Tooltip("Whether to apply a random rotation when spawning")]
//     private bool m_ApplyRandomRotation = true;

//     [SerializeField]
//     [Tooltip("Range of random rotation in degrees")]
//     private float m_RotationRange = 45f;

//     [SerializeField]
//     [Tooltip("Spawn objects as children of this transform")]
//     private bool m_SpawnAsChildren = false;

//     [SerializeField]
//     [Tooltip("The size, in viewport units, of the periphery inside the viewport that will not be considered in view.")]
//     private float m_ViewportPeriphery = 0.15f;

//     // Event for when an object is spawned
//     public event Action<GameObject> ObjectSpawned;

//     private void Awake()
//     {
//         EnsureFacingCamera();
//     }

//     private void EnsureFacingCamera()
//     {
//         if (m_CameraToFace == null)
//             m_CameraToFace = Camera.main;
//     }

//     public void RandomizeSpawnOption()
//     {
//         m_SpawnOptionIndex = -1;
//     }

//     public bool TrySpawnObjectAtViewportPosition(Vector2 viewportPosition)
//     {
//         if (m_ObjectPrefabs.Count == 0)
//         {
//             Debug.LogWarning("Object Prefabs list is empty.");
//             return false;
//         }

//         // if (!IsPositionValid(viewportPosition))
//         // {
//         //     Debug.LogWarning("Spawn position is too close to the viewport edge.");
//         //     return false;
//         // }

//         Ray ray = m_CameraToFace.ViewportPointToRay(viewportPosition);
//         RaycastHit hit;

//         Vector3 spawnPosition;
//         if (Physics.Raycast(ray, out hit))
//         {
//             spawnPosition = hit.point;
//         }
//         else
//         {
//             spawnPosition = ray.origin + ray.direction * 2.0f; // Default distance if no hit
//         }

//         int objectIndex = DetermineSpawnIndex();
//         GameObject prefabToSpawn = m_ObjectPrefabs[objectIndex];
//         GameObject spawnedObject = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
//         Debug.Log("Raycast hit position: " + hit.point);
//         spawnedObject.GetComponent<NetworkObject>().Spawn(true);

//         RotateObjectToFaceCamera(spawnedObject, spawnPosition);
        
//         if (m_ApplyRandomRotation)
//         {
//             ApplyRandomRotation(spawnedObject);
//         }

//         if (m_SpawnAsChildren)
//         {
//             spawnedObject.transform.SetParent(transform);
//         }

//         ObjectSpawned?.Invoke(spawnedObject);
//         return true;
//     }

//     private bool IsPositionValid(Vector2 viewportPosition)
//     {
//         float inViewMin = m_ViewportPeriphery;
//         float inViewMax = 1f - m_ViewportPeriphery;

//         return viewportPosition.x >= inViewMin && viewportPosition.x <= inViewMax &&
//                viewportPosition.y >= inViewMin && viewportPosition.y <= inViewMax;
//     }

//     private int DetermineSpawnIndex()
//     {
//         return (m_SpawnOptionIndex < 0 || m_SpawnOptionIndex >= m_ObjectPrefabs.Count)
//             ? UnityEngine.Random.Range(0, m_ObjectPrefabs.Count)
//             : m_SpawnOptionIndex;
//     }

//     private void RotateObjectToFaceCamera(GameObject spawnedObject, Vector3 spawnPoint)
//     {
//         Vector3 directionToCamera = m_CameraToFace.transform.position - spawnPoint;
//         directionToCamera.y = 0; // Keep horizontal rotation
//         spawnedObject.transform.rotation = Quaternion.LookRotation(directionToCamera);
//     }

//     private void ApplyRandomRotation(GameObject spawnedObject)
//     {
//         float randomRotation = UnityEngine.Random.Range(-m_RotationRange, m_RotationRange);
//         spawnedObject.transform.Rotate(Vector3.up, randomRotation);
//     }
// }


using System;
using System.Collections.Generic;
using UnityEngine;
using Niantic.Lightship.AR;
using Unity.Netcode;

public class LightshipObjectSpawner : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The camera that objects will face when spawned. If not set, defaults to the main camera.")]
    private Camera m_CameraToFace;

    public Camera cameraToFace
    {
        get
        {
            EnsureFacingCamera();
            return m_CameraToFace;
        }
        set => m_CameraToFace = value;
    }

    [SerializeField]
    [Tooltip("The list of prefabs available to spawn.")]
    private List<GameObject> m_ObjectPrefabs = new List<GameObject>();

    public List<GameObject> objectPrefabs
    {
        get => m_ObjectPrefabs;
        set => m_ObjectPrefabs = value;
    }

    [SerializeField]
    [Tooltip("The index of the prefab to spawn. If outside the range, a random object will be selected.")]
    private int m_SpawnOptionIndex = -1;

    public int spawnOptionIndex
    {
        get => m_SpawnOptionIndex;
        set => m_SpawnOptionIndex = value;
    }

    [SerializeField]
    [Tooltip("Whether to apply a random rotation when spawning")]
    private bool m_ApplyRandomRotation = true;

    [SerializeField]
    [Tooltip("Range of random rotation in degrees")]
    private float m_RotationRange = 45f;

    [SerializeField]
    [Tooltip("Spawn objects as children of this transform")]
    private bool m_SpawnAsChildren = false;

    [SerializeField]
    [Tooltip("The size, in viewport units, of the periphery inside the viewport that will not be considered in view.")]
    private float m_ViewportPeriphery = 0.15f;

    [SerializeField]
    [Tooltip("Whether to only spawn an object if the spawn point is within view of the camera.")]
    private bool m_OnlySpawnInView = true;

    // Event for when an object is spawned
    public event Action<GameObject> ObjectSpawned;

    private void Awake()
    {
        EnsureFacingCamera();
    }

    private void EnsureFacingCamera()
    {
        if (m_CameraToFace == null)
            m_CameraToFace = Camera.main;
    }

    public void RandomizeSpawnOption()
    {
        m_SpawnOptionIndex = -1;
    }

    public bool TrySpawnObjectAtViewportPosition(Vector2 viewportPosition)
    {
        if (m_ObjectPrefabs.Count == 0)
        {
            Debug.LogWarning("Object Prefabs list is empty.");
            return false;
        }

        if (m_OnlySpawnInView && !IsPositionInView(viewportPosition))
        {
            Debug.LogWarning("Spawn position is outside the viewport bounds.");
            return false;
        }

        Ray ray = m_CameraToFace.ViewportPointToRay(viewportPosition);
        RaycastHit hit;

        Vector3 spawnPosition;
        if (Physics.Raycast(ray, out hit))
        {
            spawnPosition = hit.point;
        }
        else
        {
            spawnPosition = ray.origin + ray.direction * 2.0f; // Default distance if no hit
        }

        int objectIndex = DetermineSpawnIndex();
        GameObject prefabToSpawn = m_ObjectPrefabs[objectIndex];
        GameObject spawnedObject = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
        spawnedObject.GetComponent<NetworkObject>().Spawn(true);

        RotateObjectToFaceCamera(spawnedObject, spawnPosition);
        
        if (m_ApplyRandomRotation)
        {
            ApplyRandomRotation(spawnedObject);
        }

        if (m_SpawnAsChildren)
        {
            spawnedObject.transform.SetParent(transform);
        }

        ObjectSpawned?.Invoke(spawnedObject);
        return true;
    }

    private bool IsPositionInView(Vector2 viewportPosition)
    {
        float inViewMin = m_ViewportPeriphery;
        float inViewMax = 1f - m_ViewportPeriphery;

        return viewportPosition.x >= inViewMin && viewportPosition.x <= inViewMax &&
               viewportPosition.y >= inViewMin && viewportPosition.y <= inViewMax;
    }

    private int DetermineSpawnIndex()
    {
        return (m_SpawnOptionIndex < 0 || m_SpawnOptionIndex >= m_ObjectPrefabs.Count)
            ? UnityEngine.Random.Range(0, m_ObjectPrefabs.Count)
            : m_SpawnOptionIndex;
    }

    private void RotateObjectToFaceCamera(GameObject spawnedObject, Vector3 spawnPoint)
    {
        Vector3 directionToCamera = m_CameraToFace.transform.position - spawnPoint;
        directionToCamera.y = 0; // Keep horizontal rotation
        spawnedObject.transform.rotation = Quaternion.LookRotation(directionToCamera);
    }

    private void ApplyRandomRotation(GameObject spawnedObject)
    {
        float randomRotation = UnityEngine.Random.Range(-m_RotationRange, m_RotationRange);
        spawnedObject.transform.Rotate(Vector3.up, randomRotation);
    }
}
