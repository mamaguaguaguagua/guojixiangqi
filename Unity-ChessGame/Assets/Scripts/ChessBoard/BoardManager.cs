using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ChessModel;//ChessModel命名空间
using Exploder;//Exploder实现爆炸效果
using Exploder.Utils;
using UnityEngine;

public class BoardManager : MonoBehaviour{
    private TileManager _tileManager;//瓦片管理器
    private PieceManager _pieceManager;//棋子管理器
    private ObjectPool _objectPool;//对象池（批量处理棋子）
    //UI相关
    public PromotionUIScript _promotionScript; 
    public EndGameUI EndGameUI;

    private ChessBoard _chessBoard;//棋盘逻辑

    private Dictionary<ChessColor, Player.Player> _players;//字典存玩家信息

    private List<Move> _legalMoves;//泛型列表存储合法走法
    //游戏状态控制
    public static bool _humainPlayer;
    private bool _firstClick;
    public bool playing { get; set; }
    private bool paused;
    //摄像机
    public GameObject whiteCam;
    public GameObject menuCam;
    //存储棋子和其对应的脚本
    private Dictionary<Piece, GameObject> _map;
    #region 游戏总控制器
    private void Start()
    {   //获取棋子对于管理器
        _tileManager = GetComponentInChildren<TileManager>();
        _pieceManager = GetComponentInChildren<PieceManager>();
        _objectPool = GetComponentInChildren<ObjectPool>();
        //初始化核心核心逻辑棋盘
        _chessBoard = new ChessBoard(this);
        _chessBoard.Rock += RockDone;//王车易位方法

        _map = new Dictionary<Piece, GameObject>(32);//初始化映射字典
        //初始化游戏状态变量
        _humainPlayer = false;
        _firstClick = true;

        playing = false;
        paused = true;

        _legalMoves = new List<Move>();
        //初始化逻辑棋盘状态
        _chessBoard.InitializeBoard();
        //实例化棋子
        foreach (var piece in _chessBoard.Board)
        {
            if (piece.Type != ChessType.None)
                _map.Add(piece, createPieceOnPlacement(piece.Type, piece.Color, piece.Position));
            if (piece.Color == ChessColor.Black) _map[piece].transform.Rotate(0, 180, 0);
        }

    }
    private void Update()
    {
        if (playing && paused)
        {
            paused = false;
            NextTurn();
        }
        //设置暂停
        if (!playing && !paused)
        {
            paused = true;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            SwapCam();
            //Debug.Log(_chessBoard.GetEvaluationScore());
        }
    }
    /// <summary>
    /// 点击瓦片方法
    /// </summary>
    /// <param name="placement"></param>
    public void ClickTile(int placement)
    {
        if (!_humainPlayer || !playing) return;//不是人类，游戏没开始限制点击
        //第一次点击只能选择棋子
        if (_firstClick)
        {
            if ((_legalMoves = _chessBoard.GetMoveFromPosition(placement)).Any())
            {
                _firstClick = false;
            }
        }
        //选择吗目标格
        else
        {
            //筛选合法走法
            _legalMoves = _legalMoves.Where(move => move.EndPosition.Equals(placement)).ToList();//遍历所有合法移动中的点击的移动步数，并赋值给legalmoves，筛选出走出的那一步（迭代器）
            if (_legalMoves.Count == 1)
            {
                var move = _legalMoves[0];
                _humainPlayer = false;
                _chessBoard.Play(move);//更新逻辑棋盘状态
                MovePiece(_map[move.Piece], move);//播放棋子移动动画
                _legalMoves.Clear();//清空List
                _firstClick = true;
            }
            //可以更换点击的棋子
            else
            {
                _legalMoves = _chessBoard.GetMoveFromPosition(placement);
            }
        }
        //显示高亮，显示合法目标格子
        _tileManager.updateLegalMoves(_legalMoves);
    }
    /// <summary>
    /// 下一回合方法
    /// </summary>
    public void NextTurn()
    {
        if (paused)
        {
            return;
        }

        var nextToPlay = _chessBoard.NextToPlay;


        var currentPlayer = _players[nextToPlay];
        if (currentPlayer == null)
        {
            whiteCam.SetActive(nextToPlay == ChessColor.White);
            _humainPlayer = true;
        }
        else
        {
            var move = currentPlayer.GetDesiredMove();
            MovePiece(_map[move.Piece], move);
            _chessBoard.Play(move);
        }
    }
    //重新开始游戏方法
    public void RestartGame()
    {
        _chessBoard.InitializeBoard();//重置逻辑棋盘
        //清理爆炸碎片（如果有的话）
        FragmentPool.Instance.DeactivateFragments();
        FragmentPool.Instance.DestroyFragments();
        FragmentPool.Instance.Reset(ExploderSingleton.Instance.Params);

        foreach (var piece in _map)
        {
            piece.Value.SetActive(false);
        }
        //重置游戏状态
        _humainPlayer = false;
        _legalMoves.Clear();
        _tileManager.updateLegalMoves(_legalMoves);
        _firstClick = true;
        //重新创建棋子与Gameobject的映射
        _map.Clear();//清空字典
        foreach (var piece in _chessBoard.Board)
        {
            if (piece.Type == ChessType.None) continue;
            GameObject gameObjectPiece = createPieceOnPlacement(piece.Type, piece.Color, piece.Position);
            gameObjectPiece.GetComponent<PiecePieces>().ResetMovement();//重置动画
            _map.Add(piece, gameObjectPiece);
        }
        playing = true;//游戏开始
    }
    /// <summary>
    /// 初始化玩家
    /// </summary>
    /// <param name="players"></param>
    public void InitialisePlay(Dictionary<ChessColor, Player.Player> players)
    {
        //切换摄像机
        if (players[ChessColor.White] != null && players[ChessColor.Black] == null)
            whiteCam.SetActive(false);
        //存入玩家信息
        _players = players;
        //关闭菜单摄像机
        menuCam.SetActive(false);
        GetComponent<AudioSource>().Play();//播放背景音乐
        //设置状态
        playing = true;
        paused = true;
    }
    #endregion
    #region 棋子管理方法
    /// <summary>
    /// 对象池处理棋子方法
    /// </summary>
    /// <param name="pieceType"></param>
    /// <param name="color"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    private GameObject createPieceOnPlacement(ChessType pieceType, ChessColor color, int position)
    {
        var pieceObject = _objectPool.getPooledPiece(pieceType, color, _tileManager.getCoordinatesByTilePlacement(position));
        return pieceObject;
    }

