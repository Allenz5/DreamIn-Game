using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TextFollow : MonoBehaviour
{
    public Transform target;

    public PhotonView photonView;

    // ��Ҫ���������꣨����ʵʱ��Ч��
    public bool freazeX, freazeY;

    // �����ƽ��ʱ�䣨�������ͺ�ʱ�䣩
    public float smoothTime = 0.05F;
    private float xVelocity, yVelocity = 0.0F;

    // �����ƫ����
    private Vector3 offset;

    // ȫ�ֻ����λ�ñ���
    private Vector3 oldPosition;

    // ��¼��ʼλ��
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void LateUpdate()
    {
        if (target != null)
        {
            oldPosition = transform.position;
            if (!freazeX)
            {
                oldPosition.x = Mathf.SmoothDamp(transform.position.x, target.transform.position.x + offset.x, ref xVelocity, smoothTime);
            }

            if (!freazeY)
            {
                oldPosition.y = Mathf.SmoothDamp(transform.position.y, target.transform.position.y + offset.y, ref yVelocity, smoothTime);
            }
            transform.position = oldPosition;
        }
    }

    public void SetTarget(GameObject go)
    {
        target = go.transform;
        offset = transform.position - target.transform.position;
        //photonView.RPC("RPCSetTarget", RpcTarget.All, go);
    }
    [PunRPC]
    public void RPCSetTarget(GameObject go)
    {
        target = go.transform;
        offset = transform.position - target.transform.position;
    }

    /// <summary>
    /// �������¿�ʼ��Ϸʱֱ���������λ��
    /// </summary>
    public void ResetPosition()
    {
        transform.position = startPosition;
    }
}
