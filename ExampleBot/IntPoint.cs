namespace SC2Sharp
{
    internal class IntPoint
    {
        public int X;
        public int Y;

        public IntPoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        public IntPoint(SC2APIProtocol.Point2D point)
        {
            X = (int)point.X;
            Y = (int)point.Y;
        }

        public BottomEdge GetBottomEdge() {return new BottomEdge(X, Y);}
        public BottomEdge GetTopEdge() {return new BottomEdge(X, Y+1);}
        public LeftEdge GetLeftEdge() {return new LeftEdge(X, Y);}
        public LeftEdge GetRightEdge() {return new LeftEdge(X+1, Y);}
        public Corner GetBottomLeftCorner() {return new Corner(X, Y);}
        public Corner GetBottomRightCorner() {return new Corner(X+1, Y);}
        public Corner GetTopLeftCorner() {return new Corner(X, Y + 1);}
        public Corner GetTopRightCorner() {return new Corner(X + 1, Y + 1);}

    }
}