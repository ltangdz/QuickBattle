using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SingleSkillUI : BaseUI
{
    [Space(10)]
    [Header("技能cd 单个UI")]
    [Tooltip("技能按钮")]
    public Button skillButton;
    [Tooltip("cd填充图片")]
    public Image cdBarImage;
    [Tooltip("cd文本")]
    public TextMeshProUGUI cdText;
    [Tooltip("当前剩余cd")]
    public float remainCd;

    public void Init()
    {
        cdBarImage.raycastTarget = false;

        SetFillAmount(0f);
        SetCdText(0f);
    }
    public void BindSkillButton(SkillType skillType)
    {
        skillButton.onClick.AddListener(() => GameInput.Instance.OnUseSkillEvent?.Invoke(this, skillType));
    }
    
    public void SetFillAmount(float fillAmount)
    {
        cdBarImage.fillAmount = fillAmount;
    }

    public void SetCdText(float cd)
    {
        // 大于1: 取整
        // 小于1: 保留1位小数
        // 小于等于0: 不显示
        if (cd <= 0f)
        {
            cdText.text = string.Empty;
        }
        else if (cd <= 1f)
        {
            cdText.text = $"{cd:F1}";
        }
        else
        {
            cdText.text = $"{(int)cd}";
        }
    }

    public IEnumerator StartCdCoroutine(float maxCd)
    {
        OnCdStart();
        
        Debug.Log("cd倒计时UI开始");
        remainCd = maxCd;
        
        // const float interval = 0.1f;
        
        while (remainCd > 0f)
        {
            // remainCd -= interval;
            remainCd -= Time.deltaTime;
            
            SetFillAmount(remainCd / maxCd);
            SetCdText(remainCd);
            
            // yield return new WaitForSeconds(interval);
            yield return Time.deltaTime;
        }
        

        OnCdCompleted();
    }

    /// <summary>
    /// cd开始时
    /// </summary>
    public void OnCdStart()
    {
        cdBarImage.raycastTarget = true;
    }

    /// <summary>
    /// cd结束时
    /// </summary>
    public void OnCdCompleted()
    {
        cdBarImage.raycastTarget = false;

        SetFillAmount(0f);
        SetCdText(0f);
    }
    
    public void StartSkillCd(object sender, EventArgs e)
    {
        if (e is SkillCdStartEventArgs skillCdStartEventArgs)
        {
            StartCoroutine(StartCdCoroutine(skillCdStartEventArgs.maxCd));
        }    
    }
}
