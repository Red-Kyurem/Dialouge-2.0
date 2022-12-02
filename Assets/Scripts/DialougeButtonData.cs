using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialougeButtonData : MonoBehaviour
{
    public string nextBubbleName;
    public int nextBubblePosition;
    dialouge_TEST DM; // dialouge manager

    void Start()
    {
        DM = GameObject.FindGameObjectWithTag("DialougeManager").GetComponent<dialouge_TEST>();
    }
    public void SetDialougePosition(bool uselessBool)
    {
        DM.PrepareNextSentence(nextBubblePosition);
        DM.isChoiceAnswered = true;
    }

}
