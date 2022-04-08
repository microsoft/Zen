namespace ZenLib.Generators.Tests;

using ZenLib;

partial class Program
{
    static void Main(string[] args)
    {
        var x = Zen.Symbolic<Foo>();
        x.GetId();
        x.GetBar();
        x.WithBar(Zen.Symbolic<int>());
        x.WithId(Zen.Symbolic<Real>());

        var y = Zen.Symbolic<Fat<int>>();
        y.GetField();
        y.WithField(0);

        var z = Zen.Symbolic<Bar>();
        z.GetBlah();
        z.WithBlah(0);
    }
}

[ZenObject]
public class Point
{
    public int X { get; set; }

    public int Y { get; set; }

    public static Zen<Point> Add(Zen<Point> p1, Zen<Point> p2)
    {
        return p1.WithX(p1.GetX() + p2.GetX()).WithY(p1.GetY() + p2.GetY());
    }
}

[ZenObject]
public class Foo
{
    public Real Id { get; set; }

    public int Bar { get; set; }
}

[ZenObject]
public class Bar
{
    public uint Blah;
}

[ZenObject]
public struct Fat<T>
{
    public T Field { get; set; }
}