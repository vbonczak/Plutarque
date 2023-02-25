using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using static System.Math;
using System.Windows.Forms;
using System.Drawing.Text;

namespace Plutarque
{
    /// <summary>
    /// Idée pour l'ASCII : retour à la ligne avec 0A0D ou 0A ou 0D. Pb : nécessite de revoir le modèle de dessin et
    ///  de positionnement des blocs.
    ///  
    /// Attention la touche ECHAP est active en entrée
    /// 
    /// Fonction recherche de chaîne ou de séquence d'octets
    /// 
    /// Nodes!
    /// 
    /// ColorMap, avec max value
    /// </summary>
    public partial class DataView : UserControl
    {
        /// <summary>
        /// Constructeur
        /// </summary>
        public DataView()
        {
            InitializeComponent();
            MouseWheel += DataView_MouseWheel;
            dataStream = new MemoryStream();
            baseLeft = 16;
            BaseRight = -1;
            lineLength = 16;

            DoubleBuffered = true;

            ForeColor = SystemColors.WindowText;
            SelectionBackColor = SystemColors.Highlight;
            SelectionColor = SystemColors.HighlightText;
            OffsetBackColor = SystemColors.Control;
            OffsetColor = Color.DarkViolet;
            OffsetCurrentLineColor = Color.Violet;

            OffsetFocusColor = Color.Orange;
            BackColor = SystemColors.Window;
            ForeColor = SystemColors.WindowText;
            MiddleMarginColor = SystemColors.Control;
            MiddleMarginWidth = 10;
            CaretColor = Color.Red;
            SubCaretColor = Color.Gray;

            LoadControlCharsFont();

            history = new List<IAction>();
            HistoryLength = 50;

            OffsetBase = 16;

            maxSupportedArrayLength = int.MaxValue / 10;

            //Repères de texte
            reperes = new List<Repere>();
            renderedReperesG = new Dictionary<Rectangle, int>();
            renderedReperesD = new Dictionary<Rectangle, int>();
        }


        [EditorBrowsable(EditorBrowsableState.Never)]
        public new void ResetText()
        {
            base.ResetText();
        }

        /// <summary>
        /// Change le flux de données. Les repères de textes ne sont pas effacés.
        /// </summary>
        /// <param name="s"></param>
        public void LoadStream(Stream s)
        {
            dataStream.Close();
            dataStream = s;
        }

        /// <summary>
        /// Vide le flux actuel, ainsi que les repères de texte associés.
        /// </summary>
        public void ResetStream()
        {
            reperes.Clear();
            dataStream.Close();
            dataStream = new MemoryStream();
        }

        /// <summary>
        /// Retourne les données contenues dans le flux actif dans un tableau d'octets.
        /// </summary>
        /// <returns></returns>
        public byte[] GetData()
        {
            byte[] ret = new byte[dataStream.Length];
            lock (dataStream)
            {
                dataStream.Seek(0, SeekOrigin.Begin);
                int CHUNK_SIZE = 1024 * 1024;

                byte[] buf = new byte[CHUNK_SIZE];
                for (long i = 0; i < dataStream.Length; i += CHUNK_SIZE)
                {
                    int count = (int)Min(dataStream.Length - i, CHUNK_SIZE);
                    dataStream.Read(buf, 0, count);
                    Array.Copy(buf, 0, ret, i, count);
                }
            }
            return ret;
        }

        /// <summary>
        /// Charge un tableau d'octets (extensible par défaut), sans effacer les repères de texte.
        /// </summary>
        /// <param name="data"></param>
        public void LoadData(byte[] data)
        {
            MemoryStream m = new MemoryStream();
            m.Write(data, 0, data.Length);
            LoadStream(m);
        }

        /// <summary>
        /// Définit la valeur de la barre de défilement en se basant sur le numéro de ligne.
        /// </summary>
        /// <param name="line"></param>
        protected virtual void SetScrollFromLine(long line)
        {
            long v = GetNbOflines();
            if (v > 0)
                scrollBar.Value = (int)(line * GetMaxScroll() / v);
            else
                scrollBar.Value = 0;
        }

        /// <summary>
        /// Retourne le numéro de la première ligne affichée à l'écran d'après la barre de défilement.
        /// </summary>
        /// <returns></returns>
        protected virtual long GetLineFromScroll()
        {
            return scrollBar.Value * GetNbOflines() / GetMaxScroll();
        }



