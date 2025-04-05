using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Clicker : MonoBehaviour
{
    public TMP_Text balanceText;  // Виводимо баланс
    public Button clickButton;    // Кнопка для основного кліку
    public Button doubleClickUpgradeButton; // Кнопка для апгрейду Double Click
    public TMP_Text doubleClickCostText; // Текст для вартості апгрейду
    public Button passiveIncomeUpgradeButton; // Кнопка для апгрейду пасивного доходу
    public TMP_Text passiveIncomeCostText; // Текст для вартості апгрейду пасивного доходу

    [SerializeField] double balance = 0;
    [SerializeField] double clickValue = 1;
    [SerializeField] double passiveIncome = 0;  // Пасивний дохід
    [SerializeField] double doubleClickUpgradeCost = 100; // Вартість апгрейду Double Click
    private bool doubleClickActive = false; // Чи активовано апгрейд для Double Click
    [SerializeField] double passiveIncomeUpgradeCost = 200; // Вартість апгрейду пасивного доходу

    private float passiveInterval = 1f; // Інтервал для пасивного доходу (кожну секунду)

    private void Start()
    {
        // Завантажити дані
        balance = PlayerPrefs.GetFloat("Balance", 0);
        passiveIncome = PlayerPrefs.GetFloat("PassiveIncome", 0);
        clickValue = PlayerPrefs.GetFloat("ClickValue", 1);
        doubleClickUpgradeCost = PlayerPrefs.GetFloat("DoubleClickUpgradeCost", 100);
        doubleClickActive = PlayerPrefs.GetInt("DoubleClickActive", 0) == 1;
        passiveIncomeUpgradeCost = PlayerPrefs.GetFloat("PassiveIncomeUpgradeCost", 200);

        // Додаємо слухачів на кнопки
        clickButton.onClick.AddListener(OnClickButton); // Основний клік
        doubleClickUpgradeButton.onClick.AddListener(OnDoubleClickUpgrade); // Кнопка апгрейду Double Click
        passiveIncomeUpgradeButton.onClick.AddListener(OnPassiveIncomeUpgrade); // Кнопка апгрейду пасивного доходу

        UpdateUI();
        InvokeRepeating("AddPassiveIncome", passiveInterval, passiveInterval); // Додаємо пасивний дохід кожну секунду
    }

    private void OnClickButton()
    {
        // Кожен клік додає clickValue до балансу
        double bonus = doubleClickActive ? clickValue * 2 : clickValue;
        balance += bonus;
        SaveData();
        UpdateUI();
    }

    public void OnDoubleClickUpgrade()
    {
        // Перевірка на можливість купити апгрейд для Double Click
        if (balance >= doubleClickUpgradeCost)
        {
            balance -= doubleClickUpgradeCost;
            doubleClickActive = true; // Активуємо подвоєння кліків
            doubleClickUpgradeCost *= 2; // Збільшуємо вартість наступного апгрейду
            SaveData();
            UpdateUI();
        }
    }

    public void OnPassiveIncomeUpgrade()
    {
        // Перевірка на можливість купити апгрейд для пасивного доходу
        if (balance >= passiveIncomeUpgradeCost)
        {
            balance -= passiveIncomeUpgradeCost;
            passiveIncome += 1; // Збільшуємо пасивний дохід на 1
            passiveIncomeUpgradeCost *= 1.5; // Збільшуємо вартість наступного апгрейду
            SaveData();
            UpdateUI();
        }
    }

    private void AddPassiveIncome()
    {
        // Додаємо пасивний дохід кожну секунду
        balance += passiveIncome;
        SaveData();
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Оновлюємо текст балансу та вартості апгрейдів
        balanceText.text = $"Баланс: {balance:F2} $ \nПасивний дохід: {passiveIncome:F2} $/сек";
        doubleClickCostText.text = $"Подвійний клік: {doubleClickUpgradeCost:F2} $"; // Оновлюємо вартість апгрейду
        passiveIncomeCostText.text = $"Пасивний дохід: {passiveIncomeUpgradeCost:F2} $"; // Оновлюємо вартість апгрейду пасивного доходу
    }

    private void SaveData()
    {
        // Зберігаємо всі дані, включаючи пасивний дохід і апгрейди
        PlayerPrefs.SetFloat("Balance", (float)balance);
        PlayerPrefs.SetFloat("PassiveIncome", (float)passiveIncome);
        PlayerPrefs.SetFloat("ClickValue", (float)clickValue);
        PlayerPrefs.SetFloat("DoubleClickUpgradeCost", (float)doubleClickUpgradeCost);
        PlayerPrefs.SetInt("DoubleClickActive", doubleClickActive ? 1 : 0);
        PlayerPrefs.SetFloat("PassiveIncomeUpgradeCost", (float)passiveIncomeUpgradeCost);
        PlayerPrefs.Save();
    }
}
