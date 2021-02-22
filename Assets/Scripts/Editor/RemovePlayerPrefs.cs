using UnityEngine;
using UnityEditor;

public class RemovePlayerPrefs : ScriptableObject
{
    [MenuItem("PlayerPrefs/Remove all PlayerPrefs")]
    static void deleteAllExample()
    {
        PlayerPrefs.DeleteAll();
    }
}