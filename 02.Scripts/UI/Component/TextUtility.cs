using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class TextUtility
{
    public static string CleanLineBreaks(string text)
    {
        return text.Replace("Wn", "\n").Replace("\\n", "\n").Replace(@"\n", "\n");
    }
    
    public static IEnumerator AnimatorText(TextMeshProUGUI target,string text, Action onComplete)
    {
        target.text = string.Empty;

        StringBuilder stringBuilder = new();

        for (int i = 0; i < text.Length; i++)
        {
            char currentChar = text[i];

            if (currentChar == '\n') // 줄바꿈을 만났을 경우, 입력 대기
            {
                stringBuilder.Append(currentChar);
                target.text = stringBuilder.ToString();

                yield return new WaitUntil(() => Input.anyKeyDown);

            }
            else
            {
                stringBuilder.Append(text[i]);
                target.text = stringBuilder.ToString();
                yield return new WaitForSeconds(0.05f);
            }
        }
        yield return new WaitUntil(() => Input.anyKeyDown);
        
        onComplete?.Invoke();
    }
    public static IEnumerator AnimatorText(TextMeshProUGUI target,string text ,Button button)
    {
        target.text = string.Empty;

        StringBuilder stringBuilder = new();

        for (int i = 0; i < text.Length; i++)
        {
            char currentChar = text[i];

            if (currentChar == '\n') // 줄바꿈을 만났을 경우, 입력 대기
            {
                stringBuilder.Append(currentChar);
                target.text = stringBuilder.ToString();

                yield return new WaitUntil(() => Input.anyKeyDown);

            }
            else
            {
                stringBuilder.Append(text[i]);
                target.text = stringBuilder.ToString();
                yield return new WaitForSeconds(0.05f);
            }
        }
        button.gameObject.SetActive(true);

    }
}
