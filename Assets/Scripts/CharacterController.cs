using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterController : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] private Rigidbody rb;
    [SerializeField] float speed;
    [SerializeField] private float lateralSmoothSpeed = 10f;   //Yumuşak geçiş hızı

    private float[] xPosition = { -0.36f, 0f, 0.36f };

    private int currentXpositionIndex = 1;
    Vector3 targetPosition;


    void Start()
    {
        targetPosition = transform.position;
    }

    void Update()
    { 
        if (Keyboard.current != null)
        {
            if ((Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame) && currentXpositionIndex > 0)
            {
                currentXpositionIndex--;
                UpdateLateralPosition();
            }
            else if ((Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame) && currentXpositionIndex < 2)
            {
                currentXpositionIndex++;
                UpdateLateralPosition();
            }
        }        
    }

    private void FixedUpdate()
    {
        Vector3 forwardMove = Vector3.forward * speed * Time.fixedDeltaTime;   // İleri haraket kodu

        // yana yumuşak geçiş yapımı
        Vector3 currentPosition = rb.position;
        Vector3 lateralMove = Vector3.Lerp(currentPosition, targetPosition, Time.fixedDeltaTime*lateralSmoothSpeed);

        // İleri ve yanal haraketi birleştirmek
        Vector3 combineMove = new Vector3(lateralMove.x, transform.position.y, rb.position.z) + forwardMove;
        rb.MovePosition(combineMove);
    }

    void UpdateLateralPosition()
    {
        targetPosition = new Vector3(xPosition[currentXpositionIndex], transform.position.y, transform.position.z);
    }
}