        /// <summary>
        /// Gère la longueur réelle de la barre de défilement en fonction de la valeur maximale souhaitée.
        /// </summary>
        /// <param name="maxValue"></param>
        protected void SetScrollBarLength(long maxValue)
        {
            scrollBar.Maximum = (int)Min(Max(10, maxValue + scrollBar.LargeChange - 1), 200);//200 -> trop petite

            scrollBar.Visible = maxValue > 1;
        }

        /// <summary>
        /// Retourne la dernière valeur que la barre de défilement peut atteindre lors d'une intéraction avec 
        /// l'utilisateur.
        /// </summary>
        /// <returns></returns>
        protected int GetMaxScroll()
        {
            return scrollBar.Maximum - scrollBar.LargeChange + 1;
        }



        /// <summary>
        /// Retourna la zone à la position (client) indiquée.
        /// </summary>
        /// <param name="loc"></param>
        /// <returns></returns>
        public SubZone GetSubZoneAtPoint(Point loc)
        {
            if (offsetZone.Contains(loc))
            {
                return SubZone.OffsetMargin;
            }
            else if (leftZone.Contains(loc))
            {
                return SubZone.LeftPanel;
            }
            else if (rightZone.Contains(loc))
            {
                return SubZone.RightPanel;
            }
            else if (loc.X > leftZone.Right && loc.X < rightZone.Left && loc.Y <= ClientRectangle.Bottom && loc.Y >= ClientRectangle.Top)
            {
                return SubZone.MiddleMargin;
            }
            else
            {
                return SubZone.OutsideBorders;
            }
        }

        /// <summary>
        /// La prise en compte de la valeur se fait dans la procédure de rendu.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void scrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            firstLine = GetLineFromScroll();
            Refresh();
        }

        /// <summary>
        /// Retourne le nombre optimal de degrés de la barre de défilement en fonction de la longueur
        /// des données.
        /// </summary>
        /// <returns></returns>
        protected long GetScrollTicks()
        {
            return GetNbOflines() / scrollLines;
        }

        /// <summary>
        /// Nombre total de lignes en théorie dans le flux, de largeur de l'écran.
        /// </summary>
        /// <returns></returns>
        private long GetNbOflines()
        {
            return dataStream.Length / lineLength;
        }

        /// <summary>
        /// Nombre de lignes visibles à l'écran, géométriquement
        /// </summary>
        /// <returns></returns>
        private int GetVisibleNbOfLines()
        {
            return (int)Ceiling((double)((lastOffset - firstOffset) / lineLength));//arrondi en haut
        }

        private void mainView_Resize(object sender, EventArgs e)
        {
            Refresh();
        }

        private void mainView_MouseClick(object sender, MouseEventArgs e)
        {

        }

        /// <summary>
        /// Retourne la position dans le flux à partir du point à l'écran spécifié.
        /// </summary>
        /// <param name="loc"></param>
        /// <returns></returns>
        public virtual long GetOffsetFromPoint(Point loc)
        {
            long ret = -1;
            if (lineHeight > 0)
            {
                long line = firstLine + loc.Y / lineHeight;
                SubZone z = GetSubZoneAtPoint(loc);
                switch (z)
                {
                    case SubZone.LeftPanel:
                        ret = line * lineLength + (loc.X - leftZone.Left) / blockW;
                        break;
                    case SubZone.RightPanel:
                        ret = line * lineLength + (loc.X - rightZone.Left) / blockW;
                        break;
                    case SubZone.OffsetMargin:
                        // retourner le premier indice de la ligne
                        ret = line * lineLength;
                        break;
                    default:
                        // retoruner le dernier indice de la ligne
                        ret = (line + 1) * lineLength - 1;
                        break;
                }
            }
            return Min(dataStream.Length - 1, ret);
        }



        /// <summary>
        /// Indique la visibilité de la position spécifiée (dans les bornes du flux).
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        protected bool IsOffsetVisible(long offset)
        {
            return offset <= lastOffset && offset >= firstOffset;
        }




