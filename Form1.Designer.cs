
namespace Plutarque
{
    partial class Form1
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

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.insérer10ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.collerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copierToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.annulerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.répéterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insérerAuHasardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.testDeleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.D = new Plutarque.DataView();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.insérer10ToolStripMenuItem,
            this.collerToolStripMenuItem,
            this.copierToolStripMenuItem,
            this.annulerToolStripMenuItem,
            this.répéterToolStripMenuItem,
            this.insérerAuHasardToolStripMenuItem,
            this.tToolStripMenuItem,
            this.testDeleteToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(211, 224);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // insérer10ToolStripMenuItem
            // 
            this.insérer10ToolStripMenuItem.Name = "insérer10ToolStripMenuItem";
            this.insérer10ToolStripMenuItem.Size = new System.Drawing.Size(210, 24);
            this.insérer10ToolStripMenuItem.Text = "Insérer 10";
            this.insérer10ToolStripMenuItem.Click += new System.EventHandler(this.insérer10ToolStripMenuItem_Click);
            // 
            // collerToolStripMenuItem
            // 
            this.collerToolStripMenuItem.Name = "collerToolStripMenuItem";
            this.collerToolStripMenuItem.Size = new System.Drawing.Size(210, 24);
            this.collerToolStripMenuItem.Text = "Coller";
            this.collerToolStripMenuItem.Click += new System.EventHandler(this.collerToolStripMenuItem_Click);
            // 
            // copierToolStripMenuItem
            // 
            this.copierToolStripMenuItem.Name = "copierToolStripMenuItem";
            this.copierToolStripMenuItem.Size = new System.Drawing.Size(210, 24);
            this.copierToolStripMenuItem.Text = "Copier";
            this.copierToolStripMenuItem.Click += new System.EventHandler(this.copierToolStripMenuItem_Click);
            // 
            // annulerToolStripMenuItem
            // 
            this.annulerToolStripMenuItem.Name = "annulerToolStripMenuItem";
            this.annulerToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            this.annulerToolStripMenuItem.Size = new System.Drawing.Size(210, 24);
            this.annulerToolStripMenuItem.Text = "Annuler";
            this.annulerToolStripMenuItem.Click += new System.EventHandler(this.annulerToolStripMenuItem_Click);
            // 
            // répéterToolStripMenuItem
            // 
            this.répéterToolStripMenuItem.Name = "répéterToolStripMenuItem";
            this.répéterToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
            this.répéterToolStripMenuItem.Size = new System.Drawing.Size(210, 24);
            this.répéterToolStripMenuItem.Text = "répéter";
            this.répéterToolStripMenuItem.Click += new System.EventHandler(this.répéterToolStripMenuItem_Click);
            // 
            // insérerAuHasardToolStripMenuItem
            // 
            this.insérerAuHasardToolStripMenuItem.Name = "insérerAuHasardToolStripMenuItem";
            this.insérerAuHasardToolStripMenuItem.Size = new System.Drawing.Size(210, 24);
            this.insérerAuHasardToolStripMenuItem.Text = "Insérer au hasard";
            this.insérerAuHasardToolStripMenuItem.Click += new System.EventHandler(this.insérerAuHasardToolStripMenuItem_Click);
            // 
            // tToolStripMenuItem
            // 
            this.tToolStripMenuItem.Name = "tToolStripMenuItem";
            this.tToolStripMenuItem.Size = new System.Drawing.Size(210, 24);
            this.tToolStripMenuItem.Text = "TestMoveChunk";
            this.tToolStripMenuItem.Click += new System.EventHandler(this.tToolStripMenuItem_Click);
            // 
            // testDeleteToolStripMenuItem
            // 
            this.testDeleteToolStripMenuItem.Name = "testDeleteToolStripMenuItem";
            this.testDeleteToolStripMenuItem.Size = new System.Drawing.Size(210, 24);
            this.testDeleteToolStripMenuItem.Text = "TestDelete";
            this.testDeleteToolStripMenuItem.Click += new System.EventHandler(this.testDeleteToolStripMenuItem_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(74, 48);
            this.button1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 29);
            this.button1.TabIndex = 1;
            this.button1.Text = "Ouvr";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(205, 48);
            this.button2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 29);
            this.button2.TabIndex = 1;
            this.button2.Text = "Enr";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(389, 52);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 20);
            this.label1.TabIndex = 2;
            this.label1.Text = "label1";
            // 
            // D
            // 
            this.D.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.D.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.D.ContextMenuStrip = this.contextMenuStrip1;
            this.D.Font = new System.Drawing.Font("Consolas", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.D.Location = new System.Drawing.Point(0, 102);
            this.D.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.D.MaxSupportedArrayLength = ((long)(214748364));
            this.D.Name = "D";
            this.D.ReadOnly = false;
            this.D.ReturnMode = Plutarque.DataView.ReturnModeConvention.crlf;
            this.D.SelectionLength = 0;
            this.D.SelectionStart = ((long)(0));
            this.D.Size = new System.Drawing.Size(1237, 539);
            this.D.TabIndex = 0;
            this.D.SelectionChanged += new Plutarque.DataView.EventHandler(this.dataView1_SelectionChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1237, 641);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.D);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "Form1";
            this.Text = "Éditeur";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DataView D;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem insérer10ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem collerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copierToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem annulerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem répéterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem insérerAuHasardToolStripMenuItem;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripMenuItem tToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem testDeleteToolStripMenuItem;
    }
}

