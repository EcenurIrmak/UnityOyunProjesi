using UnityEngine;

public class CarMover : MonoBehaviour
{
    [SerializeField] private float carSpeed;
    CharacterController characterController;

    void Start()
    {
        characterController = FindObjectOfType<CharacterController>();

    }
    void Update()
    {
        if (characterController.isAlive)
        {
            transform.Translate(0, 0, carSpeed);
        }
        
    }
}
