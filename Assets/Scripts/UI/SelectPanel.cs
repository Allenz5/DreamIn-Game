using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using LitJson;

public class SelectPanel : MonoBehaviour
{
    public GameObject content;
    public GameObject item;
    private ScriptsJsonData sj;

    private void Start()
    {
        StartCoroutine(GetNameAndID());
    }
    public void CreateScriptItem(List<ScriptInfo> scripts)
    {
        List<GameObject> itemList = new List<GameObject>();
        //�� Content ������ _count ��item
        if (scripts.Count > 0)
        {
            int i = 0;
            item.SetActive(true); //��һ��itemʵ���Ѿ������б��һ��λ�ã�ֱ�Ӽ���
            itemList.Add(item);
            itemList[i].GetComponentInChildren<Text>().text = scripts[i].name;
            itemList[i].GetComponent<ScrollIndexCallback1>().gameID = scripts[i].id.ToString();
            i++;

            while (i < scripts.Count)
            {
                GameObject a = GameObject.Instantiate(item) as GameObject;
                a.transform.parent = content.transform; //����Ϊ Content ���Ӷ���
                itemList.Add(a);
                RectTransform t = itemList[i - 1].GetComponent<RectTransform>(); //��ȡǰһ�� item ��λ��    
                                                                                 //��ǰ item λ�÷�����ǰһ�� item �·�    
                a.GetComponent<RectTransform>().localScale = t.localScale;
                a.GetComponent<RectTransform>().localPosition =
                 new Vector3(t.localPosition.x, t.localPosition.y - t.rect.height-20, t.localPosition.z);


                a.GetComponentInChildren<Text>().text = scripts[i].name;
                a.GetComponent<ScrollIndexCallback1>().gameID = scripts[i].id.ToString();
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

    IEnumerator GetNameAndID()
    {
        string url = "https://api.dreamin.land/game_name/";
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ProtocolError || webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError(webRequest.error + "\n");
            }
            else
            {
                sj = JsonMapper.ToObject<ScriptsJsonData>(webRequest.downloadHandler.text);
                CreateScriptItem(sj.result.Info);
            }
        }
    }
}
