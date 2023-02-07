namespace Almostengr.VideoProcessor.Core.Constants;

public struct Opacity
{
    public string Value { get; }
    public Opacity(string value)
    {
        Value = value;
    }

    public static readonly Opacity Full = new Opacity("1.0");
    public static readonly Opacity Heavy = new Opacity("0.8");
    public static readonly Opacity Medium = new Opacity("0.5");
    public static readonly Opacity Light = new Opacity("0.3");
    public static readonly Opacity None = new Opacity("0.0");

    public override string ToString()
    {
        return Value.ToString();
    }
}
