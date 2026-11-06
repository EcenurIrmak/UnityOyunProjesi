using UnityEngine;

public class WallController : MonoBehaviour
{
    [SerializeField] private float targetYPosition = -0.655808f;
    [SerializeField] private float speed = 10f;
    public bool shouldLower = false;

    void Update()
    {        
        if (shouldLower)
        {
            Vector3 targetPos = new Vector3(transform.position.x, targetYPosition, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
        }
    }

    // Companion düğmeye ulaştığında bu fonksiyon çağrılacak
    public void LowerWall()
    {
        shouldLower = true;
    }
}