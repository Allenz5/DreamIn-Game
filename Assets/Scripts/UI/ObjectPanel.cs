using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectPanel : MonoBehaviour
{
    public GameObject adjustablePanel;
    public GameObject scrollPanel;

    public TMP_Text text1;
    public TMP_Text text2;

    public Button sharedButton1;
    public Button sharedButton2;

    public int longTextLength;
    private bool isLongText;
    private void Start()
    {

    }
    private void Update()
    {
        if(isLongText)
        {
            adjustablePanel.gameObject.SetActive(false);
            scrollPanel.gameObject.SetActive(true);
        }
        else
        {
            adjustablePanel.gameObject.SetActive(true);
            scrollPanel.gameObject.SetActive(false);
        }
    }
    public void SetText(string str)
    {
        text1.text = str;
        text2.text = str;

        if (str.Length > longTextLength)
            isLongText = true;
        else
            isLongText = false;
        

    }

    public string GetText()
    {
        return text1.text;
    }
    public void SetSharedButton(bool interact, string shared)
    {
        sharedButton1.interactable = interact;
        sharedButton2.interactable = interact;

        sharedButton1.GetComponentInChildren<TMP_Text>().text = shared;
        sharedButton2.GetComponentInChildren<TMP_Text>().text = shared;
    }
}
