using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Stores varaibles over multiple states
// Runs Coroutines for states, even after they exit
public class Player_Timers : MonoBehaviour
{

    // Dictonary 
    private Dictionary<string, float> globalDataFloat = new Dictionary<string, float>();
    private Dictionary<string, bool> globalDataBool = new Dictionary<string, bool>();
    private Dictionary<string, Vector3> globalDataVector3 = new Dictionary<string, Vector3>();

    // Storing
    public void StoreFloat(string variableName, float data)
    {
        globalDataFloat[variableName] = data;
    }
    public void StoreBool(string variableName, bool data)
    {
        globalDataBool[variableName] = data;
    }
    public void StoreVector3(string variableName, Vector3 data)
    {
        globalDataVector3[variableName] = data;
    }

    // Get
    public float GetFloat(string variableName, float defaultValue)
    {
        try
        {
            return globalDataFloat[variableName];
        }
        catch (KeyNotFoundException)
        {
            StoreFloat(variableName, defaultValue);
            return defaultValue;
        }
    }

    public bool GetBool(string variableName, bool defaultValue)
    {
        try
        {
            return globalDataBool[variableName];
        }
        catch (KeyNotFoundException)
        {
            StoreBool(variableName, defaultValue);
            return defaultValue;
        }
    }
    
    public Vector3 GetVector3(string variableName, Vector3 defaultValue)
    {
        try
        {
            return globalDataVector3[variableName];
        }
        catch (KeyNotFoundException)
        {
            StoreVector3(variableName, defaultValue);
            return defaultValue;
        }
    }

    // Check
    public bool IsVariableStored(string variableName)
    {
        return globalDataFloat.ContainsKey(variableName) || globalDataBool.ContainsKey(variableName) || globalDataVector3.ContainsKey(variableName);
    }

}
