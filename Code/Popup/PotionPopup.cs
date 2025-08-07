using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 포션의 상세 정보를 보여주는 팝업
/// </summary>
public class PotionPopup : BasePopup
{
    [SerializeField] private Image iconImage; //포션 이미지
    [SerializeField] private TextMeshProUGUI potionNameText; //포션 이름
    [SerializeField] private TextMeshProUGUI potionDescriptionText; //포션 설명
    [SerializeField] private TextMeshProUGUI[] potionMaterialText; //포션 제조 재료
    [SerializeField] private Image[] materialIconImage; // 재료 이미지

    public void Show(int id) //슬롯에서 넘겨준 Id로 상세 정보를 표시
    {
        base.Show();

        DataManager dataManager = DataManager.Instance;
        var itemDataDic = dataManager.ItemDataDic;
        iconImage.sprite = dataManager.GetSprite(SpriteType.Item,itemDataDic[id].SpriteName); //id와 맞는 포션 이미지
        potionNameText.text = itemDataDic[id].Name; //id와 맞는 포션 이름
        potionDescriptionText.text = itemDataDic[id].Description; //id와 맞는 포션 설명

        var mixtureData = DataManager.Instance.PotionMixtureDataDic.Values.FirstOrDefault(m => m.PotionId == id); //id와 맞는 포션 제조 재료(조합 정보)
                                                                                                                  //여러 개의 데이터를 합쳐야 하기 때문에
        var text1 = transform.Find("MaterialComment")?.GetComponent<TextMeshProUGUI>();
        text1.gameObject.SetActive(true);
        foreach (var text in GetComponentsInChildren<TextMeshProUGUI>(false))
        {
            if (text.name == "+")
            {
                text.gameObject.SetActive(true);
            }
        }
        for (int i = 0; i < potionMaterialText.Length; i++)
        {
            potionMaterialText[i].gameObject.SetActive(true);
            materialIconImage[i].gameObject.SetActive(true);
        }
        if (id == ItemId.TrashPotionId)
        {
            var text2 = transform.Find("MaterialComment")?.GetComponent<TextMeshProUGUI>();
            text2.gameObject.SetActive(false);
            foreach (var text in GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (text.name == "+")
                {
                    text.gameObject.SetActive(false);
                }
            }
            for (int i = 0; i < potionMaterialText.Length; i++)
            {
                potionMaterialText[i].text = "";
                potionMaterialText[i].gameObject.SetActive(false);
                materialIconImage[i].gameObject.SetActive(false);
            }
            return;
        }

        if (mixtureData != null)
        {
            //var materialNames = mixtureData.MaterialIds.Select(id => DataManager.Instance.ItemDataDic.TryGetValue(id, out var item) ? item.Name : $"Unknown({id})"); //id에 해당하는 아이템이 있는 지 확인하고, 있다면 item에 결과를 저장
            for (int i = 0; i < mixtureData.MaterialIds.Count; i++)
            {
                potionMaterialText[i].text = itemDataDic[mixtureData.MaterialIds[i]].Name;
                materialIconImage[i].sprite = dataManager.GetSprite(SpriteType.Item,itemDataDic[mixtureData.MaterialIds[i]].SpriteName);
            }
        }
        else
        {
            potionMaterialText[0].text = "조합 정보 없음";
            potionMaterialText[1].text = "조합 정보 없음";
            potionMaterialText[2].text = "조합 정보 없음";
        }
    }
}
