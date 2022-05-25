using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Networking;
using System.IO;
using System.Text.RegularExpressions;
using LitJson;

public class GameManager : MonoBehaviourPunCallbacks
{ 
    public GameObject readyButton;
    public GameObject startButton;
    public GameObject scriptScroll;
    public GameObject canvas;
    public GameObject objects;
    public GameObject colliders;
    public GameObject objectPrefab;
    public GameObject colliderPrefab;
    public GameObject votePanel;

    public Text FinalText;//����������
    public Text TimerText;//��ʾʱ��

    [SerializeField]
    private int countTime=0;//����ʱ����

    private PhotonView GM_PhotonView;
    
    private GameData gameData;
    private bool isDownloadCompelete=false;

    private int ColliderSize = 32;

    public void Start()
    {
        GM_PhotonView = GetComponent<PhotonView>();

    }

#region ��ť����¼�
    public void ReadyButton()
    {
        GameObject player = PhotonNetwork.Instantiate("Player", canvas.transform.position, Quaternion.identity, 0);
        readyButton.SetActive(false);
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
            scriptScroll.SetActive(true);
        }
    }
    public void StartButton()
    {
        if (gameData == null)
        {
            Debug.Log("ûѡ��籾");
            return;
        }
        if(isDownloadCompelete==false)
        {
            Debug.Log("û���������");
            return;
        }
        startButton.SetActive(false);

        //TODO: ��������
        List<GameCharacter> characters =new List<GameCharacter>(gameData.result.info.character);
        GameObject[] playerObj = GameObject.FindGameObjectsWithTag("Player");
        for(int i=0;i<playerObj.Length;i++)
        {
            int index = Random.Range(0, characters.Count);
            characters.RemoveAt(index);
            playerObj[i].GetComponent<playerScript>().SetPlayerInfo(gameData.result.info.character, index);
        }
        //TODO: ��ʼ��ʱ
        StartCountTime(countTime);
    }

