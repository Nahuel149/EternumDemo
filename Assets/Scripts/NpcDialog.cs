using System.Threading.Tasks;
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
        // Validate required references
        if (mainCamera == null)
            Debug.LogError("Main Camera reference is missing on " + gameObject.name);
        if (toActivate == null)
            Debug.LogError("To Activate object reference is missing on " + gameObject.name);
        if (standingPoint == null)
            Debug.LogError("Standing Point reference is missing on " + gameObject.name);
    }

    private async void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isInDialog)
        {
            isInDialog = true;
            avatar = other.transform;

            PlayerInput playerInput = avatar.GetComponent<PlayerInput>();
            if (playerInput == null)
            {
                Debug.LogError("PlayerInput component not found on player!");
                return;
            }

            try
            {
                // disable player input
                playerInput.enabled = false;

                await Task.Delay(50);

                // teleport the avatar to standing point
                if (standingPoint != null)
                {
                    avatar.position = standingPoint.position;
                    avatar.rotation = standingPoint.rotation;
                }

                // disable main cam, enable dialog cam
                if (mainCamera != null)
                    mainCamera.SetActive(false);
                if (toActivate != null)
                    toActivate.SetActive(true);

                // display cursor
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error in NpcDialog: {e.Message}");
                Recover(); // Attempt to recover if something goes wrong
            }
        }
    }

    public void Recover()
    {
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
    }

    private void OnDisable()
    {
        // Ensure we recover if the script is disabled
        if (isInDialog)
            Recover();
    }
}