        /// <summary>
        /// Défile jusqu'à assurer la visibilité à l'écran de la position du flux indiquée.
        /// 
        /// Si on va en haut, et qu'on saute de page, il faut que la ligne visible soit en haut (ancrée en haut), 
        /// dans l'autre cas (vers le bas), on se base sur la visibilité de la ligne ancrée en bas.
        /// </summary>
        public virtual void ScrollToCaret()
        {
            EnsureVisible(selectionStart);
        }

        /// <summary>
        /// Adapte le défilement pour que le décalage spécifié soit visible dans la zone d'édition.
        /// SI Si on va en haut, et qu'on saute de page, il faut que la ligne visible soit en haut (ancrée en haut), 
        /// dans l'autre cas (vers le bas), on se base sur la visibilité de la ligne ancrée en bas.
        /// </summary>
        /// <param name="offset"></param>
        public virtual void EnsureVisible(long offset)
        {
            if (offset <= lastOffset && offset >= firstOffset) return;
            offset = Min(dataStream.Length - 1, Max(0, offset));
            long formerLine = firstLine;
            firstLine = offset / lineLength; //ligne correspondant au nouveau décalage à rendre visible
            if (formerLine == firstLine) return;

            if (firstLine > formerLine)
            {
                //défilement ancré en bas (il faut que firstLine' soit visible en bas, donc firstLine' < firstLine + GetVisibleNbOfLines() )
                firstLine = Max(0, firstLine - GetVisibleNbOfLines());
            }
            //else
                //sinon par défaut sans correction, ancré en haut (naturel)

            SetScrollFromLine(firstLine);
            Refresh();
        }




        /// <summary>
        /// Retourne le numéro de ligne correspondant à la position dans le flux spécifiée.
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public virtual long GetLineByOffset(long offset)
        {
            return offset / lineLength;
        }

        /// <summary>
        /// Écriture d'un octet dans la mémoire tampon des données.
        /// Cette fonction prend en charge l'historique.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="offset"></param>
        protected void WriteByte(byte b, long offset)
        {
            if (ReadOnly) return;
            dataStream.Seek(offset, SeekOrigin.Begin);

            if (offset >= dataStream.Length)
            {
                //Ajout de données
                InsertZeros(offset, 1);
            }

            byte old = (byte)dataStream.ReadByte();//pas à la fin du flux dans cette branche
            AddAction(new ByteWrittenAction(old, b, offset));
            dataStream.Seek(-1, SeekOrigin.Current);
            dataStream.WriteByte(b);
        }

        /// <summary>
        /// Remplace les données à partir du décalage spécifié par le tableau data.
        /// Fonction sans prise en charge de l'historique.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        public void WriteBytes(byte[] data, long offset)
        {
            lock (dataStream)
            {
                dataStream.Seek(offset, SeekOrigin.Begin);
                dataStream.Write(data, 0, data.Length);
            }
            Refresh();
        }

        /// <summary>
        /// Insère le nombre de zéros spécifié par size à partir de l'offset précisé.
        /// Si ce décalage est situé après le dernier indice du flux, des zéros sont insérés avant ce décalage
        /// pour faire le raccord entre la fin du flux et offset.
        /// </summary>
        /// <param name="offset">Décalage à partir duquel les zéros commencent.</param>
        /// <param name="size"></param>
        public void InsertZeros(long offset, int size)
        {
            InsertArray(new byte[size], offset);
        }

        /// <summary>
        /// Ajoute un nombre potentiellement grand de zéros à la fin du flux.
        /// </summary>
        /// <param name="size">Nombre de zéros à ajouter</param>
        protected void ExtendStream(long size)
        {
            int CHUNK_SIZE = 1024 * 1024;
            byte[] buf = new byte[CHUNK_SIZE];
            long l = DataLength + size;
            for (long i = DataLength; i < l; i += CHUNK_SIZE)
            {
                lock (dataStream)
                {
                    dataStream.Seek(i, SeekOrigin.Begin); //curseur au début du bout actuel
                    dataStream.Write(buf, 0, (int)Min(CHUNK_SIZE, l - i));//écriture du tampon à cet endroit.
                }
            }
        }

