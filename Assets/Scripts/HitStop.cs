using System.Collections;
using UnityEngine;

public class HitStop : MonoBehaviour
{
    public static HitStop Instance;

    private Coroutine currentRoutine;

    private void Awake()
    {
        Instance = this;
    }

    public void Stop(float duration)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine =
            StartCoroutine(StopRoutine(duration));
    }

    private IEnumerator StopRoutine(float duration)
    {
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1f;
    }
}