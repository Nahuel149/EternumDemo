using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    private static MainThreadDispatcher instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[MainThreadDispatcher] Instance created and set to DontDestroyOnLoad");
        }
        else
        {
            Debug.Log("[MainThreadDispatcher] Duplicate instance destroyed");
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        MainThread.Update();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
            Debug.Log("[MainThreadDispatcher] Instance destroyed");
        }
    }
}