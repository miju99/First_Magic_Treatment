using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MaterialSwipeMiniGame : BaseMiniGame
{
    public enum Direction { Up, Down, Left, Right }

    private List<Direction> targetDirections = new List<Direction>();
    private int currentIndex = 0;

    private Vector2 swipeStart;
    private bool isSwiping = false;

    [SerializeField] public int minDirections = 4;
    [SerializeField] public int maxDirections = 8;

    [Header("UI")]
    [SerializeField] private GameObject directionIconPrefab;
    [SerializeField] private Transform directionContainer;
    [SerializeField] private Image heartImg;

    [Header("Direction Sprites")]
    public Sprite upSprite;
    public Sprite downSprite;
    public Sprite leftSprite;
    public Sprite rightSprite;

    [Header("Effect")]
    public MaterialTouchEffect successEffect; // 성공 이펙트

    private float pointsPerCorrect = 0f;

    public override void StartMiniGame()
    {
        base.StartMiniGame();
        currentIndex = 0;
        GenerateRandomDirections();

        int totalDirections = targetDirections.Count;
        pointsPerCorrect = (totalDirections > 0) ? 100f / totalDirections : 0f;
        Score = 0f;
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
                SetTimer(5f);
                break;
            case Difficulty.Hard:
                SetTimer(5f);
                isHardMode = true;
                heartImg.gameObject.SetActive(true);
                heartImg.sprite = DataManager.Instance.GetSprite(SpriteType.UI, "Heart_1");
                break;
        }
    }

    public override void UpdateMiniGame()
    {
        base.UpdateMiniGame();

        if (Input.GetMouseButtonDown(0))
        {
            swipeStart = Input.mousePosition;
            isSwiping = true;
        }
        else if (Input.GetMouseButtonUp(0) && isSwiping)
        {
            Vector2 swipeEnd = Input.mousePosition;
            HandleSwipe(swipeEnd - swipeStart);
            isSwiping = false;
        }
    }

    private void HandleSwipe(Vector2 swipeVector)
    {
        if (swipeVector.magnitude < 50f) return;

        Direction inputDir = GetSwipeDirection(swipeVector);

        if (IsComplete()) return;

        if (inputDir == targetDirections[currentIndex])
        {
            currentIndex++;
            Score += pointsPerCorrect;
            Score = Mathf.Min(Score, 100f);

            successEffect?.PlayEffect(); // 성공 이펙트 재생
            UpdateDirectionHighlight();

            if (IsComplete())
            {
                Debug.Log($"미니게임 성공! 최종 점수: {Score}");
                OnGameComplete?.Invoke(Score);
                End();
            }
        }
        else if (isHardMode)
        {
            heartImg.sprite = DataManager.Instance.GetSprite(SpriteType.UI, "Heart_0");
            OnGameComplete?.Invoke(Score);
        }
    }

    public override bool IsComplete()
    {
        return currentIndex >= targetDirections.Count;
    }

    private void GenerateRandomDirections()
    {
        targetDirections.Clear();
        int count = Random.Range(minDirections, maxDirections + 1);

        for (int i = 0; i < count; i++)
        {
            Direction dir = (Direction)Random.Range(0, 4);
            targetDirections.Add(dir);
        }

        UpdateDirectionUI();
    }

    private Direction GetSwipeDirection(Vector2 swipe)
    {
        if (Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y))
        {
            return swipe.x > 0 ? Direction.Right : Direction.Left;
        }
        else
        {
            return swipe.y > 0 ? Direction.Up : Direction.Down;
        }
    }

    private void UpdateDirectionUI()
    {
        foreach (Transform child in directionContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < targetDirections.Count; i++)
        {
            Direction dir = targetDirections[i];
            GameObject icon = Instantiate(directionIconPrefab, directionContainer);
            Image img = icon.GetComponent<Image>();
            img.sprite = GetSpriteForDirection(dir);

            img.color = (i == currentIndex) ? Color.yellow : Color.white;
        }
    }

    private void UpdateDirectionHighlight()
    {
        for (int i = 0; i < directionContainer.childCount; i++)
        {
            Image img = directionContainer.GetChild(i).GetComponent<Image>();
            img.color = (i == currentIndex) ? Color.yellow : Color.white;
        }
    }

    private Sprite GetSpriteForDirection(Direction dir)
    {
        switch (dir)
        {
            case Direction.Up: return upSprite;
            case Direction.Down: return downSprite;
            case Direction.Left: return leftSprite;
            case Direction.Right: return rightSprite;
            default: return null;
        }
    }
}
