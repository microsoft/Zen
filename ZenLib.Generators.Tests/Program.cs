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

    private int Other;
}