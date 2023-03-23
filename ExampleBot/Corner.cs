namespace SC2Sharp
{
    internal class Corner
    {
        public int X;
        public int Y;

        public Corner(int x, int y)
        {
            X = x;
            Y = y;
        }

        public LeftEdge EdgeDown() { return new LeftEdge(X, Y - 1); }
        public LeftEdge EdgeUp() { return new LeftEdge(X, Y); }
        public BottomEdge EdgeLeft() { return new BottomEdge(X - 1, Y); }
        public BottomEdge EdgeRight() { return new BottomEdge(X, Y); }
        public IntPoint SquareLeftDown() { return new IntPoint(X - 1, Y - 1); }
        public IntPoint SquareRightDown() {return new IntPoint(X, Y - 1); }
        public IntPoint SquareLeftUp() { return new IntPoint(X - 1, Y); }
        public IntPoint SquareRightUp() { return new IntPoint(X, Y); }
    }
}