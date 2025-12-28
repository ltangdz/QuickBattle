using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarUI : BaseUI
{
    [Space(10)]
    [Header("子UI控件")]
    [Tooltip("填充条")]
    public Image barImage;
    [Tooltip("填充文字")]
    public TextMeshProUGUI progressText;

    public void SetFillAmount(float fillAmount)
    {
        barImage.fillAmount = fillAmount;
    }

    public void SetText(string text)
    {
        progressText.text = text;
    }
}
