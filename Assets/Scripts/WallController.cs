using UnityEngine;
using UnityEngine.InputSystem;

public class WallController : MonoBehaviour
{
    [SerializeField] private float targetYPosition = -0.655808f; // Duvarın inecek olduğu aşağıdaki Y pozisyonu
    [SerializeField] private float speed = 10f;            // İtme / inme hızı
    private bool shouldLower = false;

    void Update()
    {
        // Klavyeden B tuşuna basıldığını kontrol et
        if (Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame)
        {
            shouldLower = true;
        }

        // B tuşuna basıldıysa duvarı aşağı kaydır
        if (shouldLower)
        {
            Vector3 targetPos = new Vector3(transform.position.x, targetYPosition, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
        }
    }
}
