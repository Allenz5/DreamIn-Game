using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Networking;

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
    public Text TimerText;

    [SerializeField]
    private int countTime=0;//����ʱ��ʱ����

    private PhotonView GM_PhotonView;
    private GameData gameData;
    private int ColliderSize = 32;

    public void Start()
    {
        GM_PhotonView = GetComponent<PhotonView>();
        if (PhotonNetwork.IsMasterClient)
        {
            scriptScroll.SetActive(true);
        }
    }
    /// <summary>
    /// ������Ϸ���ݣ�ֻ�н��������ݲ��ܹ���ʼ��Ϸ
    /// </summary>
    /// <param name="gd"></param>
    public void SetGameData(GameData gd)
    {
        gameData = gd;
    }
    public void ReadyButton()
    {
        GameObject player = PhotonNetwork.Instantiate("Player", canvas.transform.position, Quaternion.identity, 0);
        readyButton.SetActive(false);
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
        }
    }
    public void UpdateScene()
    {
        //��ʼ����ͼ
        {
            GameObject map = Instantiate(objectPrefab, new Vector2(0, 0), Quaternion.identity, canvas.transform);
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
    }

    public void StartButton()
    {
        if (gameData == null)
        {
            Debug.Log("ûѡ��籾/û�������");
            return;
        }

        //TODO: ��������
        //TODO: ��ʼ��ʱ
        startButton.SetActive(false);
    }


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

}
