using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    public void UpdateHealth(float current, float max)
    {
        float currentFillAmount = current / max;
        StartCoroutine(UpdateHealthBar(currentFillAmount));
    }
    
    IEnumerator UpdateHealthBar(float currenFillAmount)
    {
        float t = 0;
        while (t < 1)
        {
            fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, currenFillAmount, t);
            t += Time.deltaTime;
            yield return null;
        }
    }
}