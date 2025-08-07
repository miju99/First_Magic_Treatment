using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 포션 슬롯들을 화면에 생성, 슬롯 클릭 시 팝업을 열기 위한 데이터 전달
/// </summary>
public class BookPopup : BasePopup
{
    private BookTab currentTab = BookTab.Potion; //현재 탭 설정

    [SerializeField] private GameObject potionSlotPrefab; //포션 슬롯 프리팹
    [SerializeField] private Transform potioncontentParent; //프리팹이 만들어질 부모 위치

    [SerializeField] private GameObject sicknessSlotPrefab;
    [SerializeField] private Transform sicknessContentParent;

    [SerializeField] private Button potionBtn; //포션 탭 버튼
    [SerializeField] private Button sicknessBtn; //질병 탭 버튼

    [SerializeField] private Button nextBtn;
    [SerializeField] private Button previousBtn;

    [SerializeField] private TextMeshProUGUI currentPageNumberText;
    [SerializeField] private TextMeshProUGUI MaxPageNumberText;

    private Dictionary<int, ItemData> itemDataTable = new();
    private Dictionary<int, SicknessData> sicknessDataTable = new();

    private int currentPage = 0; //현재 페이지
    private int potionPerPage = 2; //포션 페이지에서 보여줄 아이템 개수
    private int sicknessPerPage = 5;

    public Action<int> onSicknessSlotClicked;

    public List<ItemData> itemList = new List<ItemData>();
    public List<string> mixtureList = new List<string>();

    public Button CloseBtn => closeBtn;

    //private bool isPotionPageShown = false; // 포션 페이지에 이미 출력된 정보가 있는지 파악

    protected override void Start()
    {      
        base.Start();
        itemDataTable = DataManager.Instance.ItemDataDic;
        sicknessDataTable = DataManager.Instance.SicknessDataDic;
        CreatePotionSlot();
    }

    protected override void OnClickEventSetting()
    {
        base.OnClickEventSetting();

        potionBtn?.onClick.AddListener(() => OnTabClicked(BookTab.Potion));
        potionBtn?.onClick.AddListener(() => SoundManager.Instance.PlaySound(SoundType.SFX, SoundID.SFX_Click)); //버튼클릭사운드
        sicknessBtn?.onClick.AddListener(() => OnTabClicked(BookTab.Sickness));
        sicknessBtn?.onClick.AddListener(() => SoundManager.Instance.PlaySound(SoundType.SFX, SoundID.SFX_Click)); //버튼클릭사운드
        
        nextBtn?.onClick.AddListener(OnNextPageClicked);
        previousBtn?.onClick.AddListener(OnPreviousPageClicked);
    }

    public override void Show()
    {
        base.Show();
        MapController.SetSwipeConstraint(SwipeConstraint.None);
        onSicknessSlotClicked += OnSicknessSlotClickEvent;
        currentTab = BookTab.Potion;
        CreatePotionSlot();
    }
    public override void Close()
    {
        base.Close();
        MapController.SetSwipeConstraint(SwipeConstraint.Both);
        if(TutorialManager.Instance.IsTutorialPlaying)
        {
            TutorialManager.Instance.OnActionCompleted(ActionKey.CloseBookPopup, true);
        }
        onSicknessSlotClicked -= OnSicknessSlotClickEvent;
    }
    private void OnTabClicked(BookTab type)
    {
        if (currentTab == type) return;

        currentTab = type;
        currentPage = 0;
        ClearSlots();

        if (currentTab == BookTab.Potion)
        {
            CreatePotionSlot();
        }

        if (currentTab == BookTab.Sickness)
        {
            CreateSicknessSlot();
        }
    }
    public void ShowSelectedPage(int page)
    {
        if (currentTab != BookTab.Potion && currentTab != BookTab.Sickness) return;
        if (currentTab == BookTab.Potion)
        {
            int totalPotionCount = itemDataTable.Values.Count(i => i.Type == ItemType.Potion);
            var potionItems = itemDataTable.Values.Where(i => i.Type == ItemType.Potion).ToList();
            int maxPage = Mathf.CeilToInt(potionItems.Count / (float)potionPerPage) - 1;

            if (page <= maxPage)
            {
                currentPage = page;
                CreatePotionSlot();
                SoundManager.Instance.PlaySound(SoundType.SFX, SoundID.SFX_BookSlide);
            }
        }

        if (currentTab == BookTab.Sickness)
        {
            var sicknessItems = sicknessDataTable.Values.ToList();
            int maxPage = Mathf.CeilToInt(sicknessItems.Count / (float)sicknessPerPage) - 1;

            if (page <= maxPage)
            {
                currentPage = page;
                CreateSicknessSlot();
                SoundManager.Instance.PlaySound(SoundType.SFX, SoundID.SFX_BookSlide);
            }
        }
    }
    private void OnNextPageClicked()
    {
        Debug.Log("다음 페이지");

        if (currentTab != BookTab.Potion && currentTab != BookTab.Sickness) return;

        if (currentTab == BookTab.Potion)
        {
            int totalPotionCount = itemDataTable.Values.Count(i => i.Type == ItemType.Potion);
            var potionItems = itemDataTable.Values.Where(i => i.Type == ItemType.Potion).ToList();
            int maxPage = Mathf.CeilToInt(potionItems.Count / (float)potionPerPage) - 1;

            if (currentPage < maxPage)
            {
                currentPage++;
                CreatePotionSlot();
                SoundManager.Instance.PlaySound(SoundType.SFX, SoundID.SFX_BookSlide);
            }
        }

        if(currentTab == BookTab.Sickness)
        {
            var sicknessItems = sicknessDataTable.Values.ToList();
            int maxPage = Mathf.CeilToInt(sicknessItems.Count / (float)sicknessPerPage) - 1;

            if(currentPage < maxPage)
            {
                currentPage++;
                CreateSicknessSlot();
                SoundManager.Instance.PlaySound(SoundType.SFX, SoundID.SFX_BookSlide);
            }
        }
    }

