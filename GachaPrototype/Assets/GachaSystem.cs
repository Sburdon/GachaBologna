using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GachaSystem : MonoBehaviour
{
    public Button quickPullButton;
    public Button testYourSkillsButton;
    public Button quickPullX10Button;
    public Button testYourSkillsX10Button;

    private int totalPulls = 0;
    private Dictionary<string, int> unitCounts = new Dictionary<string, int> { { "mech", 0 }, { "idol", 0 }, { "cat", 0 } };
    private Dictionary<string, int> lastUnitCounts = new Dictionary<string, int> { { "mech", 0 }, { "idol", 0 }, { "cat", 0 } };
    private Dictionary<string, int> rarityCounts = new Dictionary<string, int> { { "Common", 0 }, { "Rare", 0 }, { "UltraRare", 0 } };
    private Dictionary<string, int> lastSpinCounts = new Dictionary<string, int> { { "Common", 0 }, { "Rare", 0 }, { "UltraRare", 0 } };

    // Odds
    private float oddsCommon = 0.90f;
    private float oddsRare = 0.08f;
    private float oddsUltraRare = 0.02f;

    // Adjusted odds after "Test Your Skills"
    private float[] successOdds = new float[] { 0.85f, 0.10f, 0.05f };
    private float[] failureOdds = new float[] { 0.95f, 0.04f, 0.01f };

    void Start()
    {
        quickPullButton.onClick.AddListener(() => QuickPull(false));
        testYourSkillsButton.onClick.AddListener(() => TestYourSkills(false));
        quickPullX10Button.onClick.AddListener(() => QuickPull(true));
        testYourSkillsX10Button.onClick.AddListener(() => TestYourSkills(true));
    }

    private void AdjustOdds()
    {
        // Ensure there were pulls last round to avoid division by zero and unnecessary adjustments
        int totalLastSpins = lastSpinCounts["Common"] + lastSpinCounts["Rare"] + lastSpinCounts["UltraRare"];
        if (totalLastSpins == 0) return;

        // Find the majority and least units pulled last spin
        string maxRarity = "Common", minRarity = "Common";
        int maxCount = -1, minCount = int.MaxValue;

        foreach (var rarity in lastSpinCounts)
        {
            if (rarity.Value > maxCount)
            {
                maxCount = rarity.Value;
                maxRarity = rarity.Key;
            }
            if (rarity.Value < minCount)
            {
                minCount = rarity.Value;
                minRarity = rarity.Key;
            }
        }

        // Avoid adjustments if all are equal or no pulls
        if (maxRarity == minRarity) return;

        // Adjusting odds, considering base odds to ensure they don't go out of plausible range
        float adjustAmount = 0.01f;  // Small adjustment to make the impact gradual
        if (maxRarity == "Common" && oddsCommon > 0.85f)  // Ensuring odds don't fall below a minimum threshold
            oddsCommon -= adjustAmount;
        else if (maxRarity == "Rare" && oddsRare > 0.05f)
            oddsRare -= adjustAmount;
        else if (maxRarity == "UltraRare" && oddsUltraRare > 0.01f)
            oddsUltraRare -= adjustAmount;

        if (minRarity == "Common" && oddsCommon < 0.95f)  // Ensuring odds don't exceed a maximum threshold
            oddsCommon += adjustAmount;
        else if (minRarity == "Rare" && oddsRare < 0.10f)
            oddsRare += adjustAmount;
        else if (minRarity == "UltraRare" && oddsUltraRare < 0.03f)
            oddsUltraRare += adjustAmount;
    }

    public void QuickPull(bool isTenPull)
    {
        ResetLastSpinCounts();
        totalPulls += isTenPull ? 10 : 1;
        AdjustOdds();
        for (int i = 0; i < (isTenPull ? 10 : 1); i++)
        {
            if (isTenPull && i == 9) // Guarantee a rare on the last pull of the ten pull
                PullGacha(oddsCommon, 1.0f - oddsCommon, 0);
            else
                PullGacha(oddsCommon, oddsRare, oddsUltraRare);
        }
        PrintCounts();
    }

    public void TestYourSkills(bool isTenPull)
    {
        ResetLastSpinCounts();
        totalPulls += isTenPull ? 10 : 1;
        AdjustOdds();
        float[] chosenOdds = Random.value > 0.5f ? successOdds : failureOdds;

        for (int i = 0; i < (isTenPull ? 10 : 1); i++)
        {
            if (isTenPull && i == 9) // Guarantee a rare on the last pull of the ten pull
                PullGacha(oddsCommon, 1.0f - oddsCommon, 0);
            else
                PullGacha(chosenOdds[0], chosenOdds[1], chosenOdds[2]);
        }
        PrintCounts();
    }

    private void ResetLastSpinCounts()
    {
        lastSpinCounts["Common"] = 0;
        lastSpinCounts["Rare"] = 0;
        lastSpinCounts["UltraRare"] = 0;
        lastUnitCounts["mech"] = 0;
        lastUnitCounts["idol"] = 0;
        lastUnitCounts["cat"] = 0;
    }

    private void PullGacha(float commonOdds, float rareOdds, float ultraRareOdds)
    {
        float pull = Random.value;
        string type = Random.value > 0.66f ? "mech" : (Random.value > 0.33f ? "idol" : "cat"); // Randomly decide type
        string rarity;

        if (pull < ultraRareOdds)
        {
            rarity = "UltraRare";
        }
        else if (pull < ultraRareOdds + rareOdds)
        {
            rarity = "Rare";
        }
        else
        {
            rarity = "Common";
        }

        unitCounts[type]++;
        rarityCounts[rarity]++;
        lastSpinCounts[rarity]++;
        lastUnitCounts[type]++;
    }

    private void PrintCounts()
    {
        // Logging counts for each rarity from the last spin and the total
        Debug.Log($"Last Spin - Common: {lastSpinCounts["Common"]}, Rare: {lastSpinCounts["Rare"]}, UltraRare: {lastSpinCounts["UltraRare"]}; " +
                  $"Total - Common: {rarityCounts["Common"]}, Rare: {rarityCounts["Rare"]}, UltraRare: {rarityCounts["UltraRare"]}");

        // Logging counts for each unit type from the last spin and the total
        Debug.Log($"Last Spin - Mech: {lastUnitCounts["mech"]}, Idol: {lastUnitCounts["idol"]}, Cat: {lastUnitCounts["cat"]}; " +
                  $"Total - Mech: {unitCounts["mech"]}, Idol: {unitCounts["idol"]}, Cat: {unitCounts["cat"]}");
    }













}
