using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.Demo.Cockpit;
using System.Security.Cryptography;
using Unity.VisualScripting;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("Connection Status Panel")]
    public Text connectionStatusText;

    [Header("Login UI Panel")]
    public InputField playerNameInput;
    public GameObject loginUIPanel;

    [Header("Game Options Panel")]
    public GameObject gameOptionsPanel;

    [Header("Create Room Panel")]
    public GameObject createRoomPanel;
    public InputField roomNameImputField;
    public InputField playerCountInputField;

    [Header("Join Random Room Panel")]
    public GameObject joinRandomRoomPanel;

    [Header("Show Room List Panel")]
    public GameObject showRoomListPanel;

    [Header("Inside Room Panel")]
    public GameObject insideRoomPanel;
    public Text roomInfoText;
    public GameObject playerListItemPrefab;
    public GameObject playerListViewParent;
    public GameObject startGameButton;

    [Header("Room List Panel")]
    public GameObject roomListPanel;
    public GameObject roomItemPrefab;
    public GameObject roomListParent;

    private Dictionary<string, RoomInfo> cachedRoomList;
    private Dictionary<string, GameObject> roomListGameObjects;
    private Dictionary<int, GameObject> playerListGameObjects;
    #region Unity Functions
    // Start is called before the first frame update
    void Start()
    {
        cachedRoomList = new Dictionary<string, RoomInfo>();
        roomListGameObjects = new Dictionary<string, GameObject>();
        ActivatePanel(loginUIPanel);

        PhotonNetwork.AutomaticallySyncScene = true;// sychs the levels, players who join the room will see the same
    }

    // Update is called once per frame
    void Update()
    {
        connectionStatusText.text = "Connection Status: " + PhotonNetwork.NetworkClientState;
    }
    #endregion

    #region UI Callbacks
    public void OnLoginButtonClicked()
    {
        string playerName=playerNameInput.text;

        if(string.IsNullOrEmpty(playerName))
        {
            Debug.Log("Player Name Is Invalid!");
        }
        else
        {
            PhotonNetwork.LocalPlayer.NickName = playerName;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void OnCreateRoomButtonClicked()
    {
        string roomName = roomNameImputField.text;

        if(string.IsNullOrEmpty (roomName))
        {
            roomName = "Room " + Random.Range(1000, 1000);
        }

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = (byte)int.Parse(playerCountInputField.text);

        PhotonNetwork.CreateRoom(roomName,roomOptions);

    }

    public void OnCancelButtonClicked()
    {
        ActivatePanel(gameOptionsPanel);
    }

    public void OnShowRoomListButtonClicked()
    {
        if(!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
        ActivatePanel(showRoomListPanel);
    }

    public void OnBackButtonClicked()
    {
        if(PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
        ActivatePanel(gameOptionsPanel);
    }//Before mag back Mag leleave muna ng room 

    public void OnLeaveGameButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void OnJoinRandomRoomClicked()
    {
        ActivatePanel(joinRandomRoomPanel);
        PhotonNetwork.JoinRandomRoom();
    }

    public void OnStartGameButtonClicked()
    {
        PhotonNetwork.LoadLevel("Game Scene");
    }
    #endregion

    #region PUN Callbacks
    public override void OnConnected()
    {
        Debug.Log("Connected to the internet");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " has cconnected to photon servers");
        ActivatePanel(gameOptionsPanel);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log(PhotonNetwork.CurrentRoom.Name + " Created!");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " has Joined " + PhotonNetwork.CurrentRoom.Name);
        ActivatePanel (insideRoomPanel);

        roomInfoText.text = "Room Name: " + PhotonNetwork.CurrentRoom.Name + " Current Player Count: " + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;
        //updates yung room name, players sa room sheesh

        if(playerListGameObjects == null)
        {
            playerListGameObjects = new Dictionary<int, GameObject>();
        }
        foreach(Player player in PhotonNetwork.PlayerList)
        {
            GameObject playerItem = Instantiate(playerListItemPrefab);
            playerItem.transform.SetParent(playerListViewParent.transform);
            playerItem.transform.localScale = Vector3.one;

            playerItem.transform.Find("PlayerNameText").GetComponent<Text>().text = player.NickName;
            playerItem.transform.Find("PlayerIndicator").gameObject.SetActive(player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber);//nag aactivate nung YOU sa lobby

            playerListGameObjects.Add(player.ActorNumber, playerItem);//add ng updated na player 
            
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        ClearRoomListGameObjects();
        Debug.Log("OnRoomListUpdate Called");
        
        startGameButton.SetActive(PhotonNetwork.LocalPlayer.IsMasterClient);

        foreach (RoomInfo info in roomList)
        {
            Debug.Log(info.Name);
            if(!info.IsOpen || !info.IsVisible || info.RemovedFromList)
            {
                if (cachedRoomList.ContainsKey(info.Name))
                {
                    cachedRoomList.Remove(info.Name);
                }
            }
            else
            {
                //update existing room info
                if(cachedRoomList.ContainsKey(info.Name))
                {
                    cachedRoomList[info.Name] = info;
                }
                else
                {
                    cachedRoomList.Add(info.Name, info);
                }
            }

        }// checks kung merong doble at mag update ng mga rooms sa roomlist 

        foreach(RoomInfo info in cachedRoomList.Values) 
        {
            GameObject listItem = Instantiate(roomItemPrefab);
            listItem.transform.SetParent(roomListParent.transform);//gawing parent 
            listItem.transform.localScale = Vector3.one;

            listItem.transform.Find("RoomNameText").GetComponent<Text>().text = info.Name;
            listItem.transform.Find("RoomPlayersText").GetComponent<Text>().text = "Player count: " + info.PlayerCount + "/" + info.MaxPlayers;
            listItem.transform.Find("JoinRoomButton").GetComponent<Button>().onClick.AddListener(()=> OnJoinRoomClicked(info.Name));

            roomListGameObjects.Add(info.Name, listItem);
        } //// Spawns yung rooms na prefab sa roomlist,  automatic na ginagawa ng progam unlike sa dati na buttons na manual
    }

    public override void OnLeftLobby()
    {
     ClearRoomListGameObjects();
        cachedRoomList.Clear();
    }// Cineclear lahat ng game objects muna

    public override void OnPlayerEnteredRoom(Player player)
    {
        roomInfoText.text = "Room Name: " + PhotonNetwork.CurrentRoom.Name + " Current Player Count: " + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;
        //updates yung room name, players sa room sheesh
        GameObject playerItem = Instantiate(playerListItemPrefab);
        playerItem.transform.SetParent(playerListViewParent.transform);
        playerItem.transform.localScale = Vector3.one;

        playerItem.transform.Find("PlayerNameText").GetComponent<Text>().text = player.NickName;
        playerItem.transform.Find("PlayerIndicator").gameObject.SetActive(player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber);//nag aactivate nung YOU sa lobby

        playerListGameObjects.Add(player.ActorNumber, playerItem);//add ng updated na player 
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        startGameButton.SetActive(PhotonNetwork.LocalPlayer.IsMasterClient);
        roomInfoText.text = "Room Name: " + PhotonNetwork.CurrentRoom.Name + " Current Player Count: " + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;
        //updates yung room name, players sa room sheesh
        Destroy(playerListGameObjects[otherPlayer.ActorNumber]);
        playerListGameObjects.Remove(otherPlayer.ActorNumber);
    }

    public override void OnLeftRoom()
    {

        foreach (var gameObject in playerListGameObjects.Values)
        {
            Destroy(gameObject);
        }
        playerListGameObjects.Clear();
        playerListGameObjects = null;
        ActivatePanel(gameOptionsPanel);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogWarning(message);

        string roomName = "Room " + Random.Range(1000, 10000);
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 20;

        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }
    #endregion

    #region Private Methods
    private void OnJoinRoomClicked(string roomName)// sumali sa room if clicked,
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
        PhotonNetwork.JoinRoom(roomName);
    }

    private void ClearRoomListGameObjects()
    {
        foreach (var item in roomListGameObjects.Values)
        {
            Destroy(item);
        }

        roomListGameObjects.Clear();

    }
    #endregion

    #region Public Methods
    public void ActivatePanel(GameObject panelToBeActivated)
    {
       loginUIPanel.SetActive(panelToBeActivated.Equals(loginUIPanel));
        gameOptionsPanel.SetActive(panelToBeActivated.Equals(gameOptionsPanel));
        createRoomPanel.SetActive(panelToBeActivated.Equals(createRoomPanel));
        joinRandomRoomPanel.SetActive(panelToBeActivated.Equals(joinRandomRoomPanel));
        showRoomListPanel.SetActive(panelToBeActivated.Equals(showRoomListPanel));
        insideRoomPanel.SetActive(panelToBeActivated.Equals(insideRoomPanel));
        roomListPanel.SetActive(panelToBeActivated.Equals(roomListPanel));
    }


    #endregion
}
