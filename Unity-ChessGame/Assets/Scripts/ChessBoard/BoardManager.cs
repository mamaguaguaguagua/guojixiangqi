using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ChessModel;//ChessModel�����ռ�
using Exploder;//Exploderʵ�ֱ�ըЧ��
using Exploder.Utils;
using UnityEngine;

public class BoardManager : MonoBehaviour{
    private TileManager _tileManager;//��Ƭ������
    private PieceManager _pieceManager;//���ӹ�����
    private ObjectPool _objectPool;//����أ������������ӣ�
    //UI���
    public PromotionUIScript _promotionScript; 
    public EndGameUI EndGameUI;

    private ChessBoard _chessBoard;//�����߼�

    private Dictionary<ChessColor, Player.Player> _players;//�ֵ�������Ϣ

    private List<Move> _legalMoves;//�����б�洢�Ϸ��߷�
    //��Ϸ״̬����
    public static bool _humainPlayer;
    private bool _firstClick;
    public bool playing { get; set; }
    private bool paused;
    //�����
    public GameObject whiteCam;
    public GameObject menuCam;
    //�洢���Ӻ����Ӧ�Ľű�
    private Dictionary<Piece, GameObject> _map;
    #region ��Ϸ�ܿ�����
    private void Start()
    {   //��ȡ���Ӷ��ڹ�����
        _tileManager = GetComponentInChildren<TileManager>();
        _pieceManager = GetComponentInChildren<PieceManager>();
        _objectPool = GetComponentInChildren<ObjectPool>();
        //��ʼ�����ĺ����߼�����
        _chessBoard = new ChessBoard(this);
        _chessBoard.Rock += RockDone;//������λ����

        _map = new Dictionary<Piece, GameObject>(32);//��ʼ��ӳ���ֵ�
        //��ʼ����Ϸ״̬����
        _humainPlayer = false;
        _firstClick = true;

        playing = false;
        paused = true;

        _legalMoves = new List<Move>();
        //��ʼ���߼�����״̬
        _chessBoard.InitializeBoard();
        //ʵ��������
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
        //������ͣ
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
    /// �����Ƭ����
    /// </summary>
    /// <param name="placement"></param>
    public void ClickTile(int placement)
    {
        if (!_humainPlayer || !playing) return;//�������࣬��Ϸû��ʼ���Ƶ��
        //��һ�ε��ֻ��ѡ������
        if (_firstClick)
        {
            if ((_legalMoves = _chessBoard.GetMoveFromPosition(placement)).Any())
            {
                _firstClick = false;
            }
        }
        //ѡ����Ŀ���
        else
        {
            //ɸѡ�Ϸ��߷�
            _legalMoves = _legalMoves.Where(move => move.EndPosition.Equals(placement)).ToList();//�������кϷ��ƶ��еĵ�����ƶ�����������ֵ��legalmoves��ɸѡ���߳�����һ������������
            if (_legalMoves.Count == 1)
            {
                var move = _legalMoves[0];
                _humainPlayer = false;
                _chessBoard.Play(move);//�����߼�����״̬
                MovePiece(_map[move.Piece], move);//���������ƶ�����
                _legalMoves.Clear();//���List
                _firstClick = true;
            }
            //���Ը������������
            else
            {
                _legalMoves = _chessBoard.GetMoveFromPosition(placement);
            }
        }
        //��ʾ��������ʾ�Ϸ�Ŀ�����
        _tileManager.updateLegalMoves(_legalMoves);
    }
    /// <summary>
    /// ��һ�غϷ���
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
    //���¿�ʼ��Ϸ����
    public void RestartGame()
    {
        _chessBoard.InitializeBoard();//�����߼�����
        //����ը��Ƭ������еĻ���
        FragmentPool.Instance.DeactivateFragments();
        FragmentPool.Instance.DestroyFragments();
        FragmentPool.Instance.Reset(ExploderSingleton.Instance.Params);

        foreach (var piece in _map)
        {
            piece.Value.SetActive(false);
        }
        //������Ϸ״̬
        _humainPlayer = false;
        _legalMoves.Clear();
        _tileManager.updateLegalMoves(_legalMoves);
        _firstClick = true;
        //���´���������Gameobject��ӳ��
        _map.Clear();//����ֵ�
        foreach (var piece in _chessBoard.Board)
        {
            if (piece.Type == ChessType.None) continue;
            GameObject gameObjectPiece = createPieceOnPlacement(piece.Type, piece.Color, piece.Position);
            gameObjectPiece.GetComponent<PiecePieces>().ResetMovement();//���ö���
            _map.Add(piece, gameObjectPiece);
        }
        playing = true;//��Ϸ��ʼ
    }
    /// <summary>
    /// ��ʼ�����
    /// </summary>
    /// <param name="players"></param>
    public void InitialisePlay(Dictionary<ChessColor, Player.Player> players)
    {
        //�л������
        if (players[ChessColor.White] != null && players[ChessColor.Black] == null)
            whiteCam.SetActive(false);
        //���������Ϣ
        _players = players;
        //�رղ˵������
        menuCam.SetActive(false);
        GetComponent<AudioSource>().Play();//���ű�������
        //����״̬
        playing = true;
        paused = true;
    }
    #endregion
    #region ���ӹ�����
    /// <summary>
    /// ����ش������ӷ���
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
    /// �����䷽��
    /// </summary>
    /// <param name="piece"></param>
    public void Promotion(Piece piece)
    {
        StartCoroutine(PromotionWait(piece));
        playing = false;
    }

    private IEnumerator PromotionWait(Piece piece)
    {
        yield return new WaitForSeconds(1f);//ͣ��һ��
        yield return new WaitUntil(() => _map[piece].GetComponent<PiecePieces>()._arrived);//�ȴ����Ӷ������
        if (_players[_chessBoard.NextToPlay.Reverse()] == null)//�ж����������
        {
            _promotionScript.show(piece);//��ʾ����UI
        }
        else GivePromotion(piece, ChessType.Queen);//AI��Ĭ������Ϊ��
    }
    //���䷽��
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
    #region ��Ϸ�¼�������

    //��Ϸʤ������
    public void EndGameWin(ChessColor color, Piece piece)
    {
        StartCoroutine(IEEndGameWin(color, piece));
    }
    //ƽ������
    public void EndGameNull(Piece piece)
    {
        StartCoroutine(IEEndGameNull(piece));
    }

    private IEnumerator IEEndGameWin(ChessColor color, Piece piece)
    {
        playing = false;
        yield return new WaitForSeconds(1f);
        yield return new WaitUntil(() => _map[piece].GetComponent<PiecePieces>()._arrived);//Э�̵ȴ����Ӷ������
                                                                                           
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

    //������λ����
    private void RockDone(object sender, Move move)
    {
        MovePiece(_map[move.Piece], move, true);
    }
    #endregion
    #region ��������
    //Update���л�����ӽǵķ���
    private void SwapCam()
    {
        whiteCam.SetActive(!whiteCam.activeInHierarchy);
    }



    /// <summary>
    /// ִ���ƶ����������Ӻ���ͨ�ƶ�������
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