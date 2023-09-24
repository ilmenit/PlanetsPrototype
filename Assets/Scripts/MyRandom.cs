using System;

// We are using MyRandom because UnityEngine.Random cannot be called from other thread
public class MyRandom : Singleton<MyRandom>
{
    protected MyRandom() : this(Environment.TickCount)
    {
    } // guarantee this will be always a singleton only - can't use the constructor!

    // Fields
    private int inext;

    private int inextp;
    private const int MBIG = 0x7fffffff;
    private const int MSEED = 0x9a4ec86;
    private const int MZ = 0;
    private int[] SeedArray;

    // Methods
    protected MyRandom(int Seed)
    {
        this.SeedArray = new int[0x38];
        int num2 = 0x9a4ec86 - Math.Abs(Seed);
        this.SeedArray[0x37] = num2;
        int num3 = 1;
        for (int i = 1; i < 0x37; i++)
        {
            int index = (0x15 * i) % 0x37;
            this.SeedArray[index] = num3;
            num3 = num2 - num3;
            if (num3 < 0)
            {
                num3 += 0x7fffffff;
            }
            num2 = this.SeedArray[index];
        }
        for (int j = 1; j < 5; j++)
        {
            for (int k = 1; k < 0x38; k++)
            {
                this.SeedArray[k] -= this.SeedArray[1 + ((k + 30) % 0x37)];
                if (this.SeedArray[k] < 0)
                {
                    this.SeedArray[k] += 0x7fffffff;
                }
            }
        }
        this.inext = 0;
        this.inextp = 0x15;
        Seed = 1;
    }

    private double GetSampleForLargeRange()
    {
        int num = this.InternalSample();
        if ((this.InternalSample() % 2) == 0)
        {
            num = -num;
        }
        double num2 = num;
        num2 += 2147483646.0;
        return (num2 / 4294967293);
    }

    private int InternalSample()
    {
        int inext = this.inext;
        int inextp = this.inextp;
        if (++inext >= 0x38)
        {
            inext = 1;
        }
        if (++inextp >= 0x38)
        {
            inextp = 1;
        }
        int num = this.SeedArray[inext] - this.SeedArray[inextp];
        if (num < 0)
        {
            num += 0x7fffffff;
        }
        this.SeedArray[inext] = num;
        this.inext = inext;
        this.inextp = inextp;
        return num;
    }

    public virtual int Next()
    {
        return this.InternalSample();
    }

    public virtual int Next(int maxValue)
    {
        if (maxValue < 0)
        {
            throw new ArgumentOutOfRangeException("maxValue");
        }
        return (int)(this.Sample() * maxValue);
    }

    public virtual int Range(int minValue, int maxValue)
    {
        if (minValue > maxValue)
        {
            throw new ArgumentOutOfRangeException();
        }
        long num = maxValue - minValue;
        if (num <= 0x7fffffffL)
        {
            return (((int)(this.Sample() * num)) + minValue);
        }
        return (((int)((long)(this.GetSampleForLargeRange() * num))) + minValue);
    }

    public virtual float Range(float minValue, float maxValue)
    {
        if (minValue > maxValue)
        {
            throw new ArgumentOutOfRangeException();
        }
        float num = maxValue - minValue;
        return (((int)(this.Sample() * num)) + minValue);
    }

    public virtual void NextBytes(byte[] buffer)
    {
        if (buffer == null)
        {
            throw new ArgumentNullException("buffer");
        }
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = (byte)(this.InternalSample() % 0x100);
        }
    }

    public virtual double NextDouble()
    {
        return this.Sample();
    }

    // [0,1)
    protected virtual double Sample()
    {
        return (this.InternalSample() * 4.6566128752457969E-10);
    }
}