namespace WinUia.Examples.Winforms;

public partial class DialogForm : Form
{
    public DialogForm()
    {
        InitializeComponent();
    }

    public string InputText => txtDialogInput.Text;
}
