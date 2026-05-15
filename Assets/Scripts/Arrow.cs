using System.Collections;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] private float speed  = 30f;
    [SerializeField] private float damage = 25f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private LayerMask hitMask = new LayerMask();
    [SerializeField] private ParticleSystem flame;

    private Rigidbody _rb;
    private TrailRenderer _trail;
    private Collider _collider;
    private bool _hasHit;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;
        _trail = GetComponentInChildren<TrailRenderer>();
        _collider = GetComponentInChildren<Collider>();
    }

    public void Launch(Vector3 direction)
    {
        _rb.isKinematic = false;
        _rb.velocity = direction * speed;

        if (direction != Vector3.zero)
            transform.forward = direction;

        if (_trail != null)
            _trail.enabled = true;

        if (_collider != null)
        {
            _collider.enabled = false;
            StartCoroutine(EnableColliderAfterDelay(0.1f));
        }

        Destroy(gameObject, lifetime);
    }

    private IEnumerator EnableColliderAfterDelay(float delay)
    {
        
        yield return new WaitForSeconds(delay);
       
        if (_collider != null)
            _collider.enabled = true;
    }

    private void Update()
    {
        if (_hasHit) return;

        if (_rb.velocity != Vector3.zero)
            transform.forward = _rb.velocity.normalized;

        Debug.DrawLine(transform.position, transform.position + _rb.velocity.normalized * 2f, Color.blue, 4f);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (_hasHit) return;
        _hasHit = true;

        _rb.velocity = Vector3.zero;
        _rb.useGravity = false;
        _rb.isKinematic = true;

        if (_trail != null)
            _trail.enabled = false;

        // deal damage if we hit an enemy
        EnemyHealth enemy = other.collider.GetComponentInParent<EnemyHealth>();
        enemy?.TakeDamage(damage);

        if (other.contacts.Length > 0)
            SpawnImpactEffect(other.contacts[0]);

        Destroy(gameObject, 10f);
    }

    private void SpawnImpactEffect(ContactPoint contact)
    {
        GameObject fx = new GameObject("ImpactFX");
        fx.transform.position = contact.point;
        // orient hemisphere to spray away from the surface
        fx.transform.rotation = Quaternion.LookRotation(contact.normal);

        ParticleSystem ps = fx.AddComponent<ParticleSystem>();
        ParticleSystemRenderer r = fx.GetComponent<ParticleSystemRenderer>();

        // use a URP-compatible unlit particle shader so it never shows as pink
        r.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        r.material.SetFloat("_Surface", 1); // transparent blend mode

        // --- main ---
        var main = ps.main;
        main.duration = 0.4f;
        main.loop = false;
        main.startLifetime  = new ParticleSystem.MinMaxCurve(0.35f, 0.7f);
        main.startSpeed     = new ParticleSystem.MinMaxCurve(1.5f, 6f);
        main.startSize      = new ParticleSystem.MinMaxCurve(0.04f, 0.18f);
        main.startRotation  = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
        main.startColor     = new ParticleSystem.MinMaxGradient(
            new Color(0.55f, 0.47f, 0.38f),   // dark dusty brown
            new Color(0.82f, 0.76f, 0.68f)    // light stone grey
        );
        main.gravityModifier  = 0.7f;
        main.maxParticles     = 40;
        main.stopAction       = ParticleSystemStopAction.Destroy;

        // --- emission: single burst ---
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 28) });

        // --- shape: hemisphere spraying along the surface normal ---
        var shape = ps.shape;
        shape.enabled   = true;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        shape.radius    = 0.06f;

        // --- fade out over lifetime ---
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(Color.red, 0f), new GradientColorKey(Color.red, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 0.65f) }
        );
        col.color = new ParticleSystem.MinMaxGradient(grad);
        flame.gameObject.SetActive(false);
        // --- shrink towards end of life ---
        var sol = ps.sizeOverLifetime;
        sol.enabled = true;
        sol.size = new ParticleSystem.MinMaxCurve(1f,
            new AnimationCurve(new Keyframe(0f, 1f, 0f, -2f), new Keyframe(1f, 0f)));

        // --- small secondary burst for fine dust ---
        var subGo = new GameObject("DustLayer");
        subGo.transform.SetParent(fx.transform, false);
        var subPs = subGo.AddComponent<ParticleSystem>();
        var subR = subGo.GetComponent<ParticleSystemRenderer>();
        subR.material = r.material;

        var subMain = subPs.main;
        subMain.duration        = 0.4f;
        subMain.loop            = false;
        subMain.startLifetime   = new ParticleSystem.MinMaxCurve(0.5f, 1.0f);
        subMain.startSpeed      = new ParticleSystem.MinMaxCurve(0.3f, 2.0f);
        subMain.startSize       = new ParticleSystem.MinMaxCurve(0.06f, 0.25f);
        subMain.startColor      = new ParticleSystem.MinMaxGradient(
            new Color(0.7f, 0.65f, 0.55f, 0.6f),
            new Color(0.9f, 0.87f, 0.80f, 0.3f)
        );
        subMain.gravityModifier = 0.15f;
        subMain.maxParticles    = 20;
        subMain.stopAction      = ParticleSystemStopAction.Destroy;

        var subEmission = subPs.emission;
        subEmission.rateOverTime = 0;
        subEmission.SetBursts(new[] { new ParticleSystem.Burst(0f, 14) });

        var subShape = subPs.shape;
        subShape.enabled   = true;
        subShape.shapeType = ParticleSystemShapeType.Hemisphere;
        subShape.radius    = 0.12f;

        var subCol = subPs.colorOverLifetime;
        subCol.enabled = true;
        var subGrad = new Gradient();
        subGrad.SetKeys(
            new[] { new GradientColorKey(Color.red, 0f), new GradientColorKey(Color.red, 1f) },
            new[] { new GradientAlphaKey(0.6f, 0f), new GradientAlphaKey(0f, 0.5f) }
        );
        subCol.color = new ParticleSystem.MinMaxGradient(subGrad);

        ps.Play();
        subPs.Play();
        Destroy(fx, 2f);
    }
}
