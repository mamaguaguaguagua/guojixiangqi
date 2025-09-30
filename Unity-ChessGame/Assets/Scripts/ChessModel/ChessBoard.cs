using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ChessModel
{
    public class ChessBoard
    {
        //单例模式类，方便其他地方直接获取方法
        public static ChessBoard Instance { get; private set; }
        //王车易位表示
        public bool WhiteLeftCastle { get; private set; }
        public bool WhiteRightCastle { get; private set; }
        public bool BlackLeftCastle { get; private set; }
        public bool BlackRightCastle { get; private set; }
        //记录是否王车易位的表示
        public bool BlackHasCastle { get; private set; }
        public bool WhiteHasCastle { get; private set; }
        //双方棋力评估
        private int BlackCount { get; set; }
        private int WhiteCount { get; set; }
        //回合计数
        private int NbMovesWhite { get; set; }
        private int NbMovesBlack { get; set; }
        //目前该谁走棋
        public ChessColor NextToPlay { get; private set; }
        //移动历史栈，记录历史移动
        private readonly Stack<MoveInfo> _moveHistory;
        //一个长度为64的数组用于记录棋盘上所有棋子
        public Piece[] Board { get; }
        //引入棋盘管理器用于升变，结束游戏的实现
        private readonly BoardManager _boardManager;
        //构造函数用于初始化
        public ChessBoard(BoardManager boardManager)
        {
            Board = new Piece[64];
            _boardManager = boardManager;
            _moveHistory = new Stack<MoveInfo>(50);
            Instance = this;
            InitializeBoard();
        }
        //获取最后一部移动
        public Move LastMove => _moveHistory.Any() ? _moveHistory.Peek().Move : null;
        /// <summary>
        /// 执行一步移动方法
        /// </summary>
        /// <param name="move"></param>
        /// <param name="simulation"></param>
        #region 核心流程方法
        public void Play(Move move, bool simulation = false)
        {   //将当前移动和棋盘状态存到历史栈里
            _moveHistory.Push(new MoveInfo(move, WhiteLeftCastle, WhiteRightCastle, BlackLeftCastle, BlackRightCastle,
                WhiteHasCastle, BlackHasCastle));
            var startPosition = move.StartPosition;
            var endPosition = move.EndPosition;
            //处理特殊规则
            UpdateCastle(move, simulation);
            TestPromotion(move, simulation);
            UpdateScore(endPosition);
            //如果吃子，目标位置棋子位置为空
            if (move.Eat)
                Board[move.EatenPiece.Position] = new Piece(ChessColor.None, move.EatenPiece.Position, ChessType.None);
            //交换棋子位置
            Switch(startPosition, endPosition);
            NextToPlay = NextToPlay.Reverse();//更新移动方
            //检查胜利
            if (!simulation && IsCheckMate)
            {
                _boardManager.EndGameWin(NextToPlay.Reverse(), move.Piece);
            }
            //检查和局
            else if (!simulation && IsDraw())
            {
                _boardManager.EndGameNull(move.Piece);
            }
            // Debug.Log(ToString());
        }

        //悔棋方法
        public void Unplay()
        {
            //弹出上一步的信息
            var moveInfo = _moveHistory.Pop();
            var move = moveInfo.Move;
            var startPosition = move.StartPosition;
            var endPosition = move.EndPosition;
            //恢复原来的位置
            Switch(startPosition, endPosition);
            //恢复被吃的棋子
            if (move.Eat)
                Board[move.EatenPiece.Position] = move.EatenPiece;
            //撤销权重判断
            if (NextToPlay == ChessColor.White)
                BlackCount -= Board[endPosition].Value;
            else
                WhiteCount -= Board[endPosition].Value;
            //恢复王车易位的情况
            switch (endPosition)
            {
                case 2 when startPosition == 4:
                    Switch(0, 3);
                    break;
                case 6 when startPosition == 4:
                    Switch(5, 7);
                    break;
                case 58 when startPosition == 60:
                    Switch(56, 59);
                    break;
                case 62 when startPosition == 60:
                    Switch(61, 63);
                    break;
            }
            //恢复王车易位的标志状态
            WhiteLeftCastle = moveInfo.WhiteLeftCastle;
            WhiteRightCastle = moveInfo.WhiteRightCastle;
            BlackLeftCastle = moveInfo.BlackLeftCastle;
            BlackRightCastle = moveInfo.BlackRightCastle;
            WhiteHasCastle = moveInfo.WhiteHasCastle;
            BlackHasCastle = moveInfo.BlackHasCastle;
            NextToPlay = NextToPlay.Reverse();
        }
        //初始化棋盘
        public void InitializeBoard()
        {
            NextToPlay = ChessColor.White;
            Board[0] = new Piece(ChessColor.White, 0, ChessType.Rook);
            Board[1] = new Piece(ChessColor.White, 1, ChessType.Knight);
            Board[2] = new Piece(ChessColor.White, 2, ChessType.Bishop);
            Board[3] = new Piece(ChessColor.White, 3, ChessType.Queen);
            Board[4] = new Piece(ChessColor.White, 4, ChessType.King);
            Board[5] = new Piece(ChessColor.White, 5, ChessType.Bishop);
            Board[6] = new Piece(ChessColor.White, 6, ChessType.Knight);
            Board[7] = new Piece(ChessColor.White, 7, ChessType.Rook);
            for (var i = 8; i < 16; i++)
            {
                Board[i] = new Piece(ChessColor.White, i, ChessType.Pawn);
            }

            for (var i = 16; i < 48; i++)
            {
                Board[i] = new Piece(ChessColor.None, i, ChessType.None);
            }

            for (var i = 48; i < 56; i++)
            {
                Board[i] = new Piece(ChessColor.Black, i, ChessType.Pawn);
            }

            Board[56] = new Piece(ChessColor.Black, 56, ChessType.Rook);
            Board[57] = new Piece(ChessColor.Black, 57, ChessType.Knight);
            Board[58] = new Piece(ChessColor.Black, 58, ChessType.Bishop);
            Board[59] = new Piece(ChessColor.Black, 59, ChessType.Queen);
            Board[60] = new Piece(ChessColor.Black, 60, ChessType.King);
            Board[61] = new Piece(ChessColor.Black, 61, ChessType.Bishop);
            Board[62] = new Piece(ChessColor.Black, 62, ChessType.Knight);
            Board[63] = new Piece(ChessColor.Black, 63, ChessType.Rook);
            //重置棋局状态
            WhiteLeftCastle = WhiteRightCastle = BlackRightCastle = BlackLeftCastle = true;
            WhiteHasCastle = BlackHasCastle = false;
            WhiteCount = BlackCount = 0;
        }
        #endregion

        #region 移动和生成验证
        //获得指定位置棋子的所有合法移动
        public List<Move> GetMoveFromPosition(int position)
        {
            var piece = Board[position];
            return piece.Color == NextToPlay ? piece.GetLegalMoves() : new List<Move>();
        }
        //获取某一方所有合法移动（用于AI计算）
        public List<Move> GetAllLegalMoves(ChessColor color)
        {
            return Board.Where(piece => piece.Color == color).SelectMany(piece => piece.GetLegalMoves()).ToList();
        }
        #endregion

        #region 游戏状态的检查
        //检查是否被将军
        public bool IsCheck(ChessColor color)
        {
            var position = Array
                    .Find(Board, piece1 =>
                        piece1.Color == color && piece1.Type == ChessType.King).Position;

            return Array.Exists(Board, piece =>
                piece.Color == color.Reverse() && piece.GetPseudoMoves().Exists(move => move.EndPosition.Equals(position)));
        }
        //检查棋子是否能威胁到某一个位置
        public bool IsThreatening(int position, ChessColor color)
        {
            return Array.Exists(Board, piece =>
                piece.Color == color && piece.GetPseudoMoves()
                    .Exists(move => move.EndPosition == position));
        }
        //检查是否将死
        public bool IsCheckMate => IsCheck(NextToPlay) &&
                                   !Array.Exists(Board,
                                       piece => piece.Color == NextToPlay && piece.GetLegalMoves().Any());
        //检车是否平局
        public bool IsDraw()
        {
            return IsPat || InsufficientMaterial;
        }
        //检查逼和
        private bool IsPat => !IsCheck(NextToPlay) &&
                              !Array.Exists(Board, piece => piece.Color == NextToPlay && piece.GetLegalMoves().Any());
        //检查棋力是否不够完成本局
        private bool InsufficientMaterial
        {
            get
            {
                var x = Board.Where(piece => piece.Type != ChessType.None && piece.Type != ChessType.King).ToList();
                return !x.Any() ||
                       x.Count == 1 && (x[0].Type == ChessType.Bishop || x[0].Type == ChessType.Knight) ||
                       x.Count == 2 && x[0].Type == ChessType.Bishop && x[1].Type == ChessType.Bishop &&
                       x[0].Color == x[1].Color;
            }
        }
        #endregion

        #region 特殊规则的处理
        //实现王车易位的方法
        private void UpdateCastle(Move move, bool simulation)
        {
            //白王移动方法
            if (move.StartPosition == 4 && (WhiteLeftCastle || WhiteRightCastle))
            {
                if (WhiteLeftCastle && move.EndPosition == 2)
                {
                    if (!simulation)
                        Rock?.Invoke(this, new Move(0, 3,
                            Board[0],
                            Board[3]));
                    Switch(0, 3);
                    WhiteHasCastle = true;
                }

                if (WhiteRightCastle && move.EndPosition == 6)
                {
                    if (!simulation)
                        Rock?.Invoke(this, new Move(7, 5,
                            Board[7],
                            Board[5]));
                    Switch(5, 7);
                    WhiteHasCastle = true;
                }
                //王移动后失去权限
                WhiteLeftCastle = WhiteRightCastle = false;
            }
            //黑王移动
            else if (move.StartPosition == 60 && (BlackLeftCastle || BlackRightCastle))
            {
                if (BlackLeftCastle && move.EndPosition == 58)
                {
                    if (!simulation)
                        Rock?.Invoke(this, new Move(56,
                            59,
                            Board[56],
                            Board[59]));
                    Switch(56, 59);
                    BlackHasCastle = true;
                }

                if (BlackRightCastle && move.EndPosition == 62)
                {
                    if (!simulation)
                        Rock?.Invoke(this, new Move(63,
                            61,
                            Board[63],
                            Board[61]));
                    Switch(61, 63);
                    BlackHasCastle = true;
                }

                BlackLeftCastle = BlackRightCastle = false;
            }
            //车移动了失去权限
            else if (WhiteLeftCastle && move.StartPosition == 0)
                WhiteLeftCastle = false;
            else if (WhiteRightCastle && move.StartPosition == 7)
                WhiteRightCastle = false;
            else if (BlackLeftCastle && move.StartPosition == 56)
                BlackLeftCastle = false;
            else if (BlackRightCastle && move.StartPosition == 63)
                BlackRightCastle = false;
        }
        //兵升变的检测方法
        private void TestPromotion(Move move, bool simulation)
        {
            if (move.Piece.Color == ChessColor.Black && move.Piece.Type == ChessType.Pawn &&
                move.EndPosition / 8 == 0 ||
                move.Piece.Color == ChessColor.White && move.Piece.Type == ChessType.Pawn && move.EndPosition / 8 == 7)
            {
                PromotePawn(move, simulation);
            }
        }
        //检测兵升变逻辑
        private void PromotePawn(Move move, bool simulation)
        {
            if (!simulation)
                AskPromotion(move.Piece);
        }
        //执行升变逻辑的方法
        public void PromotePawn(Piece piece, ChessType type)
        {
            piece.Type = type;
        }
        //通过管理器实现兵升变逻辑，实现UI显示升变界面
        private void AskPromotion(Piece piece)
        {
            _boardManager.Promotion(piece);
        }


        #endregion

        #region 辅助和评估方法
        //根据位置获取棋子的方法
        public Piece GetPiece(int position)
        {
            return Board[position];
        }
        //Ai评估函数
        public float GetEvaluationScore()
        {
            float score = WhiteCount - BlackCount;//基础分数就是双方棋力差
            //威胁加分
            if (IsThreatening(35, ChessColor.White) || IsThreatening(36, ChessColor.White)) score += 0.1f;
            if (IsThreatening(27, ChessColor.Black) || IsThreatening(28, ChessColor.Black)) score -= 0.1f;
            //王车易位加分
            if (!WhiteLeftCastle && !WhiteRightCastle && !WhiteHasCastle) score -= 0.9f;
            if (!BlackLeftCastle && !BlackRightCastle && !BlackHasCastle) score += 0.9f;
            if (WhiteHasCastle) score += 0.9f;
            if (BlackHasCastle) score -= 0.9f;
            return score;
        }
        //（吃子后调用的）更新棋力分数差
        private void UpdateScore(int endPosition)
        {
            if (NextToPlay == ChessColor.White)
                WhiteCount += Board[endPosition].Value;
            else
                BlackCount += Board[endPosition].Value;
        }
        //检查某个位置是否为空
        public bool IsEmpty(int position)
        {
            return Board[position].Type == ChessType.None;
        }
        //获取棋子颜色
        public ChessColor? Color(int position)
        {
            return GetPiece(position).Color;
        }

        //获取棋盘格颜色
        public ChessColor TileColor(int position)
        {
            return position / 8 % 2 == 0 ? position % 2 == 0 ? ChessColor.Black :
                ChessColor.White :
                position % 2 == 0 ? ChessColor.White : ChessColor.Black;
        }
        //交换位置的方法
        private void Switch(int position1, int position2)
        {
            (Board[position1], Board[position2]) = (Board[position2], Board[position1]);
            Board[position1].MoveTo(position1);
            Board[position2].MoveTo(position2);
        }


        #endregion     
        //王车易位事件
        public event EventHandler<Move> Rock;
        //打印当前棋盘状态
        public override string ToString()
        {
            return Board.Aggregate("", (current, piece) => current + (piece + "\n"));
        }
    }
}