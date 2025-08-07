using UnityEngine;
using TMPro;

public class SicknessPopup : BasePopup
{
    [SerializeField]
    private TextMeshProUGUI sicnessNameText;
    [SerializeField]
    private TextMeshProUGUI sicknessDescriptionText;
    [SerializeField]
    private TextMeshProUGUI sicknessPotionIDText;

    private int id;

    public void Show(int id)
    {
        Initialize();
        base.Show();
        this.id = id;

        sicnessNameText.text = DataManager.Instance.SicknessDataDic[id].Name;
        sicknessDescriptionText.text = DataManager.Instance.SicknessDataDic[id].Description;

        int sicknessPotion = DataManager.Instance.SicknessDataDic[id].PotionId;
        sicknessPotionIDText.text = DataManager.Instance.ItemDataDic[sicknessPotion].Name;
    }
}
