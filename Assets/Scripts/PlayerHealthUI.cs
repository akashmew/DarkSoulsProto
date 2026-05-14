using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    public void UpdateHealth(float current, float max)
    {
        fillImage.fillAmount = current / max;
    }
}