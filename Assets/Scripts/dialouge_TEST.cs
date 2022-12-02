using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

[System.Serializable]
public class Bubbles
{
    public int bubbleNum;
    public string bubbleText;
}

public class dialouge_TEST : MonoBehaviour
{
    public TextAsset dialougeFile;
    public GameObject choiceButton;
    public Transform buttonPos;

    public float textSpeed = 0.1f;
    float defaultTextSpeed;
    public string textName = "????";
    public string sentence;
    public List<Bubbles> bubbles;
    public int sentenceLinePlace = 1;

    public TMP_Text dialogTextBox;
    public TMP_Text nameTextBox;

    bool isItalic = false;
    bool isBolded = false;
    bool isUnderlined = false;
    bool isStrikeThrough = false;
    bool isSentenceFilledIn = false;
    public bool isChoiceAnswered = true;


    // Start is called before the first frame update
    void Start()
    {
        string[] dialougeText = dialougeFile.ToString().Split('\n');
        int numOfBubbles = 0;
        for (int sentenceNum = 0; sentenceNum < dialougeText.Length; sentenceNum++)
        {
            if (dialougeText[sentenceNum].Length >= 9 && dialougeText[sentenceNum].Substring(0, 9) == "<\\BUBBLE>") 
            {
                Bubbles newBubble = new Bubbles{};
                newBubble.bubbleNum = sentenceNum;
                // -1 removes an unseen newline
                newBubble.bubbleText = dialougeText[sentenceNum].Substring(0, dialougeText[sentenceNum].Length-1);
                bubbles.Add(newBubble);

                // finds the starting bubble of the text
                if (newBubble.bubbleText == "<\\BUBBLE>START")
                {
                    // +1 to get the next sentence after the bubble's name
                    sentenceLinePlace = sentenceNum + 1;
                }
                    numOfBubbles++;
            }
        }

        
        defaultTextSpeed = textSpeed;
        // will always be 1 at start (to avoid printing the first bubble's name)
        PrepareNextSentence(sentenceLinePlace); 
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && isSentenceFilledIn && isChoiceAnswered) 
        { 
            // advances dialouge
            PrepareNextSentence(sentenceLinePlace+1); 
        }
        else if (Input.GetKeyDown(KeyCode.F12) && !isSentenceFilledIn) 
        { 
            textSpeed = 0; 
        }
    }


    IEnumerator PrintSentence(string sentence)
    {
        // prevents the player from advancing to the next sentence
        isSentenceFilledIn = false;

        // skips the sentence if it has "//" at the beginning
        if (sentence.Substring(0, 2) == "//")
        {
            // skip the line with "//" at the beginning
            sentence = "";
            // advances dialouge
            PrepareNextSentence(sentenceLinePlace+1);
        }
        // when the sentence is not skipped
        else
        {
            for (int charCount = 0; sentence.ToCharArray().Length > charCount; charCount++)
            {
                // skips the '\' character
                // the character directly after '\' will be displayed in the textbox 
                if (sentence[charCount] == '\\')
                {
                    // skip the '\' character from displaying in the textbox
                    charCount++;
                    // prints the character after the '\' to the textbox
                    dialogTextBox.text += sentence[charCount];
                }
                else if (sentence[charCount] == '{' || sentence[charCount] == '}')
                {
                    // skip the '{' or '}' character from displaying in the textbox
                    charCount++;
                    // prints the character after the '{' or '}' to the textbox
                    dialogTextBox.text += sentence[charCount];
                }

                // starts/stops italicizing text
                else if (sentence[charCount] == '*')
                {
                    // starts italicizing text
                    if (!isItalic) { isItalic = true; dialogTextBox.text += "<i>"; }
                    // stops italicizing text
                    else { isItalic = false; dialogTextBox.text += "</i>"; }
                }

                // starts/stops bolding text
                else if (sentence[charCount] == '@')
                {
                    // starts bolding text
                    if (!isBolded) { isBolded = true; dialogTextBox.text += "<b>"; }
                    // stops bolding text
                    else { isBolded = false; dialogTextBox.text += "</b>"; }
                }

                // starts/stops underlining text
                else if (sentence[charCount] == '_')
                {
                    // starts underlining text
                    if (!isUnderlined) { isUnderlined = true; dialogTextBox.text += "<u>"; }
                    // stops underlining text
                    else { isUnderlined = false; dialogTextBox.text += "</u>"; }
                }

                // starts/stops striking through text
                else if (sentence[charCount] == '~')
                {
                    // starts striking through text
                    if (!isStrikeThrough) { isStrikeThrough = true; dialogTextBox.text += "<s>"; }
                    // stops striking through text
                    else { isStrikeThrough = false; dialogTextBox.text += "</s>"; }
                }

                // changes the speed of the text
                else if ((sentence.Length - charCount) >= 7 && sentence.Substring(charCount, 7) == "<speed=")
                {
                    // skip the '<speed=' part of the sentence from displaying in the textbox
                    charCount += 7;

                    // sets the new text speed
                    int substringLength = FindNextChar(sentence, '>', charCount);
                    textSpeed = float.Parse(sentence.Substring(charCount, substringLength));

                    // skip the number and '>' part of the sentence
                    charCount += substringLength;
                }

                // pauses the text for an amount of time
                else if ((sentence.Length - charCount) >= 7 && sentence.Substring(charCount, 7) == "<pause=")
                {
                    // skip the '<pause=' part of the sentence from displaying in the textbox
                    charCount += 7;

                    // pauses the text for an amount of time
                    int substringLength = FindNextChar(sentence, '>', charCount);
                    yield return new WaitForSeconds(float.Parse(sentence.Substring(charCount, substringLength)) - textSpeed);

                    // skip the number and '>' part of the sentence
                    charCount += substringLength;
                }

                // sets a new name for nameTextBox 
                else if ((sentence.Length - charCount) >= 6 && sentence.Substring(charCount, 6) == "<name=")
                {
                    // skip the '<name=' part of the sentence from displaying in the textbox
                    charCount += 6;

                    // sets the new name in nameTextBox
                    int substringLength = FindNextChar(sentence, '>', charCount);
                    nameTextBox.text = sentence.Substring(charCount, substringLength);

                    // skip the name and '>' part of the sentence
                    charCount += substringLength;
                }

                else if ((sentence.Length - charCount) >= 8 && sentence.Substring(charCount, 8) == "<choice>")
                {
                    isChoiceAnswered = false;
                    // skip the '<choice>' part of the sentence from displaying in the textbox
                    charCount += 8;
                    // checks if there is a bubble right below the current line
                    checkNextTextLine(sentenceLinePlace+1);

                }

                // sets the sentence to the next bubble
                else if ((sentence.Length - charCount) >= 2 && sentence.Substring(charCount, 2) == "[[" && isChoiceAnswered)
                {
                    // skip the '[[' part of the sentence from being included in bubbleName
                    charCount += 2;

                    /*// finds the length and text of the next bubble's name
                    int substringLength = FindNextChar(sentence, ']', charCount);
                    */

                    int choiceNameLength = FindNextChar(sentence, '|', charCount);
                    // finds the length and text of the next bubble's name
                    int bubbleSubstringLength = FindNextChar(sentence, ']', 2 + choiceNameLength);
                    string bubbleName = sentence.Substring(charCount + choiceNameLength + 1, bubbleSubstringLength - 1);
                    Debug.Log("bubbleName: " + bubbleName);

                    // checks each bubble stored in the bubbles list for a matching bubble name
                    sentenceLinePlace = CheckForMatchingBubbleName(bubbleName);
                    PrepareNextSentence(sentenceLinePlace);



                    yield break;
                }
                // prints the character to the textbox
                else
                {
                    // prints the character to the textbox
                    dialogTextBox.text += sentence[charCount];
                    // wait an amount of time 
                    yield return new WaitForSeconds(textSpeed);
                }

                
            }
            // allows the player to advance to the next sentence
            isSentenceFilledIn = true;
        } 
    }
    public void checkNextTextLine(int linePlace)
    {
        string[] sentences = dialougeFile.ToString().Split('\n');

        int totalChoices = 0;
        // finds how many choices there are
        for (int choiceNum = 0; sentences[linePlace + choiceNum].Substring(0, 2) == "[[" && 
            (linePlace + choiceNum) < dialougeFile.ToString().Split('\n').Length; choiceNum++)
        {
            totalChoices++;
        }

        for (int choiceNum = 0; choiceNum < totalChoices; choiceNum++)
        {
            int choiceNameLength = FindNextChar(sentences[linePlace + choiceNum], '|', 2);
            Debug.Log(sentences[linePlace + choiceNum].Substring(2, choiceNameLength));
            // finds the length and text of the next bubble's name
            int bubbleSubstringLength = FindNextChar(sentences[linePlace + choiceNum], ']', 2 + choiceNameLength);
            Debug.Log("bubble Substring Length: " + bubbleSubstringLength);
            

            GameObject button = Instantiate(choiceButton, buttonPos);
            //set button choice's text to be substringLength's length
            button.GetComponentInChildren<TMP_Text>().text = sentences[linePlace + choiceNum].Substring(2, choiceNameLength);

            DialougeButtonData DBD = button.GetComponent<DialougeButtonData>();
            DBD.nextBubbleName = sentences[linePlace + choiceNum].Substring(2+choiceNameLength+1, bubbleSubstringLength-1);
            DBD.nextBubblePosition = CheckForMatchingBubbleName(DBD.nextBubbleName);

            RectTransform buttonRect = button.GetComponent<RectTransform>();
            button.transform.localPosition = (transform.up * (float)((buttonRect.sizeDelta.y * totalChoices) / 2) - (transform.up * buttonRect.sizeDelta.y * (float)choiceNum));
            Debug.Log("total Choices: " + totalChoices);
            Debug.Log("choice Num: " + choiceNum);
        }
        Debug.Log(totalChoices + " option(s) detected!");
        if (totalChoices <= 1)
        {
            DestroyChoiceButtons(buttonPos.childCount);
        }
    }
    // deletes all choice options
    public void DestroyChoiceButtons(int buttonCount)
    {
        for (int buttonNum = 0; buttonNum < buttonCount; buttonNum++)
        {
            Destroy(buttonPos.GetChild(buttonNum).gameObject);
        }
    }
    public int CheckForMatchingBubbleName(string bubbleName)
    {
        // prevents sentenceLinePlace from being written over and a failsafe if no matching bubble names exist
        int newLinePlace = sentenceLinePlace;

        // checks each bubble stored in the bubbles list for a matching bubble name
        for (int bubbleNum = 0; bubbleNum < bubbles.Count; bubbleNum++)
        {
            // if both strings are the same 
            if ((bubbleName == bubbles[bubbleNum].bubbleText))
            {
                // +1 gets the next sentence after the bubble's name
                newLinePlace = bubbles[bubbleNum].bubbleNum + 1;
                break;
            }
        }
        return newLinePlace;
    }

    // creates the new sentence to be used 
    public string CreateNewTextLine(int linePlace)
    {
        return dialougeFile.ToString().Split('\n')[linePlace];
    }

    // prepares for the next sentence
    public void PrepareNextSentence(int setToLinePlace)
    {
        // resets sentence properties
        ResetSentence();

        // sets the sentence to the first one in the queue and removes it from the queue
        sentenceLinePlace = setToLinePlace;

        // starts printing the sentence to the textbox
        StartCoroutine(PrintSentence(CreateNewTextLine(setToLinePlace)));
    }

    public void ResetSentence()
    {
        // prevents the player from advancing to the next sentence
        isSentenceFilledIn = false;
        // destroys any and all choice buttons
        DestroyChoiceButtons(buttonPos.childCount);

        // empties the textbox
        dialogTextBox.text = "";

        isItalic = false;
        isBolded = false;
        isUnderlined = false;
        isStrikeThrough = false;
        textSpeed = defaultTextSpeed;
    }

    // finds the next char in a sentence and returns the number of characters it checked
    // used to find the end of the angle bracket (>) and square bracket ()
    public int FindNextChar(string sentence, char charToFind, int charCount)
    {
        int i = 0;
        for (; sentence[charCount+i] != charToFind && i < sentence.Length; i++)
        {
            // ...do nothing
        }
        return i;
    }
}
