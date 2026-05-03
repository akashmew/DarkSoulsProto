// ArrowScript.cs
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] private float speed = 30f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private LayerMask hitMask=new LayerMask();

    private Rigidbody _rb;
    private bool _hasHit;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;
    }

    public void Launch(Vector3 direction)
    {
        _rb.isKinematic = false;
        _rb.velocity = direction * speed;
        //_rb.useGravity = true;

        //Rotate arrow to face direction
        if (direction != Vector3.zero)
            transform.forward = direction;

     
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (_hasHit) return;

        // Arrow rotates to follow its arc
        if (_rb.velocity != Vector3.zero)
            transform.forward = _rb.velocity.normalized;

        Debug.DrawLine(transform.position, transform.position +_rb.velocity.normalized * 2f, Color.blue,4f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasHit) return;
        _hasHit = true;
        Debug.Log("Has hit");
        // Stick arrow into surface
        _rb.velocity = Vector3.zero;
        _rb.useGravity = false;
        _rb.isKinematic = true;

        // Optional: destroy after stuck delay
        Destroy(gameObject, 10f);
    }
}