using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class SicknessSlot : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private TextMeshProUGUI sicnessNameText;

    [SerializeField]
    private GameObject lockBlocker; //질병 슬롯 잠금(버튼 UI)

    private int itemId;
    private bool isUnlocked = false;

    private Action<int> onClick;

    public void Initialize(SicknessData sickenssData, Action<int> action, bool unlocked)
    {
        isUnlocked = unlocked;

        sicnessNameText.text = sickenssData.Name;

        itemId = sickenssData.Id;
        onClick = action;

        lockBlocker.SetActive(!isUnlocked);

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SoundManager.Instance.PlaySound(SoundType.SFX, SoundID.SFX_Click);  //버튼클릭사운드
        if (onClick != null)
        {
            onClick?.Invoke(itemId);
        }
    }
}
