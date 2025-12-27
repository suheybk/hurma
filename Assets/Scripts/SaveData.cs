using System;

[Serializable]
public class SaveData
{
    public int CurrentDay = 1;
    public float TimeOfDay = 0.28f;
    public int DatesHarvested = 0;
    
    // Tree Data
    public TreeData TreeState;

    // Narrative Data
    // We can add visited quotes or stages here later if needed
}

[Serializable]
public class TreeData
{
    public string StageName; // Storing enum as string for better JSON readability
    public float AgeInDays;
    public float WaterLevel;
    public int FruitCount;
    public bool IsPlanted;

    public TreeData()
    {
        StageName = "Seed";
        AgeInDays = 0f;
        WaterLevel = 50f;
        FruitCount = 0;
        IsPlanted = false;
    }
}
