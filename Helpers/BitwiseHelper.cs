using System;

namespace Waybound.Helpers;

public static class BitwiseHelper
{
    public static int ReadBits(int data, int from, int size)
    {
        if (from < 0 || size < 0 || from + size > 32)
            throw new ArgumentOutOfRangeException();

        if (size == 0) return 0;
        
        uint uData = (uint)data;
        uint mask = size == 32 ? uint.MaxValue : (1u << size) - 1u;
        
        return (int)((uData >> from) & mask);
    }

    public static int WriteBits(int data, int from, int size, int bits)
    {
        if (from < 0 || size < 0 || from + size > 32)
            throw new ArgumentOutOfRangeException();

        if (size == 0) return data;
        
        uint mask = size == 32 ? uint.MaxValue : (1u << size) - 1u;
        uint uData = (uint)data;
        uint uBits = (uint)bits;
        
        uData &= ~(mask << from);
        uData |= (uBits & mask) << from; 
        
        return (int)uData;
    }
}