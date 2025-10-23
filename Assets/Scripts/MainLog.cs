
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class MainLog
{   
    public static void LogError(string title,string message, string color)
    {
#if UNITY_EDITOR
        Debug.LogError($"<color={ReadColor.Red}>{"❌❌❌"+title.ToUpper() + ":"}</color>[<color={color}>{message}</color>]");

#endif
    }

    public static void LogWarning(string title, string message, string color)
    {
#if UNITY_EDITOR
        Debug.LogWarning($"<color={ReadColor.Yellow}>{title.ToUpper() + ":"}</color>[<color={color}>{message}</color>]");

#endif
    }

    public static void Log(string title, string message, string color)
    {
#if UNITY_EDITOR
        Debug.Log($"<color={ReadColor.Green}>{"✅✅✅"+title.ToUpper()+":"}</color>[<color={color}>{message}</color>]");
#endif
    }

    public static string ColorToHex(Color color)
    {
        return $"#{ColorUtility.ToHtmlStringRGB(color)}"; 
    }
}
