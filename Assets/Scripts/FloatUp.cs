using UnityEngine;
using UnityEngine.UI;

public class FloatUpImage : MonoBehaviour
{
    public float floatSpeed = 150f;
    public float lifetime = 1.5f;
    public bool enableSpin = true;
    public float spinSpeed = 180f;
    
    private Image imageComponent;
    private Vector3 startPosition;
    private float timer = 0f;
    
    void Start()
    {
        imageComponent = GetComponent<Image>();
        if (imageComponent == null)
        {
            Debug.LogError("FloatUpImage script requires Image component!");
            Destroy(gameObject);
            return;
        }
        
        // Start position - spawn from below the target
        startPosition = transform.localPosition;
        transform.localPosition = startPosition + Vector3.down * 100f; // Start 100 units below
        
        // Start invisible and small
        Color color = imageComponent.color;
        color.a = 0f;
        imageComponent.color = color;
        transform.localScale = Vector3.zero;
    }
    
    void Update()
    {
        if (imageComponent == null) return;
        
        timer += Time.deltaTime;
        float progress = timer / lifetime;
        
        // PHASE 1: Pop in and bounce up (first 30% of lifetime)
        if (progress < 0.3f)
        {
            float popProgress = progress / 0.3f;
            
            // Scale: Pop from 0 to 1.2, then settle to 1.0
            float scale = Mathf.Lerp(0f, 1.2f, Mathf.Min(popProgress * 2f, 1f));
            if (popProgress > 0.5f)
            {
                scale = Mathf.Lerp(1.2f, 1.0f, (popProgress - 0.5f) * 2f);
            }
            transform.localScale = Vector3.one * scale;
            
            // Fade in quickly
            Color color = imageComponent.color;
            color.a = Mathf.Lerp(0f, 1f, popProgress * 2f);
            imageComponent.color = color;
            
            // Bounce upward with easing
            float bounceHeight = EaseOutBounce(popProgress) * 200f;
            transform.localPosition = startPosition + Vector3.up * bounceHeight;
        }
        // PHASE 2: Float and fade (remaining 70% of lifetime)
        else
        {
            float fadeProgress = (progress - 0.3f) / 0.7f;
            
            // Continue floating upward smoothly
            float floatHeight = 200f + (fadeProgress * floatSpeed);
            transform.localPosition = startPosition + Vector3.up * floatHeight;
            
            // Fade out in last 40% of lifetime
            if (fadeProgress > 0.6f)
            {
                float fadeOutProgress = (fadeProgress - 0.6f) / 0.4f;
                Color color = imageComponent.color;
                color.a = Mathf.Lerp(1f, 0f, fadeOutProgress);
                imageComponent.color = color;
            }
        }
        
        // Spin throughout
        if (enableSpin)
        {
            transform.Rotate(0, 0, spinSpeed * Time.deltaTime);
        }
        
        // Destroy when done
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
    
    // Bounce easing function
    float EaseOutBounce(float t)
    {
        if (t < 1f / 2.75f)
        {
            return 7.5625f * t * t;
        }
        else if (t < 2f / 2.75f)
        {
            t -= 1.5f / 2.75f;
            return 7.5625f * t * t + 0.75f;
        }
        else if (t < 2.5f / 2.75f)
        {
            t -= 2.25f / 2.75f;
            return 7.5625f * t * t + 0.9375f;
        }
        else
        {
            t -= 2.625f / 2.75f;
            return 7.5625f * t * t + 0.984375f;
        }
    }
}