using UnityEngine;
using System.Collections;

public class K2FilePath : MonoBehaviour {

    public static string GameData
    {
        get
        {
            return Application.dataPath + "/Resources/SavedData/";
        }
    }

    public static string GetGameFilePath( string filename )
    {
        return GameData + filename;
    }
}
