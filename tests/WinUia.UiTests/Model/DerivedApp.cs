namespace WinUia.UiTests.Model;

public sealed class DerivedApp : App
{
    public string Title => MainWindow.Name;
}
