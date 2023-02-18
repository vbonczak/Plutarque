using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Plutarque
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            Font = SystemFonts.MenuFont;
            modified = false;
            modifiedRep = false;
            journalErreurs = new List<Exception>();
            LoadColors();

            ManageCommandLine();
        }

        private void ManageCommandLine()
        {
            string[] argv = Environment.GetCommandLineArgs();

            if (argv.Length > 1)
            {
                LoadFile(argv[1]);
            }
        }
        #region Gestion du fichier


        /// <summary>
        /// Null si aucun fichier chargé.
        /// </summary>
        private string loadedFile;

        /// <summary>
        /// Si le fichier a été modifié dans l'éditeur.
        /// </summary>
        private bool modified;
        /// <summary>
        /// Si les repères ont été modifiés
        /// </summary>
        private bool modifiedRep;

        private void ouvrirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog o = new OpenFileDialog()
            {
                Filter = "Tous les fichiers|*.*"
            };
            if (o.ShowDialog(this) == DialogResult.OK)
            {
                LoadFile(o.FileName);
            }
        }

        /// <summary>
        /// Charge un fichier dans l'éditeur de données. 
        /// </summary>
        /// <param name="f"></param>
        private void LoadFile(string f)
        {
            //D'abord, si c'est modifié
            if (modified)
            {
                if (!CloseFile())
                    return;
            }

            try
            {
                D.LoadData(File.ReadAllBytes(f));
                //Les repères
                if (File.Exists(f + ".reperes"))
                {
                    try
                    {
                        int before = D.Reperes.Count;
                        D.LoadReperes(f + ".reperes");
                        if (before > 0)
                        {
                            if (MessageBox.Show(this, "Le fichier contient des repères de texte. Voulez-vous effacer les repères" +
                                "précédents ?", "Repères de texte", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            {
                                //On efface
                                D.Reperes.RemoveRange(0, before);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        ErrSilent(e, "Échec du chargement des repères de texte pour ce fichier.");
                        throw;
                    }
                }
                loadedFile = f;
                lblPath.Text = loadedFile;
                Text = Path.GetFileName(f) + " - Plutarque";
                modified = false;
            }
            catch (Exception e)
            {
                Err(e);
            }

        }

        /// <summary>
        /// Ferme le fichier actuel si besoin, et renvoie true si l'opération est terminée, false si l'utilisateur
        /// a choisi d'annuler, ou si une erreur s'est produite.
        /// </summary>
        /// <returns></returns>
        private bool CloseFile()
        {
            if (modified)
            {
                DialogResult d = MessageBox.Show(this,
                    "Le fichier a été modifié dans l'éditeur. Voulez-vous l'enregistrer ?",
                     "Fichier modifié", MessageBoxButtons.YesNoCancel);
                switch (d)
                {
                    case DialogResult.Cancel:
                        return false;//on n'a pas terminé.
                    case DialogResult.Yes:
                        enregistrerToolStripMenuItem.PerformClick();
                        if (modified)//non réussi
                            return false;
                        break;
                    case DialogResult.No:
                        break;
                    default:
                        break;
                }
            }
            else if (modifiedRep && loadedFile != null)
            {
                //Pas modifié, mais juste les repères
                DialogResult d = MessageBox.Show(this,
                    "Les repères de texte ont changé. Voulez-vous les enregistrer ? Le fichier en lui-même " +
                    "ne sera pas modifié.",
                     "Repères modifiés", MessageBoxButtons.YesNoCancel);
                switch (d)
                {
                    case DialogResult.Cancel:
                        return false;//on n'a pas terminé.
                    case DialogResult.Yes:
                        SaveReperes(loadedFile);
                        if (modifiedRep)//non réussi
                            return false;
                        break;
                    case DialogResult.No:
                        break;
                    default:
                        break;
                }
            }
            loadedFile = null;
            D.ResetStream();

            lblPath.Text = "";
            Text = "Plutarque";
            modified = false;
            return true;
        }

        private void enregistrerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(loadedFile))
            {
                if (SaveFile(loadedFile))
                    modified = false;
            }
            else
                enregistrerSousToolStripMenuItem.PerformClick();
        }

        /// <summary>
        /// Enregistre les données de la fenêtre dans le fichier spécifié.
        /// Renvoie false en cas de problème non ignoré par l'utilisateur.
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        private bool SaveFile(string f)
        {
            try
            {
                File.WriteAllBytes(f, D.GetData());
                SaveReperes(f);
                return true;
            }
            catch (Exception e)
            {
                return ErrYesNo(e);
            }
        }

        private void SaveReperes(string file)
        {
            if (D.Reperes.Count > 0)
            {
                try
                {
                    //enr les repères dans un fichier à côté
                    D.SaveReperes(file + ".reperes");
                    modifiedRep = false;
                }
                catch (Exception e)
                {
                    modifiedRep = !ErrYesNo(e);
                }
            }
        }

        private void quitterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void enregistrerSousToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog s = new SaveFileDialog()
            {
                OverwritePrompt = true,
                AddExtension = false,
                Filter = "Tous les fichiers|*.*"
            };

            if (s.ShowDialog(this) == DialogResult.OK)
            {
                if (SaveFile(s.FileName))
                {
                    modified = false;
                    loadedFile = s.FileName;
                }
            }
        }


        #endregion
        #region Gestion des erreurs
        List<Exception> journalErreurs;//TODO fenêtre pour le visualiser?
        private void Err(Exception e)
        {
            MessageBox.Show(this, e.Message, "Erreur", MessageBoxButtons.OK);
            journalErreurs.Add(e);
        }

        private void ErrSilent(Exception e, string substMessage = null)
        {
            lblExplications.Text = substMessage == null ? e.Message : substMessage;
            journalErreurs.Add(e);
            lblExplications.Tag = e;//Quand on double clique, les détails s'affichent.
        }

        /// <summary>
        /// Si on choisit d'ignorer et de continuer, renvoie true.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private bool ErrYesNo(Exception e)
        {
            return MessageBox.Show(this, e.Message + "\r\nContinuer la suite de l'opération ?", "Erreur", MessageBoxButtons.YesNo) == DialogResult.Yes;
        }
        #endregion


        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!CloseFile())
                e.Cancel = true;
        }

        private void D_SelectionChanged(object sender)
        {
            lblPosition.Text = D.SelectionStart.ToString("G0") + ", 0x" + D.SelectionStart.ToString("X");
        }

        private void D_DataChanged(object sender)
        {
            modified = true;
        }

        private void copierToolStripMenuItem_Click(object sender, EventArgs e)
        {
            D.Copy();
        }

        private void collerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            D.Paste();
        }

        private void okToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (int.TryParse(txtNInsert.Text, out int n))
            {
                D.InsertZeros(D.SelectionStart, n);
            }
        }

        private void supprimerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            D.Delete(D.SelectionStart, D.SelectionLength);
        }

        private void Main_DragDrop(object sender, DragEventArgs e)
        {
            string f = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
            if (!File.Exists(f))
                return;

            if (!CloseFile())
            {
                return;
            }

            LoadFile(f);
        }

        private void Main_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
                e.Effect = DragDropEffects.None;
        }

        private void annulerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            D.Undo();
        }

        private void répeterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            D.Redo();
        }

        private void éditionToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            annulerToolStripMenuItem.Enabled = D.CanUndo();
            répeterToolStripMenuItem.Enabled = D.CanRedo();
        }

        private void txtNInsert_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
                okToolStripMenuItem.PerformClick();
        }

        private void insérerUnRepèreToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void txtRepere_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
                oKRepereToolStripMenuItem.PerformClick();
        }

        private void oKRepereToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (txtRepere.TextLength > 0)
            {
                D.AddRepère(txtRepere.Text, CurColor, D.SelectionStart);
                modifiedRep = true;
            }
        }

        #region Couleurs

        /// <summary>
        /// Remplit le menu des couleurs
        /// </summary>
        private void LoadColors()
        {
            CurColor = Color.Blue;
            Color[] start = new Color[] {   Color.FromArgb(0xFF0000),
                                            Color.FromArgb(0xFFFF00),
                                            Color.FromArgb(0x00FF00),
                                            Color.FromArgb(0x00FFFF),
                                            Color.FromArgb(0x0000FF),
                                            Color.FromArgb(0xFF00FF),
                                            Color.FromArgb(0xFFFFFF)};


            int tones = 5;
            for (int j = 0; j < start.Length; j++)
            {
                for (int i = 1; i <= tones; i++)
                {
                    Color color = start[j];
                    AddColorMenuItem(Color.FromArgb(color.R * i / tones, color.G * i / tones, color.B * i / tones),
                                        i == 1);

                }
            }

            MenuItem c = new MenuItem()
            {
                Tag = Color.Transparent,
                OwnerDraw = true,
                Break = true,
                Text = ""
            };
            c.MeasureItem += C_MeasureItem;
            c.DrawItem += C_DrawItem;
            c.Click += C_CustomColorClick; ;
            colors.MenuItems.Add(c);



            customColors = new List<Color>();
        }


        private void C_CustomColorClick(object sender, EventArgs e)
        {
            int[] argb = new int[Math.Min(16, customColors.Count)];
            for (int i = 0; i < argb.Length; i++)
            {
                argb[i] = customColors[customColors.Count - 17 + i].ToArgb();
            }

            ColorDialog c = new ColorDialog() { CustomColors = argb, Color = CurColor };
            if (c.ShowDialog(this) == DialogResult.OK)
            {
                AddColorMenuItem(c.Color, colors.MenuItems.Count % 5 == 0);
            }
        }

        private Size ColorItemsSize = new Size(15, 15);
        private void AddColorMenuItem(Color clr, bool endOfLine)
        {
            MenuItem c = new MenuItem()
            {
                Tag = clr,
                OwnerDraw = true,
                Break = endOfLine,
                Text = ""
            };
            c.MeasureItem += C_MeasureItem;
            c.DrawItem += C_DrawItem;
            c.Click += C_Click;
            colors.MenuItems.Add(c);
        }

        private Color curColor;

        private void C_Click(object sender, EventArgs e)
        {
            MenuItem menu = (MenuItem)sender;
            CurColor = (Color)menu.Tag;
            foreach (MenuItem item in colors.MenuItems)
            {
                item.Checked = false;
            }
            menu.Checked = true;
        }

        private void C_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = 30;
            e.ItemWidth = 10;
        }

        private List<Color> customColors;
        private Brush selChkBr = new HatchBrush(HatchStyle.ForwardDiagonal, SystemColors.HighlightText, Color.Transparent);
        private Brush chkBr = new HatchBrush(HatchStyle.ForwardDiagonal, SystemColors.WindowText, Color.Transparent);
        private void C_DrawItem(object sender, DrawItemEventArgs e)
        {
            Color c = (Color)colors.MenuItems[e.Index].Tag;
            Rectangle r = new Rectangle(e.Bounds.Left + (e.Bounds.Width - ColorItemsSize.Width) / 2,
               e.Bounds.Top + (e.Bounds.Height - ColorItemsSize.Height) / 2, ColorItemsSize.Width, ColorItemsSize.Height);
            Brush br;
            Brush bbr;
            Pen pn;
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                pn = SystemPens.HighlightText;
                br = SystemBrushes.Highlight;
                bbr = selChkBr;
            }
            else
            {
                pn = SystemPens.Highlight;
                br = SystemBrushes.Menu;
                bbr = chkBr;
            }
            e.Graphics.FillRectangle(br, e.Bounds);
            if ((e.State & DrawItemState.Checked) == DrawItemState.Checked)
            {
                e.Graphics.FillRectangle(bbr, e.Bounds);
            }
            e.Graphics.FillRectangle(new SolidBrush(c), r);
            e.Graphics.DrawRectangle(pn, r);

        }

        #endregion

        private void couleurToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            colors.Show(D, D.PointToClient(MousePosition));
        }

        private void couleurToolStripMenuItem_Click(object sender, EventArgs e)
        {
            couleurToolStripMenuItem_DropDownOpening(sender, e);
        }

        #region Réglage des bases d'affichage

        private void hexadécimalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (menuEnCours == MenuBase.Gauche)
            {
                D.BaseLeft = 16;
            }
            else
            {
                D.BaseRight = 16;
            }
        }

        enum MenuBase
        {
            Gauche, Droite
        }
        /// <summary>
        /// Savoir de quelle partie on parle en ce moment.
        /// </summary>
        private MenuBase menuEnCours;
        private void baseDeGaucheToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            menuEnCours = MenuBase.Gauche;
        }

        private void baseDeDroiteToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            menuEnCours = MenuBase.Droite;
        }

        private void décimalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (menuEnCours == MenuBase.Gauche)
            {
                D.BaseLeft = 10;
            }
            else
            {
                D.BaseRight = 10;
            }
        }

        private void octalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (menuEnCours == MenuBase.Gauche)
            {
                D.BaseLeft = 8;
            }
            else
            {
                D.BaseRight = 8;
            }
        }

        private void binaireToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (menuEnCours == MenuBase.Gauche)
            {
                D.BaseLeft = 2;
            }
            else
            {
                D.BaseRight = 2;
            }
        }

        private void aSCIIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (menuEnCours == MenuBase.Gauche)
            {
                D.BaseLeft = DataView.MakeSpecialBase(DataView.SpecialBases.ASCII);
            }
            else
            {
                D.BaseRight = DataView.MakeSpecialBase(DataView.SpecialBases.ASCII);
            }
        }
        private bool fromAffichage = false;

        public Color CurColor
        {
            get => curColor; set
            {
                curColor = value;
                txtRepere.BackColor = value;
                txtRepere.ForeColor = Utils.GetForeColorFromBackColor(value);
            }
        }

        private void affichageToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            fromAffichage = true;
        }

        private void affichageToolStripMenuItem_DropDownClosed(object sender, EventArgs e)
        {
            fromAffichage = false;
        }
        #endregion

        private void txtRepere_Click(object sender, EventArgs e)
        {

        }


        private void supprimerLeRepèreToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (overRepere >= 0)
            {
                D.Reperes.RemoveAt(overRepere);
                modifiedRep = true;
                Refresh();
            }
        }
        /// <summary>
        /// Repère à la position d'apparition du menu contextuel
        /// </summary>
        private int overRepere;
        private void contextGeneral_Opening(object sender, CancelEventArgs e)
        {
            //S'il y a un repère
            overRepere = D.GetRepereFromPoint(D.PointToClient(MousePosition));
            supprimerLeRepèreToolStripMenuItem.Visible = overRepere >= 0;

            //Gestion pour le sous-menu Base
            if (fromAffichage) return;
            DataView.SubZone curZ = D.GetSubZoneAtPoint(D.PointToClient(MousePosition));
            if (curZ == DataView.SubZone.LeftPanel)
            {
                menuEnCours = MenuBase.Gauche;
            }
            else if (curZ == DataView.SubZone.RightPanel)
            {
                menuEnCours = MenuBase.Droite;
            }
        }

        private void àProposDePlutarqueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Plutarque, Vincent Bonczak 2020-2022.");
        }
    }
}
