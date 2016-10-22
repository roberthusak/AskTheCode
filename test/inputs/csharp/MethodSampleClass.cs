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

    public static int IntAddNested(int a, int b, int c)
    {
        a = b + c + c;
        int d = b;
        b = c + IntAdd(a, b);
        a = b = c + IntAdd(a, b);
        d = d + d;
        int e = b, f;
        e = d;

        return IntAdd(a + c, b) + c;
    }

    public static int IntOperationsExample(int a, int b, int c)
    {
        int d = a + b;
        d = -a;
        d = -(a + c);
        d = a - c;
        d = a / c;
        d = a % c;
        d = a * c;

        d = a * b + c / 2;
        d = a * (b + c) / 2;
        return d;
    }

    public static bool BoolOperationsExample(bool a, bool b, bool c)
    {
        bool d = (a && b);
        d = (a & b);
        d = (a || b);
        d = (a | b);
        d = (a == b);
        d = (a != b);
        d = !c;

        d = a & b | c;
        d = a && b || c;
        d = a && (b || c);

        return a;
    }

    public static bool IntBoolOperationsExample(int a, int b, bool c)
    {
        bool d = (a == b);
        d = (a != b);
        d = (a > b);
        d = (a < b);
        d = (a >= b);
        d = (a <= b);

        d = ((a > b) || c);

        if ((a > b) && (a != b || c))
        {
            return c;
        }

        return !c;
    }

    public static int LogicExample(int a, int b, int c, bool condition)
    {
        if ((a == 0 || 10 / b > 3) && (c == 3 || (5 / b > 1 && condition)))
        {
            return a;
        }
        else if (condition == false)
        {
            return b;
        }
        else
        {
            return c;
        }
    }

    public static int QuadraticFirstRoot(int a, int b, int c)
    {
        int D = b * b - 4 * a * c;
        int dividend = 0;

        if (D > 0)
        {
            dividend = -b;
            dividend += (int)Math.Sqrt(D);
        }
        else if (D == 0)
        {
            dividend = -b;
        }
        else
        {
            Debug.Assert(D < 0);
            Trace.WriteLine(-1);
            throw new ArgumentException();
        }

        int divisor = (2 * a);
        Debug.Assert(divisor != 0);
        return dividend / divisor;
    }

    public static int ComplexExample(int a, int b, bool c)
    {
        if (a == 0)
        {
            return -b;
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

    public static bool IsNiceNumber(int i)
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

    public static void DataIndependentCycle(int x)
    {
        int i = 0;
        while (i < 20)
        {
            x = 2 * x;
            i = i + 1;
        }

        Debug.Assert(x < 32);
    }

    public static void TriangleClassificationInlined(int a, int b, int c)
    {
        int result;

        // After a quick confirmation that it's a legal triangle, detect any sides of equal length
        if (a <= 0 || b <= 0 || c <= 0)
        {
            result = 4;
        }
        else
        {
            result = 0;

            if (a == b)
            {
                /* result = result + 1 in original */
                //result = a + 1;
                result = result + 1;
            }

            /* (a == c) in original */
            //if (a >= c)
            if (a == c)
            {
                result = result + 2;
            }

            if (b == c)
            {
                result = result + 3;
            }

            if (result == 0)
            {
                // Confirm it's a legal triangle before declaring it to be scalene
                if (a + b <= c || b + c <= a || a + c <= b)
                {
                    result = 4;
                }
                else
                {
                    result = 1;
                }
            }
            else
            {
                // Confirm it's a legal triangle before declaring it to be isosceles or equilateral
                if (result > 3)
                {
                    /* result = 3 in original */
                    //result = 0;
                    result = 3;
                    }
                else if (result == 1 && a + b > c)
                {
                    result = 2;
                }
                /* (result == 2 && a + c > b) in original */
                //else if (result == 3 && a + c > b)
                else if (result == 2 && a + c > b)
                {
                    result = 2;
                }
                /* (result == 3 && b + c > a) in original */
                //else if (result != 3 && b + c > a)
                else if (result == 3 && b + c > a)
                {
                    result = 2;
                }
                else
                {
                    result = 4;
                }
            }
        }

        bool validTriangle = (a <= 0 || b <= 0 || c <= 0
                || a <= b + c || b <= a + c || c <= a + b);

        bool succeeded;
        if (result == 1)
        {
            succeeded = validTriangle && (a != b && b != c && a != c);
        }
        else if (result == 2)
        {
            succeeded = validTriangle && ((a == b && a != c) || (a == c && a != b)
                || (b == c && b != a));
        }
        else if (result == 3)
        {
            succeeded = validTriangle && (a == b && b == c);
        }
        else if (result == 4)
        {
            succeeded = !validTriangle;
        }
        else
        {
            succeeded = false;
        }

        succeeded = succeeded;
        Contract.Assert(succeeded);
    }
}
