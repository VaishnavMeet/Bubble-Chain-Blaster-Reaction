using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class GameManagerPho : MonoBehaviourPun
{
    public GameObject boxPrefab; // Your box prefab
    public Transform gridPanel;  // Your panel where grid lives (must have GridLayoutGroup)
    public int columns = 15;
    public int rows = 8;

    public BoxCellPho[,] grid;
     PhotonView view;
    public enum Player { None, Player1, Player2 }
    public Player currentPlayer = Player.Player1;

    public GameObject redFlyingBall;
    public GameObject blueFlyingBall;
    public RectTransform flyingBallLayer;
    private int currentTurnActorId;

    public Text playerText;

    void Start()
    {
        view = GetComponent<PhotonView>();
        playerText.text = PhotonNetwork.NickName;
        grid = new BoxCellPho[rows, columns];
        GenerateGrid();

        if (PhotonNetwork.IsMasterClient)
        {
            // Master decides and syncs
            int player1ActorId = PhotonNetwork.MasterClient.ActorNumber;
            
                int player2ActorId = 2;
            
            
            view.RPC("RPC_InitializeTurn", RpcTarget.All, (int)Player.Player1, player1ActorId);
        }

    }

    [PunRPC]
    void RPC_InitializeTurn(int startingPlayer, int actorId)
    {
        currentPlayer = (Player)startingPlayer;
        currentTurnActorId = actorId;
        UpdatePlayerText();
    }

    void UpdatePlayerText()
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == currentTurnActorId)
            playerText.text = "Your Turn";
        else
            playerText.text = "Waiting...";
    }



    void GenerateGrid()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                GameObject boxObj = Instantiate(boxPrefab, gridPanel);
                BoxCellPho cell = boxObj.GetComponent<BoxCellPho>();
                cell.Setup(row, col, this);
                grid[row, col] = cell;
            }
        }
    }

    public void OnBoxClicked(BoxCellPho box)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber != currentTurnActorId)
            return; // It's not your turn

        if (box.CanAddBall(currentPlayer))
        {

            //box.AddBall(currentPlayer);
            view.RPC("RPC_AddBall", RpcTarget.All, (int)currentPlayer, box.row, box.col);
            view.RPC("RPC_SwitchTurn", RpcTarget.All); // sync turn switching
        }
    }
    [PunRPC]
    public void RPC_AddBall(int player, int row, int col)
    {
        GameManagerPho.Player current = (GameManagerPho.Player)player;

        BoxCellPho box = grid[row, col]; // Get the correct box

        box.ballCount++;
        box.owner = current;

        if (box.ballCount > box.GetMaxBalls())
            StartCoroutine(box.ExplodeCoroutine(box.owner));
        else
            box.UpdateVisuals();
    }

    [PunRPC]
    void RPC_SwitchTurn()
    {
        currentPlayer = (currentPlayer == Player.Player1) ? Player.Player2 : Player.Player1;

        if (currentPlayer == Player.Player1)
            currentTurnActorId = PhotonNetwork.MasterClient.ActorNumber;
        else
            currentTurnActorId = 2;

        UpdatePlayerText();
    }




    void SwitchTurn()
    {
        currentPlayer = (currentPlayer == Player.Player1) ? Player.Player2 : Player.Player1;
    }
}
