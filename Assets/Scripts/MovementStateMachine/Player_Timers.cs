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

    // Storing
    public void StoreFloat(string variableName, float data)
    {
        globalDataFloat[variableName] = data;
    }
    public void StoreBool(string variableName, bool data)
    {
        globalDataBool[variableName] = data;
    }

    // Get
    public float GetFloat(string variableName)
    {
        return globalDataFloat[variableName];
    }

    public bool GetBool(string variableName)
    {
        return globalDataBool[variableName];
    }

    // Check
    public bool IsVariableStored(string variableName)
    {
        return globalDataFloat.ContainsKey(variableName) || globalDataBool.ContainsKey(variableName);
    }

    // Run Coroutines

}
