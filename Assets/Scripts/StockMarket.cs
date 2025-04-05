using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class StockMarket : MonoBehaviour
{
    public GameObject candlePrefab;
    public RectTransform candleParent;
    public Text balanceText;
    public Button riseButton, fallButton;

    private float balance;
    private List<float> prices = new List<float> { 100 };
    private float candleWidth = 15f;
    private float minPrice, maxPrice;
    private float padding = 0.1f;
    private float candleDelay = 0.2f;
    private bool isGenerating = false;

    void Start()
    {
        balance = PlayerPrefs.GetFloat("Bitcoins", 1000f); // Завантажити баланс або 1000, якщо немає
        UpdateBalance(); // Оновити UI

        riseButton.onClick.AddListener(() => MakeBet(true));
        fallButton.onClick.AddListener(() => MakeBet(false));
        StartCoroutine(GenerateCandlesGradually());
    }

    IEnumerator GenerateCandlesGradually()
    {
        isGenerating = true;
        riseButton.interactable = false;
        fallButton.interactable = false;

        foreach (Transform child in candleParent)
        {
            Destroy(child.gameObject);
        }

        List<float> newPrices = new List<float> { prices[prices.Count - 1] };
        for (int i = 0; i < 10; i++)
        {
            float open = newPrices[newPrices.Count - 1];
            float close = open + Random.Range(-5f, 5f);
            newPrices.Add(close);
        }

        minPrice = Mathf.Min(newPrices.ToArray()) - 2f;
        maxPrice = Mathf.Max(newPrices.ToArray()) + 2f;

        float parentWidth = candleParent.rect.width;
        float parentHeight = candleParent.rect.height;
        float candleSpacing = parentWidth / 10f;

        for (int i = 0; i < 10; i++)
        {
            float open = i == 0 ? prices[prices.Count - 1] : newPrices[i];
            float close = newPrices[i + 1];
            float high = Mathf.Max(open, close) + Random.Range(0, 2f);
            float low = Mathf.Min(open, close) - Random.Range(0, 2f);

            GameObject newCandle = Instantiate(candlePrefab, candleParent);
            RectTransform candleRect = newCandle.GetComponent<RectTransform>();

            candleRect.anchorMin = new Vector2(0, 0);
            candleRect.anchorMax = new Vector2(0, 0);
            candleRect.pivot = new Vector2(0.5f, 0);

            float xPos = i * candleSpacing + candleSpacing / 2;
            candleRect.anchoredPosition = new Vector2(xPos, 0);

            SetCandle(newCandle, open, close, high, low, parentHeight);

            if (i == 9) prices.Add(close);

            yield return new WaitForSeconds(candleDelay);
        }

        isGenerating = false;
        riseButton.interactable = true;
        fallButton.interactable = true;
    }

    void SetCandle(GameObject candle, float open, float close, float high, float low, float parentHeight)
    {
        RectTransform body = candle.transform.Find("Body").GetComponent<RectTransform>();
        RectTransform wick = candle.transform.Find("Wick").GetComponent<RectTransform>();
        Image bodyImage = body.GetComponent<Image>();
        Image wickImage = wick.GetComponent<Image>();

        body.anchorMin = new Vector2(0.5f, 0);
        body.anchorMax = new Vector2(0.5f, 0);
        body.pivot = new Vector2(0.5f, 0);

        wick.anchorMin = new Vector2(0.5f, 0);
        wick.anchorMax = new Vector2(0.5f, 0);
        wick.pivot = new Vector2(0.5f, 0);

        float usableHeight = parentHeight * (1f - 2f * padding);
        float heightOffset = parentHeight * padding;

        float normalizedOpen = NormalizePrice(open);
        float normalizedClose = NormalizePrice(close);
        float normalizedHigh = NormalizePrice(high);
        float normalizedLow = NormalizePrice(low);

        float bodyHeight = Mathf.Abs(normalizedOpen - normalizedClose) * usableHeight;
        float wickHeight = (normalizedHigh - normalizedLow) * usableHeight;

        body.sizeDelta = new Vector2(candleWidth * 0.7f, bodyHeight);
        wick.sizeDelta = new Vector2(candleWidth * 0.2f, wickHeight);
        float bodyY = heightOffset + (normalizedOpen + normalizedClose) / 2 * usableHeight;
        float wickY = heightOffset + (normalizedHigh + normalizedLow) / 2 * usableHeight;

        body.anchoredPosition = new Vector2(0, bodyY);
        wick.anchoredPosition = new Vector2(0, wickY);

        bodyImage.color = (close >= open) ? Color.green : Color.red;
        wickImage.color = (close >= open) ? Color.green : Color.red;
    }

    float NormalizePrice(float price)
    {
        if (maxPrice == minPrice) return 0.5f;
        return (price - minPrice) / (maxPrice - minPrice);
    }

    void MakeBet(bool betOnRise)
    {
        if (!isGenerating)
        {
            StartCoroutine(ProcessBet(betOnRise));
        }
    }

    IEnumerator ProcessBet(bool betOnRise)
    {
        float lastPrice = prices[prices.Count - 1];
        yield return StartCoroutine(GenerateCandlesGradually());
        float newPrice = prices[prices.Count - 1];

        bool win = (betOnRise && newPrice > lastPrice) || (!betOnRise && newPrice < lastPrice);
        balance += win ? 100 : -100;
        UpdateBalance();
    }

    void UpdateBalance()
    {
        balanceText.text = "Баланс: " + balance.ToString("F2") + " BTC";
        PlayerPrefs.SetFloat("Bitcoins", balance);
        PlayerPrefs.Save();
    }
}
