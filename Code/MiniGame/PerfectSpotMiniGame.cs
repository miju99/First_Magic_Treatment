using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PerfectSpotMiniGame : BaseMiniGame, IPointerClickHandler
{
    [SerializeField] private Transform bar;
    [SerializeField] private GameObject ball;
    [SerializeField] private Transform area;
    [SerializeField] private float speed = 1f;
    [SerializeField] private Image material;
    

    private Vector2 startPoint;
    private Vector2 endPoint;

    protected override bool IsTimer => false;

    public override void StartMiniGame()
    {
        if (bar == null) return;
        RectTransform rt = bar.GetComponent<RectTransform>();
        if (rt == null) return;
        if (ball == null) return;

        base.StartMiniGame();
        if (UIManager.Instance.TryGetHud<AlchemyJar>(HudType.AlchemyJar, out var hud))
        {
            material.sprite = hud.GetRandomMaterialSprite();
        }
        float width = rt.rect.width * bar.lossyScale.x;
        startPoint = new Vector2(bar.position.x - width / 2, bar.position.y);
        endPoint = new Vector2(bar.position.x + width / 2, bar.position.y);

        float randomAreaX = Random.Range(startPoint.x, endPoint.x);
        Vector2 areaNewPosition = new Vector2(randomAreaX, area.position.y);
        area.position = areaNewPosition;
    }
    protected override void SetDifficulty()
    {
        isHardMode = false;
        switch (difficulty)
        {
            case Difficulty.Easy:
                speed = 1f;
                break;
            case Difficulty.Normal:
                speed = 2f;
                break;
            case Difficulty.Hard:
                speed = 3.5f;
                isHardMode = true;
                break;
        }
    }
    public override void UpdateMiniGame()
    {
        base.UpdateMiniGame();

        float t = Mathf.PingPong(Time.time * speed, 1f);
        ball.transform.position = Vector2.Lerp(startPoint, endPoint, t);
    }

    public void OnPointerClick(PointerEventData eventdata)
    {
        //if (!IsPointerInArea(eventdata)) return;

        float ballX = ball.transform.position.x;
        float areaCenterX = area.position.x;

        float distance = Mathf.Abs(ballX - areaCenterX);

        float maxDistance = (bar.GetComponent<RectTransform>().rect.width * bar.lossyScale.x) / 2f;
        float normalized = Mathf.Clamp01(1f - (distance / maxDistance));
        float scoreGained = Mathf.RoundToInt(normalized * 100);
        Score = scoreGained;

        Debug.Log($"<color=yellow>score =  + {Score} </color>");

        OnGameComplete?.Invoke(Score);
    }

    /*
    private bool IsPointerInArea(PointerEventData eventData)
    {
        RectTransform rect = material.GetComponent<RectTransform>();
        if (rect == null) return true;
        return RectTransformUtility.RectangleContainsScreenPoint(rect, eventData.position, eventData.enterEventCamera);
    }
    */
}