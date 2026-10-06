namespace WinUia.TestApp;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        var form = new EmptyForm();

        if (args.Contains("--ignore-close"))
            form.FormClosing += (_, e) => e.Cancel = true;

        Application.Run(form);
    }
}
