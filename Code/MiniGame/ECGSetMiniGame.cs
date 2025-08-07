using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ECGSetMiniGame : BaseMiniGame, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform imageBox;
    [SerializeField] private RectTransform unCorrectImage;
    [SerializeField] private RectTransform CorrectImage;
    [SerializeField] private RectTransform unImageBox;
    [SerializeField] private Button upButton;
    [SerializeField] private Button downButton;

    private float moveAmount = 10f;
    private bool isHoldingUp = false;
    private bool isHoldingDown = false;

    private GameObject pressedObject;
    private bool isCompleted = false;

    public void SetHoldingUp(bool value) => isHoldingUp = value;
    public void SetHoldingDown(bool value) => isHoldingDown = value;

    public override void StartMiniGame()
    {
        base.StartMiniGame();
        isCompleted = false;

        MoveToRandomPosition();

        if (isCompleted == false)
        {
            isHoldingUp = false;
            isHoldingDown = false;
            pressedObject = null;
        }

        upButton.transform.rotation = Quaternion.Euler(0, 0, 180f);
    }

    public override void UpdateMiniGame()
    {
        base.UpdateMiniGame();

        if (isCompleted) return;

        if (isHoldingUp)
        {
            MoveImageY(moveAmount * Time.deltaTime * 10f);
        }

        if (isHoldingDown)
        {
            MoveImageY(-moveAmount * Time.deltaTime * 10f);
        }

        ClickedCheck();
    }

    private void MoveToRandomPosition()
    {
        Vector2 currentPos = unCorrectImage.anchoredPosition;
        float randomY = Random.Range((-imageBox.rect.height + unImageBox.rect.height) / 2f, (imageBox.rect.height - unImageBox.rect.height) / 2f);

        unCorrectImage.anchoredPosition = new Vector2(currentPos.x, randomY);
    }

    private void MoveImageY(float deltaY)
    {
        Vector2 pos = unCorrectImage.anchoredPosition;
        pos.y += deltaY;

        float halfBoxHeight = imageBox.rect.height / 2f;
        float halfImageHeight = unCorrectImage.rect.height / 2f;
        float halfUnBoxHeight = unImageBox.rect.height / 2f;

        float minY = -halfBoxHeight + halfUnBoxHeight;
        float maxY = halfBoxHeight - halfUnBoxHeight;

        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        unCorrectImage.anchoredPosition = pos;
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        pressedObject = eventData.pointerPress;

        if (pressedObject == upButton.gameObject)
        {
            isHoldingUp = true;
            MoveImageY(moveAmount);
        }
        else if (pressedObject == downButton.gameObject)
        {
            isHoldingDown = true;
            MoveImageY(-moveAmount);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isHoldingUp = false;
        isHoldingDown = false;
    }

   private void ClickedCheck()
    {
        float unCorrectY = unCorrectImage.anchoredPosition.y;
        float CorrectY = CorrectImage.anchoredPosition.y;

        if (Mathf.Abs(CorrectY - unCorrectY) <= 2f)
        {
            isCompleted = true;
            Score = 100;
            OnGameComplete.Invoke(Score);
        }
    }
}