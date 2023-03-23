using System;
using System.Collections.Generic;
using System.Text;

namespace SC2Sharp
{
    internal class BottomEdge
    {
        public int X;
        public int Y;

        public BottomEdge(int x, int y)
        {
            X = x;
            Y = y;
        }

        public IntPoint SquareUp() { return new IntPoint(X, Y); }
        public IntPoint SquareDown() { return new IntPoint(X, Y - 1); }
        public Corner CornerLeft() { return new Corner(X, Y); }
        public Corner CornerRight() { return new Corner(X, Y + 1); }
        public BottomEdge LeftLeft() { return new BottomEdge(X - 1, Y); }
        public LeftEdge LeftUp() { return new LeftEdge(X, Y); }
        public LeftEdge LeftDown() { return new LeftEdge(X, Y - 1); }
        public BottomEdge RightRight() { return new BottomEdge(X + 1, Y); }
        public LeftEdge RightUp() { return new LeftEdge(X + 1, Y); }
        public LeftEdge RightDown() { return new LeftEdge(X + 1, Y - 1); }
    }


}
