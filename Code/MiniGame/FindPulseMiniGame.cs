using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FindPulseMiniGame : BaseMiniGame, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform pulsePointBox;
    [SerializeField] private RectTransform pulsePoint;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private TextMeshProUGUI textFind;
    [SerializeField] private TextMeshProUGUI textSecond;
    [SerializeField] private GameObject particlePrefab;
    [SerializeField] private Image pulsePointImage;

    private GameObject currentParticle;
    private float touchTime = 0f;
    private float requiredHoldTime = 2f;
    private bool isTouchingPulse = false;

    private Color originalColor;
    private Color darkenedColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    private ParticleSystem ps;

    private void Awake()
    {
        originalColor = pulsePointImage.color;
    }

    public override void StartMiniGame()
    {
        base.StartMiniGame();

        MoveToRandomPosition();

        if (currentParticle != null)
        {
            currentParticle.SetActive(false);
        }

        text.enabled = true;
        textFind.enabled = false;
        textSecond.enabled = false;

        isTouchingPulse = false;
        touchTime = 0f;

        pulsePointImage.color = originalColor;

        if (currentParticle == null)
        {
            currentParticle = Instantiate(particlePrefab, pulsePoint.parent);
        }
        ps = currentParticle.GetComponent<ParticleSystem>();
        currentParticle.SetActive(false);
    }

    public override void UpdateMiniGame()
    {
        base.UpdateMiniGame();

        if (isTouchingPulse)
        {
            touchTime += Time.deltaTime;
            Score += 50 * Time.deltaTime;

            textSecond.text = $"{touchTime:F1}초 : {requiredHoldTime}초";

            if (touchTime >= requiredHoldTime)
            {
                isTouchingPulse = false;

                if (Score > 100)
                {
                    Score = 100;
                }

                if (Score < 100)
                {
                    Score = 0;
                }

                if (currentParticle != null)
                {
                    currentParticle.SetActive(false);
                }

                OnGameComplete?.Invoke(Score);
            }
        }

        if (currentParticle != null)
        {
            Vector2 localTouchPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(pulsePoint.parent as RectTransform, Input.mousePosition, Camera.main, out localTouchPos);

            float visualHintRadius = 500f;
            float distance = Vector2.Distance(pulsePoint.anchoredPosition, localTouchPos);
            float t = Mathf.Clamp01(1f - (distance / visualHintRadius));
            Color lerpedColor = Color.Lerp(Color.white, Color.red, t);

            if (ps != null)
            {
                var main = ps.main;
                main.startColor = lerpedColor;
            }
        }
    }

    private void MoveToRandomPosition()
    {
        Vector2 boxSize = pulsePointBox.rect.size;
        Vector2 randomPos = new Vector2(Random.Range(-boxSize.x / 2f, boxSize.x / 2f), Random.Range(-boxSize.y / 2f, boxSize.y / 2f));

        pulsePoint.anchoredPosition = randomPos;
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        CheckPulse(eventData);

        Vector2 localTouchPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(pulsePoint.parent as RectTransform, eventData.position, eventData.pressEventCamera, out localTouchPos);

        currentParticle.SetActive(true);
        currentParticle.transform.localPosition = localTouchPos;
        pulsePointImage.color = darkenedColor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isTouchingPulse = false;

        if (currentParticle != null)
        {
            currentParticle.SetActive(false);
        }
        pulsePointImage.color = originalColor;      
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        CheckPulse(eventData);
    }

    private void CheckPulse(PointerEventData eventData)
    {
        Vector2 localTouchPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(pulsePoint.parent as RectTransform, eventData.position, eventData.pressEventCamera, out localTouchPos);

        float detectRadius = 150f; //오차범위
        float distance = Vector2.Distance(pulsePoint.anchoredPosition, localTouchPos);
                
        if (distance <= detectRadius)
        {
            if (!isTouchingPulse)
            {
                text.enabled = false;
                textFind.enabled = true;
                textSecond.enabled = true;
            }
            isTouchingPulse = true;
        }
        else
        {
            isTouchingPulse = false;
        }
    }
}