        /// <summary>
        /// Insère un tableau de données de taille raisonnable (codée sur un entier 32 bits),
        /// en déplaçant les octets après si besoin.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        public void InsertArray(byte[] array, long offset)
        {
            int size = array.Length;
            long start = Min(dataStream.Length, offset);
            int realSize = (int)(size + offset - start);

            //Déplacer les octets venant ensuite si nous sommes avant la fin ;
            if (offset < dataStream.Length)
            {
                int CHUNK_SIZE = 1024 * 1024;
                long n = dataStream.Length;//fin du morceau actuel (exclu)

                byte[] buf = new byte[CHUNK_SIZE];//tampon 

                //j est le début du morceau actuel
                for (long j = n - CHUNK_SIZE; n > offset; j -= CHUNK_SIZE)
                {
                    long deb = Max(offset, j);//si j est avant, cela signifie qu'on a trop pris, on prend donc pile offset.
                    lock (dataStream)
                    {
                        //déplacement du bloc de longueur CHUNK_SIZE si j est encore à droite
                        //de offset, sinon n - offset
                        dataStream.Seek(deb, SeekOrigin.Begin);
                        dataStream.Read(buf, 0, (int)(n - deb));
                        dataStream.Seek(deb + size, SeekOrigin.Begin);
                        dataStream.Write(buf, 0, (int)(n - deb));
                    }
                    n = deb;//n devient la nouvelle borne sup (exclue)
                }
            }

            lock (dataStream)
            {
                dataStream.Seek(start, SeekOrigin.Begin);
                if (realSize > size)//combler avec des zéros
                    dataStream.Write(new byte[realSize - size], 0, realSize - size);
                dataStream.Write(array, 0, size);
            }

            AddAction(new ArrayInsertedAction(array, offset));
        }

        /// <summary>
        /// Copie un sous-tableau d'octets dans le flux à une autre position possiblement extérieure au flux.
        /// Cette fonction prend en charge l'historique.
        /// </summary>
        /// <param name="sourceOffset">Décalage dans le flux actuel où commencer la copie</param>
        /// <param name="length">Longueur du morceau</param>
        /// <param name="destinationOffset">Décalage cible</param>
        public void MoveChunk(long sourceOffset, long length, long destinationOffset)
        {
            if (sourceOffset < 0 || length < 0 || destinationOffset < 0)
                throw new ArgumentException("Arguments invalides.");
            if (sourceOffset + length > DataLength)
                throw new ArgumentException("La zone à déplacer est hors-limites.");

            byte[] old = null;
            if (length < MaxSupportedArrayLength)
            {
                old = new byte[length];
                lock (dataStream)
                {
                    dataStream.Seek(sourceOffset, SeekOrigin.Begin);
                    dataStream.Read(old, 0, (int)length);
                }
            }

            int CHUNK_SIZE = 1024 * 1024;
            long décalage = destinationOffset - sourceOffset;
            byte[] buf = new byte[CHUNK_SIZE];//tampon 
            if (destinationOffset > sourceOffset)
            {
                //Déplacer en commençant par la fin : il faut augmenter le flux avant
                long n = sourceOffset + length;//fin du morceau actuel (exclu)
                if (destinationOffset + length > DataLength)
                    ExtendStream(destinationOffset + length - DataLength);

                //j est le début du morceau actuel
                for (long j = n - CHUNK_SIZE; n > sourceOffset; j -= CHUNK_SIZE)
                {
                    //invariant  n := fin de ce que l'on copie (exclu)
                    long deb = Max(sourceOffset, j);//si j est avant, cela signifie qu'on a trop pris, on prend donc pile offset.
                    lock (dataStream)
                    {
                        //déplacement du bloc de longueur CHUNK_SIZE si j est encore à droite
                        //de offset, sinon n - offset
                        dataStream.Seek(deb, SeekOrigin.Begin); //curseur au début du bout actuel
                        dataStream.Read(buf, 0, (int)(n - deb));//remplissage du tampon
                        dataStream.Seek(deb + décalage, SeekOrigin.Begin);//curseur sur la destination
                        dataStream.Write(buf, 0, (int)(n - deb));//écriture du tampon à cet endroit.
                    }
                    n = deb;//n devient la nouvelle borne sup (exclue)
                }
            }
            else if (destinationOffset < sourceOffset)
            {
                //Déplacer à partir du début
                //j : point de départ du bout en cours.
                for (long j = sourceOffset; j < sourceOffset + length; j += CHUNK_SIZE)
                {
                    int sz = (int)Min(CHUNK_SIZE, sourceOffset + length - j);
                    lock (dataStream)
                    {
                        dataStream.Seek(j, SeekOrigin.Begin);
                        dataStream.Read(buf, 0, sz);
                        dataStream.Seek(j + décalage, SeekOrigin.Begin);
                        dataStream.Write(buf, 0, sz);
                    }
                }
            }

            //Historique
            if (old != null)
            {
                AddAction(new ArrayMovedAction(sourceOffset, destinationOffset, old));
            }
        }

