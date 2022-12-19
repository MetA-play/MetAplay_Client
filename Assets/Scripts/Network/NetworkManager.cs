using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using Google.Protobuf;
using Google.Protobuf.Protocol;

public class NetworkManager : MonoBehaviour
{

    static NetworkManager _instance;
    public static NetworkManager Instance { get { return _instance; } }
    ServerSession _session = new ServerSession();

    List<RoomInfo> roomList  = new List<RoomInfo>();
    public List<RoomInfo> RoomList
    {
        get { return roomList;} 
    }

    private void Start()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        Init();
    }

    public void Send(IMessage packet)
    {
        _session.Send(packet);
    }

    public void Init()
    {
        // DNS (Domain Name System)
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = IPAddress.Parse("10.82.17.113");
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

        Connector connector = new Connector();

        connector.Connect(endPoint,
            () => { return _session; },
            1);
    }

    public void Update()
    {
        List<PacketMessage> list = PacketQueue.Instance.PopAll();
        foreach (PacketMessage packet in list)
        {
            Action<PacketSession, IMessage> handler = PacketManager.Instance.GetPacketHandler(packet.Id);
            if (handler != null)
                handler.Invoke(_session, packet.Message);
        }
    }

    public void CreateRoom(RoomSetting setting)
    {
        C_CreateroomReq req = new C_CreateroomReq();
        req.Setting = setting;
        _session.Send(req);
    }

    public void JoinRoom(int Id)
    {
        C_JoinroomReq req = new C_JoinroomReq();
        req.RoomId = Id;
        _session.Send(req);
    }

    public void RoomListUpdate(List<RoomInfo> infos)
    {
        roomList = infos;
    }

}
