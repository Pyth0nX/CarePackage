using System.Collections.Generic;
using UnityEngine;

public static class HelperMethods
{
    public static void ShuffleList(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    public static int GetRandomFromList(List<int> list)
    {
        return list[Random.Range(0, list.Count)];
    }
}
