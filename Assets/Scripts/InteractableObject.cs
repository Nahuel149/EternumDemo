using UnityEngine;

namespace EternumInteractables
{
    public class InteractableObject : MonoBehaviour
    {
        public enum InteractionTypes { Pickup, Sit, Use }
        public InteractionTypes interactionType;

        [SerializeField] private Transform sitPosition; // Added for sitting
        [SerializeField] private Vector3 sitOffset = new Vector3(0, 0, 0); // Added for sitting

        private void OnMouseDown()
        {
            Debug.Log($"Interacted with {gameObject.name}");

            switch (interactionType)
            {
                case InteractionTypes.Pickup:
                    HandlePickup();
                    break;
                case InteractionTypes.Sit:
                    HandleSit();
                    break;
                case InteractionTypes.Use:
                    HandleUse();
                    break;
            }
        }

        private void HandlePickup()
        {
            Debug.Log($"Picking up {gameObject.name}");
            // Add pickup logic
        }

        private void HandleSit()
        {
            Debug.Log($"Sitting on {gameObject.name}");

            // Get player reference
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // Position player at sit position
                player.transform.position = transform.position + sitOffset;
                player.transform.rotation = transform.rotation;

                // Trigger sit animation if you have an animator
                Animator playerAnimator = player.GetComponent<Animator>();
                if (playerAnimator != null)
                {
                    playerAnimator.SetTrigger("Sit");
                }
            }
        }

        private void HandleUse()
        {
            Debug.Log($"Using {gameObject.name}");
            // Add use logic
        }
    }
}