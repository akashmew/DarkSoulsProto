using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Humanoid player movement controller with idle and walking animations.
/// Requires: CharacterController component, Animator with "Speed" float parameter.
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float rotationSpeed = 10f;
    private bool _isRolling = false;
    private bool _isAiming = false;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private LayerMask groundMask;

    [Header("Animation")]
    [SerializeField] private float animationSmoothTime = 0.1f;

    // -- Animator parameter hashes (cached for performance) --
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int BowSpeedHash = Animator.StringToHash("BowSpeed");
    private static readonly int IsRollingHash = Animator.StringToHash("isRolling");
    private static readonly int DrawArrow = Animator.StringToHash("isAiming");
    private static readonly int IsDamage = Animator.StringToHash("IsDamage");

    // -- Component references --
    private CharacterController _controller;
    private Animator _animator;
    private Camera _mainCamera;

    // -- Internal state --
    private Vector3 _velocity;
    private bool _isGrounded;
    private float _animationSpeed;        // smoothed value fed to Animator
    private float _bowAnimationSpeed;        // smoothed value fed to Animator
    private float _animationSpeedVelocity;
    private float _animationbowVelocity;
    private float _drawBowDelta;// used by SmoothDamp

    // -------------------------------------------------------
    #region Unity Lifecycle

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator   = GetComponent<Animator>();
        _mainCamera = Camera.main;

        // Auto-create a ground check point if one wasn't assigned
        if (groundCheck == null)
        {
            GameObject go = new GameObject("GroundCheck");
            go.transform.SetParent(transform);
            go.transform.localPosition = new Vector3(0f, -1f, 0f);
            groundCheck = go.transform;
        }
    }

    private void Update()
    {
        //CheckGround();
       // ApplyGravity();
        HandleMovement();
    }

    #endregion
    // -------------------------------------------------------
    #region Movement & Physics

    private void CheckGround()
    {
        _isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundCheckRadius,
            groundMask
        );

        // Reset downward velocity when grounded
        if (_isGrounded && _velocity.y < 0f)
            _velocity.y = -2f; // small negative keeps us grounded reliably
    }

    private void ApplyGravity()
    {
        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }

    private void HandleMovement()
    {
        // --- Raw input ---
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical   = Input.GetAxisRaw("Vertical");



        // Build input direction relative to camera's Y-rotation
        Vector3 inputDir = new Vector3(horizontal, 0f, vertical).normalized;
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if ( inputDir.magnitude >= 0.1f && !_isRolling  )
            {
                Debug.Log("run was printed");
                _isRolling = true;
            }
        }
        else if (inputDir.magnitude < 0.1f)
        {
            _isRolling = false;
           
        }
        
        // if (Input.GetMouseButton(1))
        // {
        //    if (inputDir.magnitude < 0.1f && !_isRolling )
        //    {
        //        _isAiming = true;
        //        _drawBowDelta += Time.deltaTime*0.5f;
        //    }
        // }
        // else
        // {
        //     _isAiming = false;
        //     _drawBowDelta -= Time.deltaTime*0.5f;
        // }
       

        if (inputDir.magnitude >= 0.1f)
        {
            // Camera-relative direction
            float targetAngle  = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg
                                 + _mainCamera.transform.eulerAngles.y;
            Vector3 moveDir    = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            // Smooth rotation towards movement direction
            Quaternion targetRot = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation   = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );

            _controller.Move(moveDir.normalized * moveSpeed * Time.deltaTime);
        }

        // Smooth the speed value sent to the Animator
        float targetAnimSpeed = inputDir.magnitude > 0.1f ? 1f : 0f;
        _animationSpeed = Mathf.SmoothDamp(
            _animationSpeed,
            targetAnimSpeed,
            ref _animationSpeedVelocity,
            animationSmoothTime
        );
        
       float targetBowAnimSpeed = _drawBowDelta > 0 ? 1 : 0;
        _bowAnimationSpeed = Mathf.SmoothDamp(
            _bowAnimationSpeed,
            targetBowAnimSpeed, 
            ref _animationbowVelocity,
            animationSmoothTime);

        UpdateAnimator(_animationSpeed);
    }

    #endregion
    // -------------------------------------------------------
    #region Animation

    private void UpdateAnimator(float speed)
    {
        _animator.SetFloat(SpeedHash, speed);
        _animator.SetBool(IsRollingHash, _isRolling);
        //_animator.SetBool(DrawArrow,_isAiming);
        //_animator.SetFloat(BowSpeedHash, _bowAnimationSpeed);
    }

    public void TakeDamage()
    {
        _animator.SetTrigger(IsDamage);
    }
    #endregion
    // -------------------------------------------------------
    #region Editor Helpers

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

    #endregion
}
