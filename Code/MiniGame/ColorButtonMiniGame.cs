using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ColorButtonMiniGame : BaseMiniGame
{
    [SerializeField] private GameObject ecgImage;
    [SerializeField] private Button redButton;
    [SerializeField] private Button yellowButton;
    [SerializeField] private Button greenButton;
    [SerializeField] private Button blueButton;
    [SerializeField] private Sprite defaultEcgSprite;
    [SerializeField] private Sprite wrongEcgSprite;

    private Dictionary<Button, Color> originalButtonColors = new();
    private Dictionary<string, Button> colorButtons;

    private List<string> colorSequenceNames = new();
    private List<string> userInputNames = new();

    private Image ecgImageComponent;
    private float changeDelay = 0.5f;
    private bool inputEnabled = false;
    private bool gameEnded = false;

    private void Awake()
    {
        colorButtons = new Dictionary<string, Button>{{ "Red", redButton }, { "Yellow", yellowButton }, { "Green", greenButton }, { "Blue", blueButton }};

        foreach (var kvp in colorButtons)
        {
            var buttonImage = kvp.Value.GetComponent<Image>();
            if (buttonImage != null)
            {
                originalButtonColors[kvp.Value] = buttonImage.color;
            }
        }
    }

    public override void StartMiniGame()
    {
        base.StartMiniGame();

        timer = 12f;

        ecgImageComponent = ecgImage.GetComponent<Image>();

        colorSequenceNames = new List<string> { "Red", "Yellow", "Green", "Blue" }.OrderBy(x => Random.value).ToList();

        Debug.Log("색깔 순서:");
        foreach (var name in colorSequenceNames)
        {
            Debug.Log(name);
        }

        if (ecgImageComponent != null && defaultEcgSprite != null)
        {
            ecgImageComponent.sprite = defaultEcgSprite;
            ecgImageComponent.color = Color.white;
        }

        userInputNames.Clear();
        gameEnded = false;

        foreach (var kvp in colorButtons)
        {
            Button button = kvp.Value;
            string colorName = kvp.Key;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnColorButtonClicked(colorName));
            button.interactable = true;
        }
        StartCoroutine(WaitThenPlayColorSequence());
    }
      
    public override void UpdateMiniGame()
    {
        base.UpdateMiniGame();       
    }

    private IEnumerator PlayColorSequence()
    {
        inputEnabled = false;
        SetButtonsDark(true);

        if (ecgImageComponent != null)
        {
            ecgImageComponent.color = Color.white;
        }

        yield return new WaitForSeconds(1f);

        for (int i = 0; i < colorSequenceNames.Count; i++)
        {
            if (ecgImageComponent != null)
            {
                ecgImageComponent.color = GetColorFromName(colorSequenceNames[i]);
            }

            yield return new WaitForSeconds(changeDelay);
        }

        ecgImageComponent.color = Color.white;

        SetButtonsDark(false);
        inputEnabled = true;
    }

    private Color GetColorFromName(string name)
    {
        return name switch
        {
            "Red" => Color.red,
            "Yellow" => Color.yellow,
            "Green" => Color.green,
            "Blue" => Color.blue,
            _ => Color.white
        };
    }

    public void OnColorButtonClicked(string colorName)
    {
        if (gameEnded) return;
        if (!inputEnabled)
        {
            Debug.Log("입력 불가 상태");
            return;
        }

        if (userInputNames.Count >= colorSequenceNames.Count)
        {
            Debug.Log("입력 초과");
            return;
        }

        userInputNames.Add(colorName);

        if (colorButtons.TryGetValue(colorName, out var button))
        {
            button.interactable = false;
        }

        if (userInputNames.Count == colorSequenceNames.Count)
        {
            inputEnabled = false;
            gameEnded = true;
            bool isCorrect = true;

            for (int i = 0; i < colorSequenceNames.Count; i++)
            {
                if (userInputNames[i] != colorSequenceNames[i])
                {
                    isCorrect = false;
                    break;
                }
            }

            Score = isCorrect ? 100 : 0;

            if (isCorrect)
            {
                OnGameComplete?.Invoke(Score);
            }
            else
            {
                if (ecgImageComponent != null && wrongEcgSprite != null)
                {
                    ecgImageComponent.sprite = wrongEcgSprite;
                    ecgImageComponent.color = Color.black;
                }
                StartCoroutine(EndGameAfterDelay(1f));
            }
        }
    }

    private IEnumerator EndGameAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        OnGameComplete?.Invoke(Score);
    }

    private void SetButtonsDark(bool darken)
    {
        foreach (var kvp in originalButtonColors)
        {
            var button = kvp.Key;
            var image = button.GetComponent<Image>();

            if (image != null)
            {
                if (darken)
                {
                    image.color = kvp.Value * 0.5f;
                }
                else
                {
                    image.color = kvp.Value;
                }
            }
        }
    }

    private IEnumerator WaitThenPlayColorSequence()
    {
        inputEnabled = false;
        SetButtonsDark(true);

        if (ecgImageComponent != null)
        {
            ecgImageComponent.color = Color.white;
        }

        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(PlayColorSequence());
    }
}