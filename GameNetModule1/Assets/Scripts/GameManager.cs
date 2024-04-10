using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameObject playerPrefab;
    // Start is called before the first frame update

    public static GameManager instace;
    private void Awake()
    {
        if (instace != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instace = this;
        }
    }
    void Start()
    {
        if(PhotonNetwork.IsConnected)
        {
            if (playerPrefab != null)
            {
                int xRandomPoint = Random.Range(-20, 20);
                int zRandomPoint = Random.Range(-20, 20);
                PhotonNetwork.Instantiate(playerPrefab.name,new Vector3 (xRandomPoint,0,zRandomPoint),Quaternion.identity);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnJoinedRoom()
    {
        Debug.Log(PhotonNetwork.NickName + "Has Joined The Room ");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log(PhotonNetwork.NickName + "Has Joined The Room " + PhotonNetwork.CurrentRoom.Name);
        Debug.Log("Rooom Has Now " + PhotonNetwork.CurrentRoom.PlayerCount + "/20 Players");
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("GameLauncherScene");
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
}

