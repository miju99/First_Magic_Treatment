using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ImpuritiesRemoveMiniGame : BaseMiniGame, IPointerClickHandler
{
    [SerializeField] private GameObject protectionPrefab;
    [SerializeField] private GameObject germPrefab;
    [SerializeField] private RectTransform boxRectTransform;
    [SerializeField] private Image heartImg;

    private int protectionPoolSize = 6;
    private int germPoolSize = 6;

    private List<GameObject> protectionPool = new List<GameObject>();
    private List<GameObject> germPool = new List<GameObject>();

    private bool isMiniGameEnded = false;

    private HashSet<GameObject> clickedObjects = new HashSet<GameObject>();
    private bool isBlinking = false;

    private List<Vector3> spawnPositions = new List<Vector3>();

    public override void StartMiniGame()
    {
        base.StartMiniGame();

        isMiniGameEnded = false;
        clickedObjects.Clear();

        if (protectionPool.Count == 0)
        {
            InitPool(protectionPrefab, protectionPoolSize, protectionPool, true);
        }

        if (germPool.Count == 0)
        {
            InitPool(germPrefab, germPoolSize, germPool, false);
        }

        GenerateSpawnPositions();
        Shuffle(spawnPositions);

        int index = 0;

        foreach (var obj in protectionPool)
        {
            obj.GetComponent<RectTransform>().anchoredPosition = spawnPositions[index++];
        }

        foreach (var obj in germPool)
        {
            obj.GetComponent<RectTransform>().anchoredPosition = spawnPositions[index++];
        }

        StartCoroutine(ToggleItemsCoroutine());
    }
    protected override void SetDifficulty()
    {
        isHardMode = false;
        heartImg.gameObject.SetActive(false);

        switch (difficulty)
        {
            case Difficulty.Easy:
                SetTimer(10f);
                break;
            case Difficulty.Normal:
                SetTimer(7f);
                break;
            case Difficulty.Hard:
                SetTimer(5f);
                heartImg.gameObject.SetActive(true);
                heartImg.sprite = DataManager.Instance.GetSprite(SpriteType.UI, "Heart_1");
                isHardMode = true;
                break;
        }
    }
    public override void UpdateMiniGame()
    {
        base.UpdateMiniGame();         

        if (!isMiniGameEnded)
        {
            if (Score >= 100)
            {
                isMiniGameEnded = true;
                OnGameComplete?.Invoke(Score);
            }
            else if (timer <= 0)
            {
                isMiniGameEnded = true;
                DeactivateAllObjects();
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        GameObject clickedObj = eventData.pointerCurrentRaycast.gameObject;

        if (clickedObj == null || !clickedObj.activeSelf) return;

        if (!clickedObj.transform.IsChildOf(boxRectTransform)) return;

        if (!clickedObj.name.StartsWith("Protection") && !clickedObj.name.StartsWith("Germ")) return;

        if (clickedObjects.Contains(clickedObj)) return;

        if (clickedObj.name.StartsWith("Protection"))
        {
            if (isHardMode)
            {
                heartImg.sprite = DataManager.Instance.GetSprite(SpriteType.UI, "Heart_0");
                OnGameComplete?.Invoke(Score);
            }
            Score -= 10;
        }
        else if (clickedObj.name.StartsWith("Germ"))
        {
            Score += Mathf.Round(100 / 6f);
        }

        clickedObj.SetActive(false);
        clickedObjects.Add(clickedObj);

        Score = Mathf.Clamp(Score, 0, 100);
    }

    private void InitPool(GameObject prefab, int count, List<GameObject> pool, bool isCorrect)
    {
        if (prefab == null)
        {
            Debug.LogError($"{ (isCorrect ? "protection" : "germ")}prefab이 할당되지 않았습니다.");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            GameObject item = Instantiate(prefab, boxRectTransform.gameObject.transform);
            item.name = isCorrect ? $"Protection_{i}" : $"Germ_{i}";

            item.SetActive(false);
            pool.Add(item);
        }
    }

    private IEnumerator ToggleItemsCoroutine()
    {
        while (!isMiniGameEnded)
        {
            ToggleAllItems();

            float blinkTimer = 1f;
            while (blinkTimer > 0f)
            {
                if (isMiniGameEnded)
                {
                    yield break;
                }
                blinkTimer -= Time.deltaTime;
                yield return null;
            }

            UnToggleAllItems();

            blinkTimer = 0.5f;
            while (blinkTimer > 0f)
            {
                if (isMiniGameEnded)
                {
                    yield break;
                }
                blinkTimer -= Time.deltaTime;
                yield return null;
            }
        }
    }

    private void ToggleAllItems()
    {
        foreach (var obj in protectionPool)
        {
            if (clickedObjects.Contains(obj))
            {
                continue;
            }
            obj.SetActive(!isBlinking);
        }

        foreach (var obj in germPool)
        {
            if (clickedObjects.Contains(obj))
            {
                continue;
            }
            obj.SetActive(!isBlinking);
        }
    }

    private void UnToggleAllItems()
    {

        foreach (var obj in protectionPool)
        {
            if (clickedObjects.Contains(obj))
            {
                continue;
            }
            obj.SetActive(isBlinking);
        }

        foreach (var obj in germPool)
        {
            if (clickedObjects.Contains(obj))
            {
                continue;
            }
            obj.SetActive(isBlinking);
        }
    }

    private void GenerateSpawnPositions()
    {
        spawnPositions.Clear();

        Vector2 size = boxRectTransform.rect.size;

        int totalObjects = protectionPoolSize + germPoolSize;
        int rows = 4;
        int cols = 3;

        float spacingX = size.x / (cols + 1);
        float spacingY = size.y / (rows + 1);

        for (int row = 1; row <= rows; row++)
        {
            for (int col = 1; col <= cols; col++)
            {
                float x = -size.x / 2 + spacingX * col;
                float y = -size.y / 2 + spacingY * row;

                Vector3 localPos = new Vector3(x, y, 0);
                spawnPositions.Add(localPos);
            }
        }
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }

    private void DeactivateAllObjects()
    {
        foreach (var obj in protectionPool)
        {
            obj.SetActive(false);
        }
        foreach (var obj in germPool)
        {
            obj.SetActive(false);
        }
    }
}