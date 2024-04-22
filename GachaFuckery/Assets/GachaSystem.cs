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
    private Dictionary<string, int> rarityCounts = new Dictionary<string, int> { { "Common", 0 }, { "Rare", 0 }, { "UltraRare", 0 } };

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
        // Example adjustment logic: increase the chance for underrepresented unit types
        // Calculate the total number of units pulled
        int totalUnitsPulled = unitCounts["mech"] + unitCounts["idol"] + unitCounts["cat"];
        if (totalUnitsPulled == 0) return;  // Avoid division by zero in a fresh game

        float mechRatio = (float)unitCounts["mech"] / totalUnitsPulled;
        float idolRatio = (float)unitCounts["idol"] / totalUnitsPulled;
        float catRatio = (float)unitCounts["cat"] / totalUnitsPulled;

        // Adjust odds based on the ratio
        // Assume you want each type to ideally be about 33%, you can adjust odds accordingly
        if (mechRatio > 0.33f) AdjustSpecificOdds("mech", -0.05f); // Reduce mech odds if there are too many
        if (idolRatio > 0.33f) AdjustSpecificOdds("idol", -0.05f); // Reduce idol odds if there are too many
        if (catRatio > 0.33f) AdjustSpecificOdds("cat", -0.05f);   // Reduce cat odds if there are too many

        // Similarly, increase odds if too low
        if (mechRatio < 0.33f) AdjustSpecificOdds("mech", 0.05f);
        if (idolRatio < 0.33f) AdjustSpecificOdds("idol", 0.05f);
        if (catRatio < 0.33f) AdjustSpecificOdds("cat", 0.05f);
    }

    private void AdjustSpecificOdds(string unitType, float adjustment)
    {
        // Adjusting specific odds for each type
        if (unitType == "mech")
        {
            oddsCommon += adjustment;
        }
        else if (unitType == "idol")
        {
            oddsRare += adjustment;
        }
        else if (unitType == "cat")
        {
            oddsUltraRare += adjustment;
        }
    }


    public void QuickPull(bool isTenPull)
    {
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

    private void PullGacha(float commonOdds, float rareOdds, float ultraRareOdds)
    {
        float pull = Random.value;
        string type = Random.value > 0.66f ? "mech" : (Random.value > 0.33f ? "idol" : "cat"); // Randomly decide type
        string rarity;

        if (pull < ultraRareOdds)
        {
            rarity = "UltraRare";
            Debug.Log("Pulled an Ultra Rare " + type + "!");
        }
        else if (pull < ultraRareOdds + rareOdds)
        {
            rarity = "Rare";
            Debug.Log("Pulled a Rare " + type + "!");
        }
        else
        {
            rarity = "Common";
            Debug.Log("Pulled a Common " + type + "!");
        }

        unitCounts[type]++;
        rarityCounts[rarity]++;
    }

    private void PrintCounts()
    {
        Debug.Log("Unit Counts:");
        foreach (var pair in unitCounts)
            Debug.Log(pair.Key + ": " + pair.Value);

        Debug.Log("Rarity Counts:");
        foreach (var pair in rarityCounts)
            Debug.Log(pair.Key + ": " + pair.Value);
    }
}
