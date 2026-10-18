using System.Collections;
using UnityEngine;

public class birthdaycard : MonoBehaviour
{
    public float popupDuration = 1f;
    public float startYOffset = 700f;
    public float startScale = 0.1f;
    public float overshootScale = 1.08f;

    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private Vector3 originalScale;
    private Coroutine popupCoroutine;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;
        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        rectTransform.anchoredPosition = originalPosition + Vector2.down * startYOffset;
        transform.localScale = originalScale * startScale;

        if (popupCoroutine != null)
        {
            StopCoroutine(popupCoroutine);
        }

        popupCoroutine = StartCoroutine(popup_animation());
    }

    private IEnumerator popup_animation()
    {
        float time = 0f;

        Vector2 startPosition = originalPosition + Vector2.down * startYOffset;
        Vector3 smallScale = originalScale * startScale;
        Vector3 bigScale = originalScale * overshootScale;

        while (time < popupDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / popupDuration);

            float moveT = 1f - Mathf.Pow(1f - t, 3f);
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, originalPosition, moveT);

            if (t < 0.8f)
            {
                float scaleT = t / 0.8f;
                scaleT = 1f - Mathf.Pow(1f - scaleT, 3f);
                transform.localScale = Vector3.Lerp(smallScale, bigScale, scaleT);
            }
            else
            {
                float scaleT = (t - 0.8f) / 0.2f;
                transform.localScale = Vector3.Lerp(bigScale, originalScale, scaleT);
            }

            yield return null;
        }

        rectTransform.anchoredPosition = originalPosition;
        transform.localScale = originalScale;
        popupCoroutine = null;
    }
}