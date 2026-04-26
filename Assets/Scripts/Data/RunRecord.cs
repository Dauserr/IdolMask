using System;

[Serializable]
public class RunRecord
{
    public int   attemptNumber;
    public float timeOfDeath;

    public RunRecord(int attemptNumber, float timeOfDeath)
    {
        this.attemptNumber = attemptNumber;
        this.timeOfDeath   = timeOfDeath;
    }
}
