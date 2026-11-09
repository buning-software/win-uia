namespace WinUia.Recording.UnitTests;

internal sealed class FakeFramework : ITestFramework
{
    public string? Name { get; init; }

    public bool? Failed { get; init; }

    public bool Throw { get; init; }

    public bool Asked { get; private set; }

    public List<string> Attached { get; } = [];

    public string? CurrentTestName => Name;

    public bool? CurrentTestFailed
    {
        get
        {
            Asked = true;
            return Throw ? throw new InvalidOperationException() : Failed;
        }
    }

    public void Attach(string path, string? description) => Attached.Add(path);
}
