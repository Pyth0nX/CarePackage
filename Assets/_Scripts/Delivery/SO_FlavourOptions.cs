using UnityEngine;

[CreateAssetMenu(fileName = "SO_FlavourOptions", menuName = "CarePackage/Flavour Options")]
public class SO_FlavourOptions : ScriptableObject
{
    public System.Collections.Generic.List<string> flavourOptions;

    public string GetRandomFlavour()
    {
        if (flavourOptions.Count == 0) return " someone";
        return flavourOptions[Random.Range(0, flavourOptions.Count)];
    }
}
