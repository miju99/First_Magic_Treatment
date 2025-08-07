using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchPulseMiniGame : BaseMiniGame, IPointerDownHandler
{
    [SerializeField] private RectTransform imageBox;
    [SerializeField] private RectTransform pointImage;
    [SerializeField] private TextMeshProUGUI count;

    private bool isMiniGameEnded = false;
    private int clickCount = 0;
    private bool istouched = false;

    public override void StartMiniGame()
    {
        base.StartMiniGame();

        isMiniGameEnded = false;
        clickCount = 0;
        count.text = $"{clickCount} / 5";

        StartCoroutine(MovePulsePointRoutine());
    }

    public override void UpdateMiniGame()
    {
        base.UpdateMiniGame();
    }

    private void MoveToRandomPosition()
    {
        Vector2 boxSize = imageBox.rect.size;
        Vector2 pointSize = pointImage.rect.size;
        Vector2 randomPos = new Vector2(Random.Range((-boxSize.x / 2f) + (pointSize.x / 2f), (boxSize.x / 2f) - (pointSize.x / 2f)), Random.Range((-boxSize.y / 2f) + pointSize.y, (boxSize.y / 2f) - pointSize.y));

        pointImage.anchoredPosition = randomPos;
    }

    private IEnumerator MovePulsePointRoutine()
    {
        while (!isMiniGameEnded)
        {
            if (!istouched)
            {
                MoveToRandomPosition();
            }
            else
            {
                istouched = false;
            }
            yield return new WaitForSeconds(0.7f);
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        GameObject clickedObj = eventData.pointerCurrentRaycast.gameObject;

        if (clickedObj != null && clickedObj == pointImage.gameObject)
        {
            clickCount++;
            count.text = $"{clickCount} / 5";

            istouched = true;
            MoveToRandomPosition();

            if (clickCount >= 5)
            {
                isMiniGameEnded = true;
                Score = 100;
                OnGameComplete.Invoke(Score);
            }
        }
    }

    /*public void OnPointerClick(PointerEventData eventData)
    {
        GameObject clickedObj = eventData.pointerCurrentRaycast.gameObject;

        if (clickedObj == pointImage.gameObject)
        {
            clickCount++;
            count.text = $"{clickCount} / 5";

            istouched = true;
            MoveToRandomPosition();

            if (clickCount >= 5)
            {
                Score = 100;
                OnGameComplete.Invoke(Score);
            }
        }
    }*/
}