    private void OnPreviousPageClicked()
    {
        Debug.Log("이전 페이지");

        if (currentTab != BookTab.Potion && currentTab != BookTab.Sickness) return;

        if (currentPage > 0)
        {
            currentPage--;
            if (currentTab == BookTab.Potion)
            {
                CreatePotionSlot();
                SoundManager.Instance.PlaySound(SoundType.SFX, SoundID.SFX_BookSlide);
            }
            else if (currentTab == BookTab.Sickness)
            {
                CreateSicknessSlot();
                SoundManager.Instance.PlaySound(SoundType.SFX, SoundID.SFX_BookSlide);
            }
        }
    }

    private void CreatePotionSlot() //슬롯을 생성하고 슬롯마다 아이템 데이터와 클릭 이벤트를 전달
    {
        ClearSlots(); //슬롯 제거

        var PotionItems = itemDataTable.Values.Where(x => x.Type == ItemType.Potion).OrderBy(x => x.Id).ToList();

        int startIndex = currentPage * potionPerPage;
        var pageItems = PotionItems.Skip(startIndex).Take(potionPerPage).ToList();

        Debug.Log($"[페이지 {currentPage + 1}] startIndex: {startIndex}, count: {pageItems.Count}");

        var unlockedPotionIds = Shop.Instance.Player.CollectedPotion;

        foreach (var item in pageItems)
        { 
            bool isUnlocked = unlockedPotionIds.Contains(item.Id);

            Debug.Log($"[슬롯 생성] PotionId: {item.Id}");

            var go = Instantiate(potionSlotPrefab, potioncontentParent.transform);
            go.GetComponent<PotionSlot>().Initialize(item, null, isUnlocked);
        }

        ShowPageNumber();
    }

    public void OnSicknessSlotClickEvent(int value) //슬롯이 클릭되면 슬롯에서 전달된 value(itemID)를 받는다.
    {
        if (UIManager.Instance.TryGetPopup<SicknessPopup>(UIPopupType.SicknessPopup, out var sicknessPopup))
        {
            sicknessPopup.Show(value);
        }
    }

    private void CreateSicknessSlot()
    {
        ClearSlots();

        var SicknessItem = sicknessDataTable.Values.ToList();
        int startIndex = currentPage * sicknessPerPage;
        var pageItems = SicknessItem.Skip(startIndex).Take(sicknessPerPage);

        var unlockedSicknessIds = Shop.Instance.Player.CollectedSickness;

        foreach (var sickness in pageItems)
        {
            bool isUnlocked = unlockedSicknessIds.Contains(sickness.Id);
            var go = Instantiate(sicknessSlotPrefab, sicknessContentParent.transform);
            go.GetComponent<SicknessSlot>().Initialize(sickness, onSicknessSlotClicked, isUnlocked);
        }

        ShowPageNumber();
    }

    private void ClearSlots()
    {
        foreach (Transform child in potioncontentParent)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in sicknessContentParent)
        {
            Destroy(child.gameObject);
        }
    }

    private void ShowPageNumber()
    {
        int maxPage = 0;

        if (currentTab == BookTab.Potion)
        {
            var potionItems = itemDataTable.Values.Where(i => i.Type == ItemType.Potion).ToList();
            maxPage = Mathf.CeilToInt(potionItems.Count / (float)potionPerPage);
        }
        else if (currentTab == BookTab.Sickness)
        {
            var sicknessItems = sicknessDataTable.Values.ToList();
            maxPage = Mathf.CeilToInt(sicknessItems.Count / (float)sicknessPerPage);
        }
        currentPageNumberText.text = (currentPage + 1).ToString();
        MaxPageNumberText.text = maxPage.ToString();
    }
}