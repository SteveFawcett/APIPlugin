using APIPlugin.Properties;
using BroadcastPluginSDK.Interfaces;

namespace APIPlugin.Forms
{
    public delegate void WriteMessageDelegate(string message);
    public partial class InfoPanel : UserControl , IInfoPage
    {
        public InfoPanel()
        {
            InitializeComponent();

            pictureBox1.Image = Resources.green;
        }

        public Control GetControl()
        {
            return this;
        }

        public void WriteMessage(string message)
        {
            if ( listBox1.InvokeRequired)
            {
                listBox1.BeginInvoke(new Action(() => WriteMessage(message)));
                return;
            }

            if (InvokeRequired)
            {
                Invoke(new Action(() => WriteMessage(message)));
                return;
            }
            listBox1.Items.Add(message);
        }
    }
}
