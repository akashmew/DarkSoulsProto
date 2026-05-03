using UnityEngine;
using Cinemachine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class PlayerAiming : MonoBehaviour
{
    [Header("Cinemachine")]
    [SerializeField] private CinemachineFreeLook freeLookCam;
    [SerializeField] private CinemachineVirtualCamera aimCam;

    [Header("Aim Settings")]
    [SerializeField] private float aimMouseSensitivity = 2f;
    [SerializeField] private float verticalClampMin = -30f;
    [SerializeField] private float verticalClampMax = 60f;

    [Header("Shoulder Offset")]
    [SerializeField] private float rightOffset = 0.7f;
    [SerializeField] private float upOffset = 1.5f;
    [SerializeField] private float backOffset = 2.5f;

    [Header("Aim Settings")]
    [SerializeField] private LayerMask aimColliderMask;
    [SerializeField] private Image crosshair;

    private static readonly int IsAimingHash = Animator.StringToHash("isAiming");
    private static readonly int IsShoot = Animator.StringToHash("shoot");

    private Animator _animator;
    private PlayerMovement _movement;

    private bool _isAiming;
    private bool _wasAiming;

    private bool _shoot;

    private float _yaw;
    private float _pitch;
    private float _aimEntryTime;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _movement = GetComponent<PlayerMovement>();
        _yaw = transform.eulerAngles.y;
        crosshair.gameObject.SetActive(false);
    }

    private void Update()
    {
        _isAiming = Input.GetMouseButton(1);
        

        // if (Input.GetMouseButtonDown(0))
        // {
        //     ShootArrow();
        // }
        
        _animator.SetBool(IsAimingHash, _isAiming);
       
        aimCam.Priority = _isAiming ? 11 : 9;
        //_movement.IsAiming = _isAiming;

        if (_isAiming && !_wasAiming)
            OnAimEnter();

        if (!_isAiming && _wasAiming)
            OnAimExit();

        if (_isAiming)
        {
            HandleMouseInput();

            if (Time.time > _aimEntryTime + 0.1f)
                RotateCharacterToAim();
        }

        _wasAiming = _isAiming;
    }

    private void ShootArrow()
    {
        _shoot = true;
        _animator.SetBool(IsShoot, _shoot);
        _shoot =false;
        _animator.SetBool(IsShoot, _shoot);
    }

    private void LateUpdate()
    {
        if (!_isAiming) return;

        // Build camera position from yaw/pitch directly
        // No pivot needed — we just place the camera manually
        Quaternion rotation  = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 offset       = rotation * new Vector3(rightOffset, upOffset, -backOffset);
        Vector3 shoulderBase = transform.position + Vector3.up * upOffset;

        aimCam.transform.position = shoulderBase + rotation * new Vector3(rightOffset, 0f, -backOffset);
        aimCam.transform.rotation = rotation;
    }

    private void OnAimEnter()
    {
        // Sync yaw to freeLook's current angle
        float raw = freeLookCam.m_XAxis.Value;
        _yaw      = raw > 180f ? raw - 360f : raw;
        _pitch    = 0f;
        _aimEntryTime = Time.time;
        crosshair.gameObject.SetActive(true);
    }

    private void OnAimExit()
    {
        float yawOut = _yaw < 0f ? _yaw + 360f : _yaw;
        freeLookCam.m_XAxis.Value = yawOut;
        crosshair.gameObject.SetActive(false);
    }

    private void HandleMouseInput()
    {
        _yaw   += Input.GetAxis("Mouse X") * aimMouseSensitivity;
        _pitch -= Input.GetAxis("Mouse Y") * aimMouseSensitivity;
        _pitch  = Mathf.Clamp(_pitch, verticalClampMin, verticalClampMax);
    }

    private void RotateCharacterToAim()
    {
        // Rotate character body to face horizontal aim direction only
        Quaternion flatRot    = Quaternion.Euler(0f, _yaw, 0f);
        transform.rotation    = Quaternion.Slerp(
            transform.rotation,
            flatRot,
            Time.deltaTime * 15f
        );
    }

    private Vector3 GetAimPoint()
    {
        Ray ray = Camera.main.ScreenPointToRay(
            new Vector3(Screen.width / 2f, Screen.height / 2f, 0f)
        );

        if (Physics.Raycast(ray, out RaycastHit hit, 999f, aimColliderMask))
            return hit.point;

        return ray.GetPoint(50f);
    }
}