namespace CarePackage.Interaction
{
    [System.Serializable]
    public enum InteractionType
    {
        Active, // interaction happening when pressing the interaction key
        Passive, // interaction happening when overlapping two objects
        Clicked, // interaction happening when clicked in UI or on the object with mouse pointer
    }
}