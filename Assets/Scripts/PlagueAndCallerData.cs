using System;
using UnityEngine;

[Serializable]
public class PlagueData
{
     public string id;
     public string name;
     public string description;
     public string danger;
     public string solution;
     public Sprite image;
     public PlagueType type;
}

[Serializable]
public class CallerData 
{
     public string callId;
     public string callerName;
     public string transcription;
     public Sprite callerImage;
     public string correctPlagueId;
     public int dayNumber;
     public CallType callType; 
}

public enum PlagueType { Normal, Strange, Special }

public enum CallType { Advice, Emergency }