using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterController : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] private Rigidbody rb;
    [SerializeField] float speed;
    [SerializeField] private float lateralSmoothSpeed = 10f;   //Yumuşak geçiş hızı
    [SerializeField] private GameObject menuCanvas;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] TMP_Text stoneText;
    [SerializeField] private AudioSource effectSourses;
    [SerializeField] AudioClip stonesClip, deathClip;
    private bool onPlatform = false;
    private bool isMovingForward = false;
    private bool isMovingBackward = false;
    private float[] xPosition = { -0.36f, 0f, 0.36f };
    private int currentXpositionIndex = 1;
    Vector3 targetPosition;
    public bool isAlive = true;
    private float score;
    private int stones;
    void Start()
    {
        targetPosition = transform.position;

        // Bunu ekliyoruz: Karakterin fiziksel olarak devrilmesini engeller
        if (rb != null)
        {
            rb.freezeRotation = true;
        }
    }

    void Update()
    {
        if (isAlive)
        {
            score += Time.deltaTime;
            scoreText.text = "Score:" + score.ToString(format: "f1");
            stoneText.text = "Energy Stone:" + stones.ToString(format: "f1");

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
                isMovingForward = Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed;
                isMovingBackward = Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed;
            }                        
        }
    }

    private void FixedUpdate()
    {
        if (isAlive)
        {
            // Yana yumuşak geçişi sadece X ekseninde uyguluyoruz (Z eksenini bozmamak için)
            float targetX = xPosition[currentXpositionIndex];
            float currentX = Mathf.Lerp(rb.position.x, targetX, Time.fixedDeltaTime * lateralSmoothSpeed);
            float forwardMove;
            Vector3 combineMove;

            if (!onPlatform)
            {
                // 1. Durum: Platforma gelene kadar normal ileri gider
                forwardMove = speed * Time.fixedDeltaTime;   //ileri haraket miktarı
                combineMove = new Vector3(currentX, rb.position.y, rb.position.z + forwardMove);
                rb.MovePosition(combineMove);
            }
            else
            {
                // 2. Durum: Platformdayız
                float moveDirection = 0f;
                if (isMovingForward)
                {
                    moveDirection = 1f;  // İleri git (Pozitif yön)
                }
                else if (isMovingBackward)
                {
                    moveDirection = -1f; // Geri git (Negatif yön)
                }                
                
                if (isMovingForward || isMovingBackward)
                {
                    // W veya İleri oka basıyorsak ileri git
                    forwardMove = moveDirection * Time.fixedDeltaTime; 
                    combineMove = new Vector3(currentX, rb.position.y, rb.position.z + forwardMove);
                    rb.MovePosition(combineMove);
                }
                else
                {
                    // Tuşa basmıyorsak Z eksenini hiç değiştirme (Olduğu yerde kilitlensin, geri gitmesin)
                    combineMove = new Vector3(currentX, rb.position.y, rb.position.z);
                    rb.MovePosition(combineMove);
                }
            }
        }
    }

    void UpdateLateralPosition()
    {
        targetPosition = new Vector3(xPosition[currentXpositionIndex], transform.position.y, transform.position.z);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Cars"))
        {            
            isAlive = false;
            //animator.SetBool(name: "Die", true);
            effectSourses.Stop();
            effectSourses.clip = deathClip;
            effectSourses.Play();
            
            menuCanvas.SetActive(true);
        }
        if (other.gameObject.CompareTag("Platform"))
        {
            onPlatform = true;
            rb.isKinematic = true;           
        }
        //if(other.gameObject.CompareTag("Wall"))
        //    rb.isKinematic = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Stone"))
        {
            stones += 1;
            score += 5;
            speed += 0.02f;
            effectSourses.clip = stonesClip;
            effectSourses.Play();
            Destroy(other.gameObject);
        }
    }
}   
