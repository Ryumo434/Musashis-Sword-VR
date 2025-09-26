using UnityEngine;
using TMPro;
using System.Collections;

public class TextFlash : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    private string originalText;

    void Awake()
    {
        if (text != null)
            originalText = text.text;
    }

    public void ShowTemporaryMessage(string message, float duration = 2f)
    {
        if (text != null)
            StartCoroutine(FlashRoutine(message, duration));
    }

    private IEnumerator FlashRoutine(string message, float duration)
    {
        string prev = text.text;
        text.text = message;

        yield return new WaitForSeconds(duration);

        text.text = prev;
    }
}