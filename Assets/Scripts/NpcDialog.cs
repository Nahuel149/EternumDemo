using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class NpcDialog : MonoBehaviour
{
    [Header("Camera References")]
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject toActivate;

    [Header("Position Reference")]
    [SerializeField] private Transform standingPoint;

    private Transform avatar;
    private bool isInDialog = false;

    private void Start()
    {
        Debug.Log("[NpcDialog] Initializing...");

        // Initialize MainThreadDispatcher
        if (FindObjectOfType<MainThreadDispatcher>() == null)
        {
            var go = new GameObject("MainThreadDispatcher");
            go.AddComponent<MainThreadDispatcher>();
            Debug.Log("[NpcDialog] MainThreadDispatcher created");
        }

        // Validate required references
        if (mainCamera == null)
            Debug.LogError("Main Camera reference is missing on " + gameObject.name);
        if (toActivate == null)
            Debug.LogError("To Activate object reference is missing on " + gameObject.name);
        if (standingPoint == null)
            Debug.LogError("Standing Point reference is missing on " + gameObject.name);
        Debug.Log("[NpcDialog] Initialization complete");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[NpcDialog] Trigger entered by {other.gameObject.name}");

        if (other.CompareTag("Player") && !isInDialog)
        {
            Debug.Log("[NpcDialog] Starting dialog sequence");
            isInDialog = true;
            avatar = other.transform;

            PlayerInput playerInput = avatar.GetComponent<PlayerInput>();
            if (playerInput == null)
            {
                Debug.LogError("[NpcDialog] PlayerInput component not found!");
                return;
            }

            try
            {
                Debug.Log("[NpcDialog] Disabling player input");
                playerInput.enabled = false;

                // Replace Task.Delay with coroutine for WebGL compatibility
                StartCoroutine(InitializeDialog());
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[NpcDialog] Error: {e.Message}");
                Recover();
            }
        }
    }

    private IEnumerator InitializeDialog()
    {
        yield return new WaitForSeconds(0.05f);

        try
        {
            Debug.Log("[NpcDialog] Positioning avatar");
            if (standingPoint != null)
            {
                avatar.position = standingPoint.position;
                avatar.rotation = standingPoint.rotation;
            }

            Debug.Log("[NpcDialog] Switching cameras");
            if (mainCamera != null)
                mainCamera.SetActive(false);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[NpcDialog] Error in InitializeDialog: {e.Message}");
            Recover();
            yield break;
        }

        // Move yield return outside of try-catch
        if (toActivate != null)
        {
            // Add delay before activating chat UI
            yield return new WaitForSeconds(0.5f);

            try
            {
                toActivate.SetActive(true);

                Debug.Log("[NpcDialog] Setting cursor state");
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[NpcDialog] Error in InitializeDialog: {e.Message}");
                Recover();
            }
        }
    }

    public void Recover()
    {
        Debug.Log("[NpcDialog] Recovering dialog state");
        if (avatar != null)
        {
            PlayerInput playerInput = avatar.GetComponent<PlayerInput>();
            if (playerInput != null)
                playerInput.enabled = true;
        }

        if (mainCamera != null)
            mainCamera.SetActive(true);
        if (toActivate != null)
            toActivate.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        isInDialog = false;
        Debug.Log("[NpcDialog] Recovery complete");
    }

    private void OnDisable()
    {
        Debug.Log("[NpcDialog] OnDisable called");
        // Ensure we recover if the script is disabled
        if (isInDialog)
            Recover();
    }
}