using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkingObject : MonoBehaviour
{
    public int Id { get; set; }
    public bool isMine { get; set; }

    public bool isStaticObject; // 정적인 오브젝트인가?
    public virtual void Start()
    {
        if(isMine)
            StartCoroutine(PosSyncSendCor());
    }

    void Update()
    {
        
    }

    public IEnumerator PosSyncSendCor()
    {

        while (true)
        {
            if (isStaticObject) break;
            yield return null;

            C_Move move = new C_Move();
            move.InputFlag = 0;
            move.Transform = new TransformInfo()
            {
                Pos = new Vector()
                {
                    X = transform.position.x,
                    Y = transform.position.y,
                    Z = transform.position.z
                },
                Rot = new Vector() { X = transform.eulerAngles.x, Y = transform.eulerAngles.y, Z = transform.eulerAngles.z }
            };

            NetworkManager.Instance.Send(move);
            yield return new WaitForSeconds(10);
        }
    }

}