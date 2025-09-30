namespace ChessModel
{ 
    public class Move//表示一个移动动作
    {
        public int StartPosition { get; }
        public int EndPosition { get; }
        
        //执行移动的棋子
        public Piece Piece { get; }
        //被吃的棋子
        public Piece EatenPiece { get; }
        //构造函数
        public Move(int startPosition, int endPosition, Piece piece, Piece eatenPiece)
        {
            StartPosition = startPosition;
            EndPosition = endPosition;
            Piece = piece;
            EatenPiece = eatenPiece;
        }
        //吃子的标识
        public bool Eat => EatenPiece.Type != ChessType.None;

        public override string ToString()
        {
            return StartPosition + ", " + EndPosition + ", " + Piece.Type + ", " + EatenPiece.Type;
        }
    }
}