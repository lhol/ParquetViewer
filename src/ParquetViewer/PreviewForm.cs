using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ParquetViewer
{
    public partial class PreviewForm : Form
    {
        public PreviewForm()
        {
            InitializeComponent();
            this.reloadPicture();
        }

        private void reloadPicture()
        {
            if (Clipboard.ContainsText())
            {
                try
                {
                    string[] a = Clipboard.GetText().Split("\"");
                    byte[] b = Convert.FromBase64String(a[3]);
                    using (MemoryStream ms = new MemoryStream(b))
                    {
                        Image i = Image.FromStream(ms);
                        Clipboard.SetImage(i);
                    }
                }
                catch (Exception x)
                {

                }
            }
            if (Clipboard.ContainsImage())
            {
                pictureBox1.BackgroundImage = Clipboard.GetImage();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            reloadPicture();
        }
    }
}
