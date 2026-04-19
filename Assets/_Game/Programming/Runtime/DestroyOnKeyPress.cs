using UnityEngine;
using UnityEngine.InputSystem;

public class DestroyOnKeyPress : MonoBehaviour
{
    [SerializeField] private GameObject targetObject;

    private void Update()
    {
        if (Keyboard.current.vKey.wasPressedThisFrame)
        {
            if (targetObject != null)
                Destroy(targetObject);
        }
    }
}