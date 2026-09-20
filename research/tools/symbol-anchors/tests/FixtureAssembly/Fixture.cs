namespace Fixture;

public sealed class FakeOwner
{
    public void Alpha()
    {
    }

    public bool Beta(int value, string text)
    {
        return value > 0 && text.Length > 0;
    }

    public int Value { get; set; }
}
