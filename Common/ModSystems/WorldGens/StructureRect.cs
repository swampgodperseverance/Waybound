using System;

namespace Waybound.Common.ModSystems.WorldGens;

public readonly struct StructureRect(int x, int y, int width, int height)
{
    public int X { get; } = x;
    public int Y { get; } = y;
    public int Width { get; } = width;
    public int Height { get; } = height;



    public StructureRect WithPadding(int padding)
    {
        return new StructureRect(X - padding, Y - padding, Width + padding * 2, Height + padding * 2);
    }

    public bool Contains(int pointX, int pointY)
    {
        return pointX >= X && pointX < X + Width &&
               pointY >= Y && pointY < Y + Height;
    }


    public bool IntersectsWith(StructureRect other)
    {
        return IntersectsWith(other.X, other.Y, other.Width, other.Height);
    }

    public bool IntersectsWith(int startX, int startY, int w, int h)
    {
        if (Width <= 0 || Height <= 0 || w <= 0 || h <= 0)
            return false;

        return startX < X + Width &&
               startX + w > X &&
               startY < Y + Height &&
               startY + h > Y;
    }

    public (int X, int Y) GetPushOutOffset(StructureRect other)
    {
        return GetPushOutOffset(other.X, other.Y, other.Width, other.Height);
    }

    public (int X, int Y) GetPushOutOffset(int startX, int startY, int w, int h)
    {
        int pushRight = (X + Width) - startX;
        int pushLeft = X - (startX + w);

        int pushDown = (Y + Height) - startY;
        int pushUp = Y - (startY + h);

        if (pushRight <= 0 || pushLeft >= 0 || pushDown <= 0 || pushUp >= 0)
        {
            return (0, 0);
        }

        int minX = pushRight < -pushLeft ? pushRight : pushLeft;
        int minY = pushDown < -pushUp ? pushDown : pushUp;

        if (Math.Abs(minX) <= Math.Abs(minY))
        {
            return (minX, 0);
        }
        else
        {
            return (0, minY);
        }
    }
}