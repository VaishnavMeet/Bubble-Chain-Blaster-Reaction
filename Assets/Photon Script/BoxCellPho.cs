using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;


public class BoxCellPho : MonoBehaviour
{
    public int row, col;
    public GameManagerPho.Player owner = GameManagerPho.Player.None;
    public int ballCount = 0;

    public GameObject[] redBalls;  // Assign prefabs in order: 1,2,3
    public GameObject[] blueBalls;
    public Button button;
    
    private GameManagerPho GameManagerPho;



   

    public void Setup(int r, int c, GameManagerPho manager)
    {
        row = r;
        col = c;
        GameManagerPho = manager;
        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(OnClick);
        else
            Debug.LogError("Button component missing in box prefab!");
    }

    void OnClick()
    {
        
            GameManagerPho.OnBoxClicked(this);
        
    }

    public bool CanAddBall(GameManagerPho.Player current)
    {
        return owner == GameManagerPho.Player.None || owner == current;
    }

    public void AddBall(GameManagerPho.Player current)
    {
        ballCount++;
        owner = current;

        if (ballCount > GetMaxBalls())
        {
            StartCoroutine(ExplodeCoroutine(owner));
        }
        else
        {
            UpdateVisuals();
        }
    }

    [PunRPC]
    public void RPC_AddBall(int player)
    {
        GameManagerPho.Player current = (GameManagerPho.Player)player;

        ballCount++;
        owner = current;

        if (ballCount > GetMaxBalls())
            StartCoroutine(ExplodeCoroutine(owner));
        else
            UpdateVisuals();
    }

    public int GetMaxBalls()
    {
        bool isEdge = row == 0 || row == 7 || col == 0 || col == 14;
        bool isCorner = (row == 0 || row == 7) && (col == 0 || col == 14);

        if (isCorner) return 1;
        else if (isEdge) return 2;
        else return 3;
    }

    public IEnumerator ExplodeCoroutine(GameManagerPho.Player explodingOwner)
    {
        // Reset current cell
        ballCount = 0;
        owner = GameManagerPho.Player.None;
        UpdateVisuals();

        yield return new WaitForSeconds(0.25f); 

        SpreadToNeighbours(explodingOwner);
    }


    void SpreadToNeighbours(GameManagerPho.Player explodingOwner)
    {
        int[,] directions = new int[,]
        {
        { 0, 1 },  // Right
        { 0, -1 }, // Left
        { 1, 0 },  // Down
        { -1, 0 }  // Up
        };

        for (int i = 0; i < 4; i++)
        {
            int newRow = row + directions[i, 0];
            int newCol = col + directions[i, 1];

            if (newRow >= 0 && newRow < 8 && newCol >= 0 && newCol < 15)
            {
                BoxCellPho neighbor = GameManagerPho.grid[newRow, newCol];
                StartCoroutine(FlyBallToNeighbor(explodingOwner, neighbor));
            }
        }
    }

    IEnumerator FlyBallToNeighbor(GameManagerPho.Player fromPlayer, BoxCellPho neighbor)
    {
        

        GameObject ballPrefab = fromPlayer == GameManagerPho.Player.Player1
            ? GameManagerPho.redFlyingBall
            : GameManagerPho.blueFlyingBall;

        GameObject flyingBall = Instantiate(ballPrefab, transform.position, Quaternion.identity, GameManagerPho.flyingBallLayer);

        RectTransform flyRect = flyingBall.GetComponent<RectTransform>();
        Vector3 startPos = ((RectTransform)transform).localPosition;
        Vector3 endPos = ((RectTransform)neighbor.transform).localPosition;

        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            flyRect.localPosition = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        flyRect.localPosition = endPos;
        Destroy(flyingBall);

        neighbor.ReceiveFromExplosion(fromPlayer);
    }






    public void ReceiveFromExplosion(GameManagerPho.Player fromPlayer)
    {
        owner = fromPlayer;
        ballCount++;

        if (ballCount > GetMaxBalls())
        {
            StartCoroutine(ExplodeCoroutine(fromPlayer));
        }
        else
        {
            UpdateVisuals();
        }
    }

    public void UpdateVisuals()
    {
        foreach (var b in redBalls) b.SetActive(false);
        foreach (var b in blueBalls) b.SetActive(false);

        if (owner == GameManagerPho.Player.Player1 && ballCount > 0)
        {
            redBalls[ballCount - 1].SetActive(true);
        }
        else if (owner == GameManagerPho.Player.Player2 && ballCount > 0)
        {
            blueBalls[ballCount - 1].SetActive(true);
        }
    }
}
