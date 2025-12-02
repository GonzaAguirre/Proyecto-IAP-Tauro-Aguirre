using System;
using System.Collections.Generic;

[Serializable]
public class PestData
{
    public string id;           
    public string name;         
    public string imageURL;     
    public string description;  
    public string danger;       
    public string solution;     
    public string type;         // "Normal", "Extraño", "Especial"
}

[Serializable]
public class CallData
{
    public string id;               
    public string callerName;       
    public string callerImageURL;   
    public string message;          
    public string correctPestID;    
    public int day;                 
    public string callType;         // "Consejo", "Confirmacion", "Extra"
    public string audio;            
}

[Serializable]
public class GameDataCollection
{
    public string themeName;
    public List<PestData> pests;
    public List<CallData> calls;
}