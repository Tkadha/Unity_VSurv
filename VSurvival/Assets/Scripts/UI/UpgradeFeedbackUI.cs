using System.Collections;
using TMPro;
using UnityEngine;

public class UpgradeFeedbackUI : MonoBehaviour
{
    [SerializeField] private GameObject rootObject;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private float displayDuration = 1.2f;

    private Coroutine hideCoroutine;

    private void Awake()
    {
        HideImmediate();
    }

    public void ShowMessage(string message)
    {
        if (feedbackText != null)
            feedbackText.text = message;

        if (rootObject != null)
            rootObject.SetActive(true);

        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        hideCoroutine = StartCoroutine(HideRoutine());
    }

    public void HideImmediate()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        if (rootObject != null)
            rootObject.SetActive(false);
    }

    private IEnumerator HideRoutine()
    {
        yield return new WaitForSecondsRealtime(displayDuration);

        if (rootObject != null)
            rootObject.SetActive(false);

        hideCoroutine = null;
    }
}