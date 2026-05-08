using UnityEngine;
using Photon.Pun;
using Photon.Realtime; 

public class NetworkManager : MonoBehaviourPunCallbacks
{
    void Start()
    {
        Debug.Log("Photon sunucusuna baðlanýlýyor...");
        PhotonNetwork.SendRate = 15;
        PhotonNetwork.SerializationRate = 15;

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Baþarýyla Master sunucusuna baðlanýldý!");
        Debug.Log("Rastgele bir odaya katýlmaya çalýþýlýyor...");

        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Açýk bir oda bulunamadý. Yeni oda kuruluyor...");

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 4;

        PhotonNetwork.CreateRoom(null, roomOptions); 
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Bir odaya baþarýyla katýlýndý! Odadaki kiþi sayýsý: " + PhotonNetwork.CurrentRoom.PlayerCount);

        PhotonNetwork.Instantiate("NetworkPlayer", Vector3.zero, Quaternion.identity);
    }
}