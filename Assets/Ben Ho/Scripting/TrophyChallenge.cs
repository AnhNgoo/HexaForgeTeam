using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class TrophyChallenge
{
    public string title;
    public string description;

    public bool isCompleted;
    public bool isClaimed;

    public GameObject darkOverlay;
    public Button claimButton;
    public GameObject claimedIcon;
}