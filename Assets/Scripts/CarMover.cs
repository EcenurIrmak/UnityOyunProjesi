using UnityEngine;

public class CarMover : MonoBehaviour
{
    [SerializeField] private float carSpeed;

    void Start()
    {

    }
    void Update()
    {
        transform.Translate(0,0,carSpeed);
    }
}
