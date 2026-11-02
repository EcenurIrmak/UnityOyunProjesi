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

    private float[] xPosition = { -0.36f, 0f, 0.36f };

    private int currentXpositionIndex = 1;
    Vector3 targetPosition;

    public bool isAlive = true;

    private float score;
    private int stones;
    void Start()
    {
        targetPosition = transform.position;
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
            }
        }
    }

    private void FixedUpdate()
    {
        if (isAlive)
        {
            Vector3 forwardMove = Vector3.forward * speed * Time.fixedDeltaTime;   // İleri haraket kodu

            // yana yumuşak geçiş yapımı
            Vector3 currentPosition = rb.position;
            Vector3 lateralMove = Vector3.Lerp(currentPosition, targetPosition, Time.fixedDeltaTime*lateralSmoothSpeed);

            // İleri ve yanal haraketi birleştirmek
            Vector3 combineMove = new Vector3(lateralMove.x, transform.position.y, rb.position.z) + forwardMove;
            rb.MovePosition(combineMove);
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
            rb.isKinematic = true;
            isAlive = false;
            //animator.SetBool(name: "Die", true);
            effectSourses.Stop();
            effectSourses.clip = deathClip;
            effectSourses.Play();
            
            menuCanvas.SetActive(true);
        }             
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
