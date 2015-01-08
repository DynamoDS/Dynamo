using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class HogManager
{
    private static int hogID = 0;

    public static int GetNextUnusedID()
    {
        int next = hogID;
        hogID++;
        return next;
    }

}

public class Hog
{

    public double X { get; set; }
    public double Y { get; set; }

    public int ID { get; private set; }

    private Hog(double x, double y) : this(x,y, HogManager.GetNextUnusedID())
    {
    }

    private Hog(double x, double y, int id)
    {
        this.X = x;
        this.Y = y;
        this.ID = id;
    }

    public static Hog ByPoint(double x, double y)
    {
        return  new Hog(x,y);
    }

    public override string ToString()
    {
        return String.Format("{0}: ({1}, {2})", ID, X, Y);
    }

}
