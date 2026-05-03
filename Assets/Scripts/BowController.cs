using UnityEngine;
using Cinemachine;

public class BowController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform arrowSpawnPoint;
    [SerializeField] private GameObject arrowPrefab;
    //[SerializeField] private GameObject bowMesh;
    [SerializeField] private LayerMask aimColliderMask = new LayerMask();

    [Header("Bow Settings")]
    [SerializeField] private float drawTime = 0.5f;       // time to fully draw bow
    [SerializeField] private float maxArrowForce = 30f;
    [SerializeField] private float minArrowForce = 10f;

    private static readonly int IsAimingHash   = Animator.StringToHash("IsAiming");
    private static readonly int IsShootingHash = Animator.StringToHash("shoot");

    private Animator _animator;
    private PlayerAiming _playerAiming;

    private bool _isAiming;
    private bool _isDrawing;
    private bool arrowPlaced;
    private float _drawProgress;
    private float elapsedTime;// 0 to 1
    private bool canDrawAgain = true;

    [SerializeField] Transform debugTransform;

    private void Awake()
    {
        _animator      = GetComponent<Animator>();
        _playerAiming  = GetComponent<PlayerAiming>();

        // Hide bow when not aiming
        //bowMesh.SetActive(false);
    }

    private void Update()
    {
        _isAiming = Input.GetMouseButton(1);

        // Show/hide bow with aim
        //bowMesh.SetActive(_isAiming);

        if (_isAiming)
        {
            if (!arrowPlaced && canDrawAgain)
            {
                PlaceArrow();
                Debug.Log("Called how many times");
               
            }

            if (!_isAiming && currentArrow != null)
            {
                Destroy(currentArrow);
                currentArrow = null;
                arrowPlaced = false;
            }

            HandleDraw();
        }
        else
        {
            // Reset draw if released without shooting
            _drawProgress = 0f;
            _isDrawing    = false;
        }

        if(arrowPlaced && !canDrawAgain)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime > 3f)
            {
                canDrawAgain = true;
                Debug.Log("Was this called"+ elapsedTime);
            }

        }

    }

    GameObject currentArrow;

    private void PlaceArrow()
    {
       
        currentArrow = Instantiate(arrowPrefab,arrowSpawnPoint);
        currentArrow.transform.localPosition = new Vector3(0.195f,0,0);
        currentArrow.transform.localRotation = Quaternion.Euler(0,90,0);

        Rigidbody rb = currentArrow.GetComponent<Rigidbody>();
        rb.isKinematic = true;
        arrowPlaced = true;
        canDrawAgain = false;
    }

    private void HandleDraw()
    {
        // Left click to draw and shoot
        if (Input.GetMouseButtonDown(0))
        {
            _isDrawing = true;
        }

        if (_isDrawing)
        {
            _drawProgress += Time.deltaTime / drawTime;
            _drawProgress  = Mathf.Clamp01(_drawProgress);
            Debug.Log(_drawProgress);

            // Optional: drive a draw blend parameter for animation
            _animator.SetFloat("DrawProgress", _drawProgress);
            Debug.Log("the arrow draw is "+ (1-_drawProgress));
            currentArrow.transform.localPosition = Vector3.Lerp(currentArrow.transform.localPosition, new Vector3(0,0,0), _drawProgress);
        }

        if (Input.GetMouseButtonUp(0) && _isDrawing)
        {
            Shoot();
            _isDrawing    = false;
            _drawProgress = 0f;
            _animator.SetFloat("DrawProgress", _drawProgress);
        }
        
    }

    private void Shoot()
    {
        _animator.SetTrigger(IsShootingHash);
        elapsedTime = Time.time;
        // Actual arrow spawn is called from Animation Event: OnArrowRelease()
    }

    // Called by Animation Event on the shoot clip at arrow release frame
    public void OnArrowRelease()
    {
        Vector3 aimPoint  = GetArrowAimPoint();
        Vector3 direction = (aimPoint - arrowSpawnPoint.position).normalized;
        Debug.DrawRay(arrowSpawnPoint.position, direction*30f, Color.green,4f);

        


        float force = Mathf.Lerp(minArrowForce, maxArrowForce, _drawProgress);
        currentArrow.transform.SetParent(null);
        currentArrow.GetComponent<Arrow>().Launch(direction*force);
       
        
        _animator.ResetTrigger(IsShootingHash);
        currentArrow = null;
        arrowPlaced = false;
       
    }

    private Vector3 GetArrowAimPoint()
    {
        Ray ray = Camera.main.ScreenPointToRay(
            new Vector3(Screen.width / 2f, Screen.height / 2f, 0f)
        );

        if (Physics.Raycast(ray, out RaycastHit hit, 999f, aimColliderMask))
           // debugTransform.position = hit.point;
            return hit.point;

        return ray.GetPoint(50f);
    }

    private void DrawTrajectory(Vector3 startPos, Vector3 velocity)
    {
        Vector3 pos = startPos;
        Vector3 vel = velocity;

        for (int i = 0; i < 30; i++)
        {
            Vector3 nextPos = pos + vel * 0.1f;
            vel += Physics.gravity * 0.1f;

            Debug.DrawLine(pos, nextPos, Color.yellow, 2f);

            pos = nextPos;
        }
    }
}