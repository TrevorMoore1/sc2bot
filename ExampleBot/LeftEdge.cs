namespace SC2Sharp
{
    internal class LeftEdge
    {
        public int X;
        public int Y;

        public LeftEdge(int x, int y)
        {
            X = x;
            Y = y;
        }

        public IntPoint SquareLeft() { return new IntPoint(X - 1, Y); }
        public IntPoint SquareRight() { return new IntPoint(X, Y); }
        public Corner CornerUp() { return new Corner(X, Y + 1); }
        public Corner CornerDown() { return new Corner(X, Y); }
        public LeftEdge DownDown() { return new LeftEdge(X, Y - 1); }
        public BottomEdge DownLeft() { return new BottomEdge(X - 1, Y); }
        public BottomEdge DownRight() { return new BottomEdge(X, Y); }
        public LeftEdge UpUp() { return new LeftEdge(X, Y + 1); }
        public BottomEdge UpLeft() { return new BottomEdge(X - 1, Y + 1); }
        public BottomEdge UpRight() { return new BottomEdge(X, Y + 1); }



    }
}