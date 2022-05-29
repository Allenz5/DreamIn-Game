using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class VotePanel : MonoBehaviour
{
    public GameObject content;
    public GameObject item;

    public void CreatePlayerItem(GameObject[] players)
    {
        List<GameObject> itemList = new List<GameObject>();
        //�� Content ������ _count ��item
        if (players.Length > 0)
        {
            int i = 0;
            item.SetActive(true); //��һ��itemʵ���Ѿ������б��һ��λ�ã�ֱ�Ӽ���
            itemList.Add(item);
            itemList[i].GetComponentInChildren<TMP_Text>().text = players[i].GetComponent<playerScript>().playerName;
            i++;

            while (i < players.Length)
            {
                GameObject a = GameObject.Instantiate(item) as GameObject;
                a.transform.parent = content.transform; //����Ϊ Content ���Ӷ���
                itemList.Add(a);
                RectTransform t = itemList[i - 1].GetComponent<RectTransform>(); //��ȡǰһ�� item ��λ��    
                                                                                 //��ǰ item λ�÷�����ǰһ�� item �·�    
                a.GetComponent<RectTransform>().localPosition =
                 new Vector3(t.localPosition.x, t.localPosition.y - t.rect.height-20, t.localPosition.z);
                a.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

                a.GetComponentInChildren<TMP_Text>().text = players[i].GetComponent<playerScript>().playerName;
                i++;
            }
            //���ݵ�ǰ item �������� Content �߶� 
            content.GetComponent<RectTransform>().sizeDelta =
              new Vector2(content.GetComponent<RectTransform>().sizeDelta.x, itemList.Count * (item.GetComponent<RectTransform>().rect.height+20));
        }
        else
        {
            item.SetActive(false);
        }
    }

    public void VoteThisPlayer(Button btn)
    {
        ColorBlock cb = btn.colors;
        cb.highlightedColor = btn.colors.highlightedColor;
        cb.pressedColor = btn.colors.pressedColor;
        cb.disabledColor = btn.colors.disabledColor;
        cb.selectedColor = btn.colors.highlightedColor;
        btn.colors = cb;
    }
}
