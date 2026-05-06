using System;
using System.Windows.Forms;

namespace DynaAppX
{
    public class LoadingDialog : Form
    {
        private readonly Label _messageLabel;
        private readonly ProgressBar _progressBar;

        public LoadingDialog(string message)
        {
            _messageLabel = new Label
            {
                Text = message,
                AutoSize = true,
                Location = new System.Drawing.Point(20, 20)
            };

            _progressBar = new ProgressBar
            {
                Style = ProgressBarStyle.Marquee,
                Location = new System.Drawing.Point(20, 50),
                Size = new System.Drawing.Size(260, 20),
                MarqueeAnimationSpeed = 30
            };

            ClientSize = new System.Drawing.Size(300, 90);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            ControlBox = false;
            Text = "Loading...";

            Controls.Add(_messageLabel);
            Controls.Add(_progressBar);
        }

        public void UpdateMessage(string message)
        {
            _messageLabel.Text = message;
        }
    }
}