    /// <summary>
    /// 兵升变方法
    /// </summary>
    /// <param name="piece"></param>
    public void Promotion(Piece piece)
    {
        StartCoroutine(PromotionWait(piece));
        playing = false;
    }

    private IEnumerator PromotionWait(Piece piece)
    {
        yield return new WaitForSeconds(1f);//停顿一秒
        yield return new WaitUntil(() => _map[piece].GetComponent<PiecePieces>()._arrived);//等待棋子动画完成
        if (_players[_chessBoard.NextToPlay.Reverse()] == null)//判断是人类玩家
        {
            _promotionScript.show(piece);//显示升变UI
        }
        else GivePromotion(piece, ChessType.Queen);//AI，默认升变为后
    }
    //升变方法
    public void GivePromotion(Piece piece, ChessType chessType)
    {
        switch (chessType)
        {
            case ChessType.Bishop:
                _map[piece].SetActive(false);
                _map[piece] = _objectPool.getPooledPiece(ChessType.Bishop, piece.Color, _tileManager.getCoordinatesByTilePlacement(piece.Position));
                break;
            case ChessType.Rook:
                _map[piece].SetActive(false);
                _map[piece] = _objectPool.getPooledPiece(ChessType.Rook, piece.Color, _tileManager.getCoordinatesByTilePlacement(piece.Position));
                break;
            case ChessType.Queen:
                _map[piece].SetActive(false);
                _map[piece] = _objectPool.getPooledPiece(ChessType.Queen, piece.Color, _tileManager.getCoordinatesByTilePlacement(piece.Position));
                break;
            case ChessType.Knight:
                _map[piece].SetActive(false);
                _map[piece] = _objectPool.getPooledPiece(ChessType.Knight, piece.Color, _tileManager.getCoordinatesByTilePlacement(piece.Position));
                break;
        }
        _chessBoard.PromotePawn(piece, chessType);
        playing = true;
    }

    #endregion
    #region 游戏事件处理方法

    //游戏胜利流程
    public void EndGameWin(ChessColor color, Piece piece)
    {
        StartCoroutine(IEEndGameWin(color, piece));
    }
    //平局流程
    public void EndGameNull(Piece piece)
    {
        StartCoroutine(IEEndGameNull(piece));
    }

    private IEnumerator IEEndGameWin(ChessColor color, Piece piece)
    {
        playing = false;
        yield return new WaitForSeconds(1f);
        yield return new WaitUntil(() => _map[piece].GetComponent<PiecePieces>()._arrived);//协程等待棋子动画完成
                                                                                           
        if (EndGameUI != null)
        {
            EndGameUI.EndGameWin(color);
        }
        else
        {
            Debug.LogError("EndGameUI reference is null!");
        }
        
    }

    private IEnumerator IEEndGameNull(Piece piece)
    {
        playing = false;
        yield return new WaitForSeconds(1f);
        yield return new WaitUntil(() => _map[piece].GetComponent<PiecePieces>()._arrived);
        EndGameUI.EndGameNull();
    }

    //王车易位方法
    private void RockDone(object sender, Move move)
    {
        MovePiece(_map[move.Piece], move, true);
    }
    #endregion
    #region 辅助方法
    //Update中切换相机视角的方法
    private void SwapCam()
    {
        whiteCam.SetActive(!whiteCam.activeInHierarchy);
    }



    /// <summary>
    /// 执行移动动画（吃子和普通移动方法）
    /// </summary>
    /// <param name="piece"></param>
    /// <param name="move"></param>
    /// <param name="rock"></param>
    private void MovePiece(GameObject piece, Move move, bool rock = false)
    {
        int position = move.EndPosition;
        if (move.Eat)
        {
            _pieceManager.AttackWithPiece(piece, _tileManager.getCoordinatesByTilePlacement(position), _tileManager.getCoordinatesByTilePlacement(move.EatenPiece.Position), _map[move.EatenPiece]);
        }
        else
        {
            _pieceManager.MovePiece(piece, _tileManager.getCoordinatesByTilePlacement(position), rock);
        }
    }

    #endregion
}