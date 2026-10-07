namespace WinUia.Examples.Winforms
{
    partial class DialogForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblDialogMessage = new Label();
            txtDialogInput = new TextBox();
            btnDialogOk = new Button();
            btnDialogCancel = new Button();
            SuspendLayout();
            //
            // lblDialogMessage
            //
            lblDialogMessage.AutoSize = true;
            lblDialogMessage.Location = new Point(12, 12);
            lblDialogMessage.Name = "lblDialogMessage";
            lblDialogMessage.Size = new Size(84, 15);
            lblDialogMessage.TabIndex = 0;
            lblDialogMessage.Text = "Enter a value:";
            //
            // txtDialogInput
            //
            txtDialogInput.Location = new Point(12, 35);
            txtDialogInput.Name = "txtDialogInput";
            txtDialogInput.Size = new Size(236, 23);
            txtDialogInput.TabIndex = 1;
            //
            // btnDialogOk
            //
            btnDialogOk.DialogResult = DialogResult.OK;
            btnDialogOk.Cursor = Cursors.Hand;
            btnDialogOk.Location = new Point(92, 70);
            btnDialogOk.Name = "btnDialogOk";
            btnDialogOk.Size = new Size(75, 23);
            btnDialogOk.TabIndex = 2;
            btnDialogOk.Text = "OK";
            btnDialogOk.UseVisualStyleBackColor = true;
            //
            // btnDialogCancel
            //
            btnDialogCancel.DialogResult = DialogResult.Cancel;
            btnDialogCancel.Cursor = Cursors.Hand;
            btnDialogCancel.Location = new Point(173, 70);
            btnDialogCancel.Name = "btnDialogCancel";
            btnDialogCancel.Size = new Size(75, 23);
            btnDialogCancel.TabIndex = 3;
            btnDialogCancel.Text = "Cancel";
            btnDialogCancel.UseVisualStyleBackColor = true;
            //
            // DialogForm
            //
            AcceptButton = btnDialogOk;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnDialogCancel;
            ClientSize = new Size(260, 105);
            Controls.Add(lblDialogMessage);
            Controls.Add(txtDialogInput);
            Controls.Add(btnDialogOk);
            Controls.Add(btnDialogCancel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "DialogForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "WinUia Dialog";
            TopMost = true;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblDialogMessage;
        private TextBox txtDialogInput;
        private Button btnDialogOk;
        private Button btnDialogCancel;
    }
}
