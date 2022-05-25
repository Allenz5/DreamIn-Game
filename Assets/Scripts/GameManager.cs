using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Networking;

public class GameManager : MonoBehaviourPunCallbacks
{ 
    public GameObject readyButton;
    public GameObject scriptScroll;
    public GameObject canvas;
    public GameObject objects;
    public GameObject colliders;
    public GameObject votePanel;
    public Text TimerText;

    [SerializeField]
    private int countTime=0;//����ʱ��ʱ����

    private PhotonView GM_PhotonView;
    private GameData gameData;

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
        if (gameData == null)
        {
            Debug.Log("��δѡ��籾�����ݻ�δ������ϣ�");
            return;
        }

        StartGame();
        //GameObject obj = PhotonNetwork.Instantiate("Object", canvas.transform.position + new Vector3(100, 100, 100), Quaternion.identity, 0);
        //GameObject wall = PhotonNetwork.Instantiate("Wall", canvas.transform.position - new Vector3(100, 100, 100), Quaternion.identity, 0);
    }
    GameObject map;
    void StartGame()
    {
        readyButton.SetActive(false);

        //��ʼ����ͼ
        {
            GameObject map = PhotonNetwork.Instantiate("BasedObject", new Vector2(0, 0), Quaternion.identity, 0);
            map.gameObject.name = "GameMap";
            //���õ�ͼ��λ��
            float w = canvas.GetComponent<RectTransform>().rect.width;
            float h = canvas.GetComponent<RectTransform>().rect.height;
            map.GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);
            map.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            map.transform.SetSiblingIndex(0);//���õ�ͼ��UI�㼶�����²�

            //����sprite
            float sw = gameData.result.info.Map[0].mapTexture.width;
            float sh = gameData.result.info.Map[0].mapTexture.height;
            map.GetComponent<Image>().sprite = Sprite.Create(gameData.result.info.Map[0].mapTexture, new Rect(0, 0, sw, sh), new Vector2(0, 0));

        }

        //��ʼ��Object
        {
            //Ŀǰmap��һ�����飬������ʱ������ֻ��ȡmap[0]
            for (int i = 0; i < gameData.result.info.Map[0].Map_Object.Count; i++)
            {
                GameObject obj = PhotonNetwork.Instantiate("BasedObject", new Vector2(0, 0), Quaternion.identity, 0);

                //��ʱ������������image_link
                obj.gameObject.name = "obj" + i; //gameData.result.info.Map[0].Map_Object[i].image_link;

                float w = gameData.result.info.Map[0].Map_Object[i].objTexture.width;
                float h = gameData.result.info.Map[0].Map_Object[i].objTexture.height;

                obj.GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);
                obj.GetComponent<Image>().sprite = Sprite.Create(gameData.result.info.Map[0].Map_Object[i].objTexture, new Rect(0, 0, w, h), new Vector2(0, 0));
                obj.GetComponent<RectTransform>().anchoredPosition = gameData.result.info.Map[0].Map_Object[i].GetPosition();
            }
        }

        StartCountTime(gameData.result.info.length);
        //GameObject player = PhotonNetwork.Instantiate("Player", canvas.transform.position, Quaternion.identity, 0);
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
