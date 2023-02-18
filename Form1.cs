using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Plutarque
{
    /// <summary>
    /// Pour porter vers Core 3.1 :
    /// https://docs.microsoft.com/fr-fr/dotnet/core/porting/
    /// </summary>
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Click += Form1_Click;
            for (char i = 'Z'; i < 500; i++)
                Utils.alph += i;
            byte[] data = new byte[20 * 1024 * 1024];
            System.Security.Cryptography.RandomNumberGenerator.Create().GetBytes(data);


            D.LoadData(data);
        }

        private void Form1_Click(object sender, EventArgs e)
        { 
        }

        private void dataView1_SelectionChanged(object sender)
        {

        }

        private void insérer10ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            byte[] avant = D.GetData();
            long endroit = D.SelectionStart;
            D.InsertZeros(D.SelectionStart, 10);
            byte[] après = D.GetData();
            for (long i = 0; i < endroit; i++)
            {
                if (avant[i] != après[i])
                {
                    throw new Exception("Test failed before");
                }
            }

            for (long i = endroit; i < endroit + 10; i++)
            {
                if (après[i] != 0) throw new Exception("Test failed before");
            }

            for (long i = endroit; i < avant.Length; i++)
            {
                if (après[i + 10] != avant[i]) throw new Exception("Test failed before");
            }
        }

        private void collerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            D.Paste();
        }

        private void copierToolStripMenuItem_Click(object sender, EventArgs e)
        {
            D.Copy();
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void annulerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            D.Undo();
        }

        private void répéterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            D.Redo();
        }

        private void insérerAuHasardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            byte[] avant = D.GetData();
            long endroit = D.SelectionStart;
            byte[] insertion = new byte[25 * 1024 * 1024];
            byte[] toi = new byte[] { 1, 5, 85, 63, 25, 48, 9, 5, 4, 8, 9, 0, 0, 1, 56, 6, 7 };
            Array.Copy(toi, 0,
                insertion, 12 * 1024 * 1024, toi.Length);
            D.InsertArray(insertion, D.SelectionStart);
            byte[] après = D.GetData();
            for (long i = 0; i < endroit; i++)
            {
                if (avant[i] != après[i])
                {
                    throw new Exception("Test failed before");
                }
            }

            for (long i = endroit; i < endroit + insertion.Length; i++)
            {
                if (après[i] != insertion[i - endroit]) throw new Exception("Test failed before");
            }

            for (long i = endroit; i < avant.Length; i++)
            {
                if (après[i + insertion.Length] != avant[i]) throw new Exception("Test failed before");
            }
        }

        string fn;
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog o = new OpenFileDialog()
            {
                Filter = "Tous les fichiers|*.*"
            };
            if (o.ShowDialog(this) == DialogResult.OK)
            {
                D.LoadData(File.ReadAllBytes(o.FileName));
                fn = o.FileName;
                label1.Text = fn;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(fn))
                File.WriteAllBytes(fn, D.GetData());
        }

        private void tToolStripMenuItem_Click(object sender, EventArgs e)
        {
            D.GetSelectionRange(out long a, out long b);
            byte[] dataA = D.GetData();

            const int V = +2 * 1024 * 1024;
            D.MoveChunk(a, b - a + 1, a + V);

            byte[] dataB = D.GetData();

            for (long i = a; i < b + 1; i++)
            {
                if (dataA[i] != dataB[i + V])
                    MessageBox.Show("Test failed");
            }
        }

        private void testDeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            D.GetSelectionRange(out long a, out long b);
            byte[] dataA = D.GetData();

            long l = b - a + 1;
            D.Delete(a, l);
            byte[] dataB = D.GetData();

            for (long i = b + 1; i < dataA.Length; i++)
            {
                if (dataA[i] != dataB[i - l])
                    MessageBox.Show("Test failed 1");
            }

            if (D.DataLength != dataA.Length - l)
                MessageBox.Show("Test failed 2");
        }
    }
}
