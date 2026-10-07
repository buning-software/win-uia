namespace WinUia.TestApp;

internal sealed class EmptyForm : Form
{
    public EmptyForm()
    {
        Text = "WinUia Test App";
        ClientSize = new Size(300, 150);
    }
}
