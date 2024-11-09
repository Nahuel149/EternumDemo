using UnityEngine;
using EternumInteractables;  // Add this using statement

public class InteractableManager : MonoBehaviour
{
    [SerializeField] private GameObject[] interactableObjects;

    void Start()
    {
        foreach (GameObject obj in interactableObjects)
        {
            if (!obj.GetComponent<InteractableObject>())
            {
                obj.AddComponent<InteractableObject>();
            }
        }
    }
}