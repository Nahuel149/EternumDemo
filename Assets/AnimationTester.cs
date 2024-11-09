using UnityEngine;

public class AnimationTester : MonoBehaviour
{
    private Animator animator;
    private CharacterController characterController;
    private bool isSitting = false;

    [SerializeField] private Transform chairTarget; // Reference to sitting position
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    void Start()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isSitting)
        {
            StartSitting();
        }
        else if (Input.GetKeyDown(KeyCode.E) && isSitting)
        {
            StopSitting();
        }
    }

    private void StartSitting()
    {
        isSitting = true;

        // Disable character controller
        if (characterController != null)
            characterController.enabled = false;

        // Store current position
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        // Move to chair
        if (chairTarget != null)
        {
            transform.position = chairTarget.position;
            transform.rotation = chairTarget.rotation;
        }

        // Trigger animation
        animator.SetTrigger("Sit");
    }

    private void StopSitting()
    {
        isSitting = false;

        // Return to original position
        transform.position = originalPosition;
        transform.rotation = originalRotation;

        // Re-enable character controller
        if (characterController != null)
            characterController.enabled = true;

        // Trigger stand animation
        animator.SetTrigger("StandUp");
    }
}