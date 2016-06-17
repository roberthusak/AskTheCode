using System;
using System.Diagnostics;

public static class MethodSampleClass
{
    public static void Trivial()
    {
    }

    public static int IntAdd(int a, int b)
    {
        return a + b;
    }

    public static int IntMax(int a, int b)
    {
        if (a > b)
        {
            return a;
        }
        else
        {
            return b;
        }
    }

    public static int ComplexExample(int a, int b, bool c)
    {
        if (a == 0)
        {
            return -1;
        }

        if (IsNiceNumber(a) && a / b > 2 && b != -1)
        {
            int d = b;
            while (a < b)
            {
                a += a;
                Debug.Assert(a != 0);
            }
        }
        else
        {
            a += b;
        }

        return a;
    }

    private static bool IsNiceNumber(int i)
    {
        if (i % 2 == 0)
        {
            return true;
        }
        else if (i % 3 == 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
