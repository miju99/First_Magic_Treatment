using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 하나의 슬롯이 어떤 포션인지 표시
/// </summary>
public class PotionSlot : MonoBehaviour
{
    [SerializeField] private Image iconImage; //포션 아이템
    [SerializeField] private TextMeshProUGUI potionNameText; //포션 이름
    [SerializeField] private TextMeshProUGUI potionDescriptionText;
    [SerializeField] private GameObject recieptGameObject;
    [SerializeField] private Image[] materialIconImage; // 재료 이미지
    [SerializeField] private TextMeshProUGUI[] potionMaterialText; //포션 제조 재료

    private int itemId; //어떤 아이템인지 기억하기 위한 ID
    private bool isUnlocked = false;

    public void Initialize(ItemData itemdata, Action<int> action, bool unlocked) //아이템 정보와 액션 함수를 저장(itemData = UI/ action = 클릭 시 실행될 함수)
    {
        DataManager dataManager = DataManager.Instance;
        var itemDataDic = dataManager.ItemDataDic;
        Sprite loadedItemSprite = DataManager.Instance.GetSprite(SpriteType.Item, itemdata.SpriteName);

        isUnlocked = unlocked;
        iconImage.sprite = loadedItemSprite;
        potionNameText.text = itemdata.Name;
        potionDescriptionText.text = $"{itemdata.Description}";
        itemId = itemdata.Id;

        var mixtureData = DataManager.Instance.PotionMixtureDataDic.Values.FirstOrDefault(m => m.PotionId == itemId);

        if (mixtureData == null)
        {
            recieptGameObject.SetActive(false);
            return;
        }
        else
        {
            for (int i = 0; i < mixtureData.MaterialIds.Count; i++)
            {
                materialIconImage[i].sprite = dataManager.GetSprite(SpriteType.Item, itemDataDic[mixtureData.MaterialIds[i]].SpriteName);
            }
        }
        if (!isUnlocked)
        {
            iconImage.color = Color.gray;
            potionNameText.color = Color.gray;
            potionDescriptionText.color = Color.gray;
            for (int i = 0; i < mixtureData.MaterialIds.Count; i++)
            {
                potionMaterialText[i].text = itemDataDic[mixtureData.MaterialIds[i]].Description2;
                materialIconImage[i].color = Color.black;
            }
        }
        else
        {
            iconImage.color = Color.white;
            potionNameText.color = Color.black;
            potionDescriptionText.color = Color.black;
            for (int i = 0; i < mixtureData.MaterialIds.Count; i++)
            {
                potionMaterialText[i].text = itemDataDic[mixtureData.MaterialIds[i]].Name;
                materialIconImage[i].color = Color.white;
            }
        }

    }
}
