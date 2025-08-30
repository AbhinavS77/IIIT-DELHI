using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BlockHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    [SerializeField] float currentHealth;

    [Header("UI")]
    public Slider healthSlider;         // assign a Slider (child) in Inspector
    public float smoothTime = 0.25f;    // time to animate slider change (seconds)

    [Header("Death")]
    public bool destroyOnZero = true;
    public float destroyDelay = 0.1f;   // optional delay before destroy
    public UnityEvent onDeath;          // hook for other behaviors (particle, sound...)

    Coroutine sliderAnim;

    void Awake()
    {
        currentHealth = maxHealth;
        // Try auto-find slider in children if not assigned
        if (healthSlider == null)
            healthSlider = GetComponentInChildren<Slider>(true);

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    // PUBLIC: call this from other scripts to apply damage
    public void TakeDamage(float damage)
    {
        if (damage <= 0f) return;

        float previousHealth = currentHealth;
        currentHealth = Mathf.Max(0f, currentHealth - damage);

        // animate slider smoothly from previous to new value
        if (healthSlider != null)
        {
            if (sliderAnim != null) StopCoroutine(sliderAnim);
            sliderAnim = StartCoroutine(AnimateSlider(previousHealth, currentHealth));
        }

        if (currentHealth <= 0f)
            HandleDeath();
    }

    IEnumerator AnimateSlider(float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < smoothTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / smoothTime);
            float value = Mathf.Lerp(from, to, t);
            healthSlider.value = value;
            yield return null;
        }
        healthSlider.value = to;
        sliderAnim = null;
    }

    void HandleDeath()
    {
        onDeath?.Invoke();

        if (destroyOnZero)
            Destroy(gameObject, destroyDelay);
    }

    // optional helper: restore health (not requested but handy)
    public void HealTo(float newHealth)
    {
        float prev = currentHealth;
        currentHealth = Mathf.Clamp(newHealth, 0f, maxHealth);
        if (healthSlider != null)
        {
            if (sliderAnim != null) StopCoroutine(sliderAnim);
            sliderAnim = StartCoroutine(AnimateSlider(prev, currentHealth));
        }
    }
}
