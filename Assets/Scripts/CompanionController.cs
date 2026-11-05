using UnityEngine;
using UnityEngine.AI;

public class CompanionController : MonoBehaviour
{
    [SerializeField] private Transform player;           // Sahnedeki Player objesi
    private Transform buttonTarget;     // Duvarın üzerindeki düğme konumu
    [SerializeField] private WallController wallController; // Duvarı indirmek için referans

    private NavMeshAgent agent;
    private Rigidbody rb;
    private Animator animator;

    private bool isMovingToButton = false;
    private bool hasReachedButton = false;
    private bool returningToPlayer = false;
    private float xOffset = 0.07f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();        

        if (agent == null)
        {
            Debug.LogError("NavMeshAgent bileşeni Companion üzerinde bulunamadı!");
        }

        // Sahneden player'ı otomatik bul (Eğer Inspector'dan atanmadıysa)
        if (player == null && GameObject.FindWithTag("Player") != null)
        {
            player = GameObject.FindWithTag("Player").transform;
        }

        // Sahneden duvar kontrolcüsünü otomatik bul
        if (wallController == null)
        {
            wallController = FindObjectOfType<WallController>();
        }

        // Fiziksel devrilmeyi engelle
        if (rb != null)
        {
            rb.freezeRotation = true;
        }
    }

    void Update()
    {
        if (player == null) return;

        // Karakterin CharacterController scriptindeki 'onPlatform' durumunu kontrol ediyoruz
        CharacterController charController = player.GetComponent<CharacterController>();
        bool isPlayerOnPlatform = (charController != null && charController.onPlatform);        
        
        // Runner Kısmı ve Platforma Varana Kadar Birlikte Koşma
        if (!isPlayerOnPlatform && !isMovingToButton && !hasReachedButton)
        {
            // Runner aşamasında NavMeshAgent kapalı olmalı ki Rigidbody ile çakışmasın/titreme yapmasın
            if (agent.enabled) agent.enabled = false;
            if (rb.isKinematic) rb.isKinematic = false;

            // Player'ın X ekseninde 0.07 yanında, Y ve Z'de ise birebir aynı konumda tutuyoruz (Kaymayı önler)
            Vector3 targetPos = new Vector3(player.position.x + xOffset, player.position.y, player.position.z);
            transform.position = targetPos; // Doğrudan pozisyon eşitlemesi kaymayı ve gecikmeyi tamamen bitirir.
        }

        //  Player Platforma Geldiği An Düğmeye Gitmeye Başlas        
        if (isPlayerOnPlatform && !hasReachedButton && !isMovingToButton)
        {
            // NavMeshAgent'ı aktif et çünkü artık bağımsız olarak düğmeye yürüyecek
            StartMovingToButton();
        }

        // Düğmeye Gitme Hareketi (NavMesh Aktif)
        if (isMovingToButton && agent.enabled && agent.isOnNavMesh && !agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude < 0.1f)
                {
                    TriggerButtonAction();
                }
            }
        }

        // Düğmeye Bastıktan Sonra Tekrar Player'ın Yanına Dönüş 
        if (returningToPlayer)
        {
            if (agent.enabled) agent.enabled = false;
            Vector3 sidePos = new Vector3(player.position.x + xOffset, player.position.y, player.position.z);
            transform.position = sidePos;
        }
    }

    void StartMovingToButton()
    {
        //if (rb != null)
        //    rb.isKinematic = true;
        
        if (buttonTarget == null)
        {
            if (!agent.enabled) agent.enabled = true;

            // Ajanın NavMesh üzerinde güvenli bir noktaya yerleşmesini sağlar
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5.0f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
            GameObject foundButton = GameObject.FindWithTag("ButtonTarget");
            if (foundButton != null)
            {
                buttonTarget = foundButton.transform;
            }
            else
            {
                Debug.LogError("Sahnesinde 'Button' etiketine sahip bir düğme bulunamadı!");
                return;
            }
        }

        if (buttonTarget != null)
        {
            agent.SetDestination(buttonTarget.position);
            Debug.Log("Companion platforma ulaşıldı, düğmeye doğru hareket ediyor.");
        }
        else
        {
            Debug.LogError("Button Target (Düğme) atanmadı!");
        }
        isMovingToButton = true;
    }

    void TriggerButtonAction()
    {
        isMovingToButton = false;
        hasReachedButton = true;

        Debug.Log("Companion düğmeye ulaştı ve bastı!");
        if (wallController == null)
        {
            wallController = FindObjectOfType<WallController>();
        }

        // Duvarı indir
        if (wallController != null)
        {
            wallController.LowerWall();
            Debug.Log("Duvar indirildi");
        }

        // Düğmeye bastıktan sonra tekrar player'ın yanına dönmesini istiyorsan:
        returningToPlayer = true;
    }
}