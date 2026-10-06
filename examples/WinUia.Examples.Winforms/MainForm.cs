namespace WinUia.Examples.Winforms;

public partial class MainForm : Form
{
    private int _generation = 1;

    public MainForm()
    {
        InitializeComponent();
    }

    // ReSharper disable once LocalizableElement (the tests read this text)
    private void btnClick_Click(object? sender, EventArgs e) => lblResult.Text = "Clicked";

    private void btnOpenDialog_Click(object? sender, EventArgs e)
    {
        using var dialog = new DialogForm();
        var result = dialog.ShowDialog(this);
        lblResult.Text = result == DialogResult.OK ? $"Dialog: OK ({dialog.InputText})" : $"Dialog: {result}";
    }

    private void btnRecreate_Click(object? sender, EventArgs e)
    {
        var old = btnVolatile;
        btnVolatile = Recreate(old, $"Volatile {++_generation}");
        Controls.Remove(old);
        old.Dispose();
        Controls.Add(btnVolatile);
    }

    // Reversed with new window handles, so index-based re-resolution would pick the wrong button.
    private void btnReverse_Click(object? sender, EventArgs e)
    {
        var old = pnlItems.Controls.Cast<Button>().ToArray();
        var reversed = old.Reverse().Select(b => Recreate(b, b.Text)).ToArray<Control>();

        pnlItems.SuspendLayout();
        foreach (var button in old)
        {
            pnlItems.Controls.Remove(button);
            button.Dispose();
        }

        pnlItems.Controls.AddRange(reversed);
        pnlItems.ResumeLayout();
    }

    private static Button Recreate(Button original, string text) => new()
    {
        Name = original.Name,
        Text = text,
        Cursor = original.Cursor,
        Location = original.Location,
        Size = original.Size,
        UseVisualStyleBackColor = true,
    };
}
