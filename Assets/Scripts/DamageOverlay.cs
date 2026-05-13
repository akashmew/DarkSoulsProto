using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DamageOverlay : MonoBehaviour
{
    [SerializeField] private Image overlayImage;

    [SerializeField] private float flashAlpha = 0.5f;
    [SerializeField] private float fadeSpeed = 2f;

    private Coroutine currentRoutine;

    public void ShowDamage()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        Color color = overlayImage.color;

        color.a = flashAlpha;
        overlayImage.color = color;
        overlayImage.transform.parent.gameObject.SetActive(true);

        while (overlayImage.color.a > 0)
        {
            color.a -= Time.deltaTime * fadeSpeed;

            overlayImage.color = color;

            yield return null;
        }
        overlayImage.transform.parent.gameObject.SetActive(false);
    }
}