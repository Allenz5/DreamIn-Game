using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

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
            itemList[i].GetComponentInChildren<Text>().text = players[i].GetComponent<playerScript>().playerName;
            i++;

            while (i < players.Length)
            {
                GameObject a = GameObject.Instantiate(item) as GameObject;
                a.transform.parent = content.transform; //����Ϊ Content ���Ӷ���
                itemList.Add(a);
                RectTransform t = itemList[i - 1].GetComponent<RectTransform>(); //��ȡǰһ�� item ��λ��    
                                                                                 //��ǰ item λ�÷�����ǰһ�� item �·�    
                a.GetComponent<RectTransform>().localPosition =
                 new Vector3(t.localPosition.x, t.localPosition.y - t.rect.height, t.localPosition.z);
                a.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

                a.GetComponentInChildren<Text>().text = players[i].GetComponent<playerScript>().playerName;
                i++;
            }
            //���ݵ�ǰ item �������� Content �߶� 
            content.GetComponent<RectTransform>().sizeDelta =
              new Vector2(content.GetComponent<RectTransform>().sizeDelta.x, itemList.Count * item.GetComponent<RectTransform>().rect.height);
        }
        else
        {
            item.SetActive(false);
        }
    }
}
