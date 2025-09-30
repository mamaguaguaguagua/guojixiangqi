using System.Collections;
using System.Collections.Generic;
using ChessModel;
using UnityEngine;
using UnityEngine.AI;

public class PieceManager : MonoBehaviour
{
    
    private BoardManager _boardManager;
    public GameObject explosion;//爆炸特效

    void Awake()
    {
        _boardManager = GetComponentInParent<BoardManager>();
    }
    
    public void MovePiece(GameObject piece, Vector3 placement, bool rock = false)
    {
        //调用棋子上的PiecePiece中的Move方法
        piece.GetComponent<PiecePieces>().Move(placement, rock);
    }

    public void AttackWithPiece(GameObject piece, Vector3 placement, Vector3 enemyPlacement, GameObject enemy)
    {   //调用吃子动画
        piece.GetComponent<PiecePieces>().Attack(placement, enemyPlacement, enemy);
    }
    //动画完成回调
    public void FinishedAnim()
    {
        _boardManager.NextTurn();
    }
}
