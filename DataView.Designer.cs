
namespace Plutarque
{
    partial class DataView
    {
        /// <summary> 
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur de composants

        /// <summary> 
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas 
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.scrollBar = new System.Windows.Forms.VScrollBar();
            this.mainView = new Plutarque.TextZone();
            this.Tip = new System.Windows.Forms.ToolTip(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.scrollBar, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.mainView, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(996, 598);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // scrollBar
            // 
            this.scrollBar.Dock = System.Windows.Forms.DockStyle.Right;
            this.scrollBar.LargeChange = 5;
            this.scrollBar.Location = new System.Drawing.Point(975, 0);
            this.scrollBar.Maximum = 200;
            this.scrollBar.Name = "scrollBar";
            this.scrollBar.Size = new System.Drawing.Size(21, 598);
            this.scrollBar.TabIndex = 0;
            this.scrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.scrollBar_Scroll);
            // 
            // mainView
            // 
            this.mainView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainView.Location = new System.Drawing.Point(0, 0);
            this.mainView.Margin = new System.Windows.Forms.Padding(0);
            this.mainView.Name = "mainView";
            this.mainView.Size = new System.Drawing.Size(975, 598);
            this.mainView.TabIndex = 1;
            this.mainView.FontChanged += new System.EventHandler(this.mainView_FontChanged);
            this.mainView.Paint += new System.Windows.Forms.PaintEventHandler(this.mainView_Paint);
            this.mainView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.mainView_MouseClick);
            this.mainView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mainView_MouseDown);
            this.mainView.MouseLeave += new System.EventHandler(this.mainView_MouseLeave);
            this.mainView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.mainView_MouseMove);
            this.mainView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.mainView_MouseUp);
            this.mainView.Resize += new System.EventHandler(this.mainView_Resize);
            // 
            // DataView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "DataView";
            this.Size = new System.Drawing.Size(996, 598);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DataView_KeyDown);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.DataView_KeyPress);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.DataView_KeyUp);
            this.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.DataView_PreviewKeyDown);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.VScrollBar scrollBar;
        private TextZone mainView;
        private System.Windows.Forms.ToolTip Tip;
    }
}
