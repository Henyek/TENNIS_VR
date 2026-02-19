using UnityEngine;

public class EnhancedBallController : MonoBehaviour
{
    [Header("Ball Speed Settings")]
    public float speed = 5f;
    
    [Header("Materials")]
    public Material blueMaterial;
    public Material yellowMaterial;
    public Material redMaterial;
    
    [Header("Visual Effects")]
    public LineRenderer trajectoryLine;
    
    private Renderer ballRenderer;
    private Rigidbody rb;
    public Vector3 direction;
    private bool isGlowing = false;
    private float glowTimer = 0f;
    private bool hasBeenHit = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        ballRenderer = GetComponent<Renderer>();

        // Only set a default direction/velocity if none was provided by spawner
        if (direction == Vector3.zero)
        {
            direction = Vector3.back; // sensible default
        }

        if (rb != null && rb.linearVelocity.sqrMagnitude < 0.0001f)
        {
            rb.linearVelocity = direction * speed;
        }
        
        SetBallColor();
        
        // Don't auto-glow - let GameManager handle inactivity
    }
   
    
    void Update()
    {
        // Update trajectory line if enabled
        if (trajectoryLine != null && trajectoryLine.enabled)
        {
            UpdateTrajectoryLine();
        }
        
        // Handle glow effect
        if (isGlowing)
        {
            glowTimer += Time.deltaTime;
            float glowIntensity = Mathf.PingPong(glowTimer * 2f, 1f);
            ballRenderer.material.SetColor("_EmissionColor", Color.yellow * glowIntensity);
        }
    }
    void SetBallColor()
    {
        if (speed <= 5f)
            ballRenderer.material = blueMaterial;
        else if (speed <= 10f)
            ballRenderer.material = yellowMaterial;
        else
            ballRenderer.material = redMaterial;
            
        // Enable emission for glow effect
        ballRenderer.material.EnableKeyword("_EMISSION");
    }
    
    void UpdateTrajectoryLine()
    {
        // Show trajectory prediction (straight line - no gravity)
        int segments = 20;
        trajectoryLine.positionCount = segments;
        
        Vector3 currentPos = transform.position;
        Vector3 currentVel = rb.linearVelocity;
        
        for (int i = 0; i < segments; i++)
        {
            float time = i * 0.1f;
            // Straight line projection (no gravity since ball doesn't use gravity)
            Vector3 pos = currentPos + currentVel * time;
            trajectoryLine.SetPosition(i, pos);
        }
    }
    
    public void EnableTrajectoryGuide(bool enable)
    {
        if (trajectoryLine != null)
            trajectoryLine.enabled = enable;
    }
    
    public void EnableGlow(bool enable)
    {
        isGlowing = enable;
        if (!enable && ballRenderer != null)
        {
            ballRenderer.material.SetColor("_EmissionColor", Color.black);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Racket"))
        {
            hasBeenHit = true;
            isGlowing = false;
            
            // Calculate hit quality based on racket velocity
            Rigidbody racketRb = other.GetComponentInParent<Rigidbody>();
            float racketSpeed = racketRb != null ? racketRb.linearVelocity.magnitude : 0f;
            
            // Bounce ball back with physics
            Vector3 bounceDirection = Vector3.Reflect(rb.linearVelocity.normalized, other.transform.up);
            float bounceSpeed = Mathf.Max(speed * 0.8f, racketSpeed * 0.5f);
            rb.linearVelocity = bounceDirection * bounceSpeed;
            
            // Add some randomness to bounce
            rb.linearVelocity += new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(0f, 2f),
                Random.Range(-1f, 1f)
            );
            
            // Notify GameManager
            GameManager.Instance?.OnBallHit(speed, transform.position, racketSpeed > 2f);
            
            // Change color as bonus feedback
            if (Random.value > 0.7f)
            {
                ballRenderer.material.color = Color.Lerp(ballRenderer.material.color, Color.white, 0.5f);
            }
            
            // Destroy after bouncing away
            Destroy(gameObject, 3f);
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // Bounce off walls and floor
        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Floor"))
        {
            // Physics will handle the bounce naturally due to Rigidbody
            // Just add a bit of dampening
            rb.linearVelocity *= 0.8f;
        }
    }
    
    // Auto-destroy if ball goes too far
    void OnBecameInvisible()
    {
        // If ball wasn't hit and becomes invisible, it was missed
        if (!hasBeenHit)
        {
            if(GameManager.Instance != null)
            {
                GameManager.Instance?.OnBallMissed();
            }
        }
        Destroy(gameObject, 1f);
    }
}