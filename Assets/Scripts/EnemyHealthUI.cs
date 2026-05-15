using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private GameObject fillImageParent;

    public void UpdateHealth(float current, float max)
    {
        float currenFillAmount = current / max;
       fillImageParent.SetActive(true);
       StartCoroutine(UpdateHealthBar(currenFillAmount));
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

        t = 0;
        Image parenTimg = fillImageParent.GetComponent<Image>();
        Color color=default;
        Color colorF = default;
        while (t<1)
        {
            
             color = parenTimg.color;
             colorF = fillImage.color;
            color.a = Mathf.Lerp(parenTimg.color.a, 0f, t);
            colorF.a = Mathf.Lerp(parenTimg.color.a, 0f, t);
            parenTimg.color = color;
            fillImage.color = colorF;
            t += Time.deltaTime;
            yield return null;
        }

        color.a = 1;
        colorF.a = 1;
        fillImage.color = colorF;
        parenTimg.color = color;
        fillImageParent.SetActive(false);
        
    }
}