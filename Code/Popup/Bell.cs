using AYellowpaper.SerializedCollections;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Bell : BaseUI
{
    [SerializeField] private Image emotionImage;
    [SerializeField] private Animator animator;

    private CustomerController customerController;
    private Coroutine coroutine;

    [SerializeField] private SerializedDictionary<CustomerEmotionType, Sprite> emotionSpriteDic;

    public override void Initialize()
    {
        base.Initialize();
        customerController = WorkManager.Instance.CustomerController;
        customerController.onCustomerEnter += Belling;
        customerController.onCustomerLeft += StopBelling;
        
    }

    private void OnDestroy()
    {
        customerController.onCustomerEnter -= Belling;
        customerController.onCustomerLeft -= StopBelling;
    }

    private void Belling()
    {
        gameObject.SetActive(true);
        coroutine = StartCoroutine(BellingTimer());
        SetEmoji(customerController.Customer.Emotion);

        customerController.Customer.onEmotionChanged -= SetEmoji;
        customerController.Customer.onEmotionChanged += SetEmoji;
    }

    private void StopBelling()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
        gameObject.SetActive(false);
    }

    private IEnumerator BellingTimer()
    {
        yield return new WaitForSeconds(5f);

        StopBelling();
    }

    private void SetEmoji(CustomerEmotionType curEmo)
    {
        if (curEmo == CustomerEmotionType.None) return;

        emotionImage.sprite = emotionSpriteDic[curEmo];

        if (!gameObject.activeSelf)
        {
            Belling();
        }
    }
}
