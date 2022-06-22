using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class t_RPCGameObject : MonoBehaviour
{
    //������Ҫ������GameCanvas�ϵ���Ϸ�������������ش����Է�ֹ��������
    public void Awake()
    {
        AddToCanvas();
    }

    public void AddToCanvas()
    {
        GameObject canvas = GameObject.Find("GameCanvas");
        transform.SetParent(canvas.transform);
        transform.localScale = new Vector3(1, 1, 1);
    }
}
