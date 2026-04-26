using UnityEngine;
using UnityEngine.InputSystem;

public class SpawnOnKeyPress : MonoBehaviour
{
    private GameObject targetObject;
    [SerializeField]private GameObject spawnObject;

    private void Update()
    {
        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            targetObject = Instantiate(spawnObject, transform.position, Quaternion.identity);
                
        }

        if (Keyboard.current.vKey.wasPressedThisFrame)
        {
            if (targetObject != null)
                Destroy(targetObject);
        }
    }
}
