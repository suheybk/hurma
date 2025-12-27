using UnityEngine;

public static class GameFinder
{
    public static T FindComponent<T>(string name) where T : Component
    {
        GameObject go = GameObject.Find(name);
        if (go != null) return go.GetComponent<T>();
        return null; // or Object.FindFirstObjectByType<T>() if unique
    }
}