#endregion

    public void UpdateScene()
    {
        //���ü�ʱ
        countTime = gameData.result.info.length;
        //��ʼ����ͼ
        {
            GameObject map = Instantiate(objectPrefab, new Vector2(0, 0), Quaternion.identity, canvas.transform);

            map.transform.SetParent(canvas.transform);
            map.transform.localScale = new Vector3(1, 1, 1);

            //���õ�ͼ��λ��
            float w = gameData.result.info.Map[0].mapTexture.width;
            float h = gameData.result.info.Map[0].mapTexture.height;
            map.GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);
            map.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

            //���õ�ͼ��UI�㼶�����²�
            map.transform.SetSiblingIndex(0);

            //����sprite
            map.GetComponent<Image>().sprite = Sprite.Create(gameData.result.info.Map[0].mapTexture, new Rect(0, 0, w, h), new Vector2(0, 0));
        }

        //��ʼ��Object
        {
            //Ŀǰmap��һ�����飬������ʱ������ֻ��ȡmap[0]
            for (int i = 0; i < gameData.result.info.Map[0].Map_Object.Count; i++)
            {
                GameObject obj = Instantiate(objectPrefab, new Vector2(0, 0), Quaternion.identity, objects.transform);

                obj.transform.SetParent(canvas.transform);
                obj.transform.localScale = new Vector3(1, 1, 1);

                float w = gameData.result.info.Map[0].Map_Object[i].objTexture.width;
                float h = gameData.result.info.Map[0].Map_Object[i].objTexture.height;
                obj.GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);
                obj.GetComponent<Image>().sprite = Sprite.Create(gameData.result.info.Map[0].Map_Object[i].objTexture, new Rect(0, 0, w, h), new Vector2(0, 0));
                obj.GetComponent<Object>().SetInfoText(gameData.result.info.Map[0].Map_Object[i].message);
                obj.transform.position = gameData.result.info.Map[0].Map_Object[i].GetPosition();
            }
        }

        //��ʼ����ײ��
        {
            float w = gameData.result.info.Map[0].mapTexture.width / 2;
            float h = gameData.result.info.Map[0].mapTexture.height / 2;
            string[] rows = gameData.result.info.Map[0].collide_map.Split(';');
            for (int i = 0; i < rows.Length; i++)
            {
                string[] cols = rows[i].Split(',');
                for (int j = 0; j < cols.Length - 1; j++)
                {
                    GameObject obj = Instantiate(colliderPrefab, new Vector2(0, 0), Quaternion.identity, colliders.transform);
                    obj.transform.localPosition = new Vector3(-w + int.Parse(cols[j]) * ColliderSize + ColliderSize / 2, h - i * ColliderSize - ColliderSize / 2, 0);
                }
            }
        }
        //TODO: ���ý�β
        FinalText.text = gameData.result.info.end;
    }

    #region ��Ϸ��������
    public void DownLoadGameData(string ID)
    {
       GM_PhotonView.RPC("RPCDownloadGameData",RpcTarget.All,ID);
    }

    [PunRPC]
    void RPCDownloadGameData(string ID)
    {
        StartCoroutine(GetGameData(ID));
    }
    IEnumerator GetGameData(string ID)
    {
        string url = "http://52.71.182.98/q_game/?id=";
        url += ID;

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ProtocolError || webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError(webRequest.error + "\n" + webRequest.downloadHandler.text);
            }
            else
            {
#if UNITY_EDITOR
                //����һ�ݸ������ݵ�����
                string savePath = "Assets/Scripts/TempData.json";
                File.WriteAllText(savePath, Regex.Unescape(webRequest.downloadHandler.text));
#endif

                gameData = JsonMapper.ToObject<GameData>(webRequest.downloadHandler.text);

                //������������Ƿ��㹻�������Ļ����ܿ�����Ϸ    ����������ΪС�ڵ���
                int playerCount = GameObject.FindGameObjectsWithTag("Player").Length;
                if (playerCount >= gameData.result.info.character.Count)
                {
                    //�����������ͼ����
                    for (int i = 0; i < gameData.result.info.Map.Count; i++)
                    {
                        string addr = gameData.result.info.Map[i].background;
                        StartCoroutine(GetMapTexture(addr, i));//background???��������Ҫ�޸�

                        //��ͼ�е���Ʒ��ͼ
                        for (int j = 0; j < gameData.result.info.Map[i].Map_Object.Count; j++)
                        {
                            string objAddr = gameData.result.info.Map[i].Map_Object[j].image_link;
                            StartCoroutine(GetObjectTexture(objAddr, i, j));
                        }
                        //TODO��������ͼ����

                    }
                    //��������Ƿ����
                    StartCoroutine(WaitForDownloadCompelete());
                    scriptScroll.gameObject.SetActive(false);
                }
                else
                {
                    Debug.Log("����������������ѡ��籾!\n Ҫ��������"+ gameData.result.info.character.Count);
                    scriptScroll.gameObject.SetActive(true);
                }
            }
        }

    }
    /// <summary>
    /// ��ȡ��ͼ��ͼ
    /// </summary>
    /// <param name="addr"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    IEnumerator GetMapTexture(string addr, int i)
    {
        string imageLink = "https://raw.githubusercontent.com/hanxuan5/DreamIn-Assets/master/";
        imageLink += addr;
        imageLink = imageLink.Replace(" ", "%20");

        UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageLink);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error + "imageLink: " + imageLink);
        }
        else
        {
            gameData.result.info.Map[i].mapTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
        }
    }

    /// <summary>
    /// ��ȡobject����ͼ
    /// </summary>
    /// <param name="addr"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    IEnumerator GetObjectTexture(string addr, int i, int j)
    {
        string imageLink = "https://raw.githubusercontent.com/hanxuan5/DreamIn-Assets/master/";
        imageLink += addr;
        imageLink = imageLink.Replace(" ", "%20");

        UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageLink);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error + "imageLink: " + imageLink);
        }
        else
        {
            gameData.result.info.Map[i].Map_Object[j].objTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
        }
    }
    /// <summary>
    /// �ȴ�����ͼƬ�������
    /// </summary>
    /// <param name="gd"></param>
    /// <returns></returns>
    IEnumerator WaitForDownloadCompelete()
    {
        while (true)
        {
            bool isCompelete = true;
            foreach (GameMap gm in gameData.result.info.Map)
            {
                //����ͼ��ͼ
                if (gm.mapTexture == null) isCompelete = false;
                //����ͼ��Ʒ��ͼ
                foreach (PlacedObject po in gm.Map_Object)
                {
                    if (po.objTexture == null) isCompelete = false;
                }
                //TODO�����������ͼ

            }
            if (isCompelete == true) break;
            yield return null;
        }
        //������ֶ����������
        Debug.Log("�����������");
        isDownloadCompelete = true;
        UpdateScene();
    }
    #endregion

    #region ��ʱ����
    void StartCountTime(int t)
    {
        if(PhotonNetwork.IsMasterClient)
        {
            countTime = t;
            StartCoroutine(CountTime());
        }
    }
    IEnumerator CountTime()
    {
        while(true)
        {
            countTime -= 1;
            GM_PhotonView.RPC("RPCSetTimerText", RpcTarget.All, countTime);
            if(countTime==0)
            {
                GM_PhotonView.RPC("RPCShowVotePanel", RpcTarget.All);
                break;
            }
            yield return new WaitForSeconds(1);
        }
    }

    [PunRPC]
    void RPCSetTimerText(int t)
    {
        string s = "";
        if(t>=3600)
        {
            int hour = t / 3600;
            t = t % 3600;
            int min = t / 60;
            int sec = t % 60;
            if(min>=10)
            {
                if(sec>=10)
                    s = "" + hour + ":" + min + ":" + sec;
                else
                    s = "" + hour + ":" + min + ":0" + sec;
            }
            else
            {
                if (sec >= 10)
                    s = "" + hour + ":0" + min + ":" + sec;
                else
                    s = "" + hour + ":0" + min + ":0" + sec;
            }
            TimerText.text = s;
        }
        else if(t>60)
        {
            int min = t / 60;
            int sec = t % 60;

            if (min >= 10)
            {
                if (sec >= 10)
                    s =""+ min + ":" + sec;
                else
                    s = "" + min + ":0" + sec;
            }
            else
            {
                if (sec >= 10)
                    s = "0" + min + ":" + sec;
                else
                    s = "0" + min + ":0" + sec;
            }
            TimerText.text = s;
        }
        else
        {
            TimerText.text = "" + t;
        }
    }

    [PunRPC]
    void RPCShowVotePanel()
    {
        votePanel.SetActive(true);
        votePanel.GetComponent<VotePanel>().CreatePlayerItem(GameObject.FindGameObjectsWithTag("Player"));
    }
#endregion

}
