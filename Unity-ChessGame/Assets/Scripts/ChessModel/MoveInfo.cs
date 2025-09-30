namespace ChessModel
{
    public struct MoveInfo
    {
        //移动本身
        public Move Move { get; }
        //王车易位表示
        public bool WhiteLeftCastle { get; }
        public bool WhiteRightCastle { get; }
        public bool BlackLeftCastle { get; }
        public bool BlackRightCastle { get; }
        public bool BlackHasCastle { get; }
        public bool WhiteHasCastle { get; }
        //构造函数
        public MoveInfo(Move move, bool whiteLeftCastle, bool whiteRightCastle, bool blackLeftCastle, bool blackRightCastle, bool whiteHasCastle, bool blackHasCastle)
        {
            Move = move;
            WhiteLeftCastle = whiteLeftCastle;
            WhiteRightCastle = whiteRightCastle;
            BlackLeftCastle = blackLeftCastle;
            BlackRightCastle = blackRightCastle;
            WhiteHasCastle = whiteHasCastle;
            BlackHasCastle = blackHasCastle;
        }
    }
}