        /// <summary>
        /// Supprime l'étendue de données spécifiée, en déplaçant les données
        /// suivantes à startOffset, et en tronquant le flux.
        /// Cette fonction prend en charge l'historique.
        /// </summary>
        /// <param name="startOffset"></param>
        /// <param name="length"></param>
        public void Delete(long startOffset, long length)
        {
            if (length == 0) return;
            byte[] ret = null;
            if (length < MaxSupportedArrayLength)
            {
                ret = new byte[length];
                lock (dataStream)
                {
                    dataStream.Seek(startOffset, SeekOrigin.Begin);
                    dataStream.Read(ret, 0, (int)length);
                }
            }
            bool navigating_nested = navigating;
            navigating = true; //blocage dans tous les cas de l'ajout d'une étape d'historique : Delete est atomique.
            MoveChunk(startOffset + length, DataLength - startOffset - length, startOffset);
            dataStream.SetLength(DataLength - length); //Troncature
            navigating = navigating_nested;
            if (ret != null) //Ajout après le traitement si aucune erreur auparavant pour rester cohérent.
            {
                AddAction(new ArrayDeletedAction(startOffset, ret));
            }

            selectionLength = 0;
            selectionStart = startOffset;
        }

        /// <summary>
        /// Auxiliaire : retourne l'octet à la position spécifiée dans le flux.
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        protected byte ReadByte(long offset)
        {
            dataStream.Seek(offset, SeekOrigin.Begin);
            int v = dataStream.ReadByte();
            if (v == -1)
            {
                //TODO arrivé à la fin?
                /** En fait, cette fonction est utilisée uniquement
                 * pour savoir quelle était la valeur précédente lorsqu'on
                 * entre une valeur au clavier. À ce moment-là, nous ne sommes
                 * plus à la fin car au début de KeyInBase, on insère des zéros.
                 */
            }
            return (byte)v;
        }

        /// <summary>
        /// Auxiliaire : écriture d'un caractère (possiblement multi-octets) dans le flux.
        /// </summary>
        /// <param name="c"></param>
        private void WriteChar(char c)
        {
            short v = (short)c;
            WriteByte((byte)(v & 0xff), selectionStart);
            SelectionStart++; //place le curseur dans le flux au bon endroit

            if (v > 255)
            {
                WriteByte((byte)(v >> 8), selectionStart);
                SelectionStart++;
            }
        }

        /// <summary>
        /// Mise à la puissance p de a.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        private int Power(int a, int p) => (int)Pow(a, p);

        /// <summary>
        /// Nombre de chiffres dans l'écriture en base b du nombre n.
        /// Au minimum 1 chiffre (pour écrire 0 en particulier).
        /// </summary>
        /// <param name="n"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private int GetNbofDigits(long n, int b)
        {
            switch (b)
            {
                case (int)SpecialBases.ASCII:
                    return 1;
                case (int)SpecialBases.NotSupported:
                    throw new ArgumentException("Base non supportée.");
                default:
                    return n > 0 ? (int)Floor(Log(n, b)) + 1 : 1;
            }
        }

        /// <summary>
        /// Contient les valeurs spéciales indiquant une représentation non numérique des valeurs, par 
        /// exemple avec des caractères ASCII.
        /// </summary>
        public enum SpecialBases : int
        {
            /// <summary>
            /// Valeur spéciale pour afficher des lettres unioctets
            /// </summary>
            ASCII = -1,
            /// <summary>
            /// Valeur invalide
            /// </summary>
            NotSupported = 0

        }

        /// <summary>
        /// Permet de convertir rapidement la base spéciale b en entier à placer dans BaseLeft ou Right.
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int MakeSpecialBase(SpecialBases b) => (int)b;

        private void mainView_FontChanged(object sender, EventArgs e)
        {
            LoadControlCharsFont();
        }

    }
}
