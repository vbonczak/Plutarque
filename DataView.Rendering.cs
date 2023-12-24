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
    public partial class DataView
    {
        private const int bufferSz = 50 * 1024;
        private byte[] buffer;
        /// <summary>
        /// Rendu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void View_Main_Paint(object sender, PaintEventArgs e)
        {
            Rectangle r = mainView.ClientRectangle;
            Graphics g = e.Graphics;
            if (Font.Height < 15)
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
                g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
            }
            //Détermination de la largeur de la colonne d'offset
            int stringW = Max(4, (int)Ceiling(Log(dataStream.Length, offsetBase)));
            Size offsetZoneSz = g.MeasureString(new string('0', stringW), Font).ToSize();

            int charW = offsetZoneSz.Width / stringW;         // Largeur d'1 caractère
            int nOfDigitsL = GetNbofDigits(255, baseLeft);    // largeur du texte de bloc
            int nOfDigitsR = GetNbofDigits(255, baseRight);    // largeur du texte de bloc

            /*blockWidth = (int)(offsetZoneSz.Width * Max(BaseLeft < 0 ? 2 : Log(256, BaseLeft),
                BaseRight < 0 ? 2 : Log(256, BaseRight)) / stringSz);*/

            int offsetCurX = r.X;
            int leftCurX = offsetCurX + offsetZoneSz.Width;
            int textZoneWidth = (r.Width - offsetZoneSz.Width - middleMarginWidth) / 2;
            int rightCurX = leftCurX + textZoneWidth + middleMarginWidth;

            blockW = Max(nOfDigitsL, nOfDigitsR) * charW + innerMargin;


            //Adaptation de la taille de ligne en fonction de la largeur du bloc et des zones
            lineLength = textZoneWidth / blockW;

            //Nous assurons la position actuelle dans le flux
            dataStream.Seek(firstOffset = firstLine * lineLength, SeekOrigin.Begin);

            //Fond général
            g.FillRectangle(backBr, r);

            //Colonne des décalages
            g.FillRectangle(offsetBackBr, new Rectangle(r.Location,
                new Size(offsetZoneSz.Width, r.Height)));

            //Marge du milieu
            g.FillRectangle(middleMarginBr, new Rectangle(new Point(leftCurX + textZoneWidth, r.Y),
                new Size(middleMarginWidth, r.Height)));

            //Zones pour l'intéraction avec la souris
            leftZone = new Rectangle(leftCurX, r.Y, textZoneWidth, r.Height);
            rightZone = new Rectangle(rightCurX, r.Y, textZoneWidth, r.Height);
            offsetZone = new Rectangle(offsetCurX, r.Y, offsetZoneSz.Width, r.Height);

            //todo gros tampon puis parcours (besoin de calculer le total d'abord. Mettre le tampon dans la classe générale)

            //byte[] buf = new byte[lineLength];
            Rectangle blockRectL = new Rectangle(leftCurX, r.Y, blockW, offsetZoneSz.Height);
            Rectangle blockRectR = new Rectangle(rightCurX, r.Y, blockW, offsetZoneSz.Height);
            long p;//Position actuelle
                   //                  v vvvvvvvvvv  éviter d'avoir une ligne partiellement visible 

            GetSelectionRange(out long sBegin, out long sEnd);


            unsafe
            {

                //Longueur du bloc lu en cours
                int L = Min(bufferSz - bufferSz % lineLength, bufferSz);//avoir un nombre entier
                                                                        //de lignes *au max* (il se peut qu'on lise moins)

                fixed (byte* pBuf = buffer)
                {

                    int i = 0;
                    byte* line = pBuf;
                    //parcours successif des tampons pour parcourir l'ensemble du fichier dont nous avons besoin.
                    while (dataStream.Position < dataStream.Length)
                    {
                        p = dataStream.Position;
                        L = dataStream.Read(buffer, 0, bufferSz);
                        while (blockRectL.Y + lineHeight <= r.Bottom && i < L)
                        {

                            DrawOffset(g, stringW, offsetCurX, blockRectL, p, sBegin);//offset

                            int l = Min(lineLength, L - i);

                            DrawLine(g, BaseLeft, pBuf, l, blockRectL, p, textZoneWidth, sBegin, sEnd);//Partie gauche
                            DrawLine(g, BaseRight, pBuf, l, blockRectR, p, textZoneWidth, sBegin, sEnd);//Partie droite
                            blockRectL.Y += offsetZoneSz.Height;
                            blockRectR.Y += offsetZoneSz.Height;

                            line += l;
                            i += l;
                            p += l;
                        }
                    } //dataStream.Position < dataStream.Length


                }




            }


            lastOffset = dataStream.Position - 1;

            lineHeight = offsetZoneSz.Height;
            SetScrollBarLength(firstLine == 0 && dataStream.Position == dataStream.Length ? 0 : GetScrollTicks());

            //g.DrawString(offsetMouse.ToString(), Font, foreBr, PointToClient(MousePosition));
            //g.DrawString(scrollBar.Value.ToString() + " de " + scrollBar.Maximum.ToString(), Font, backBrSel, r.Location + new Size(5, 5));

            //Dessin de la sélection
            GetRectanglesFromOffset(selectionStart, out Rectangle s1, out Rectangle s2);
            if (!s1.IsEmpty)
            {
                // sous-caret
                switch (selectedZone)
                {
                    case SubZone.LeftPanel:
                        DrawCur(g, BaseLeft, offsetZoneSz, charW, nOfDigitsL, s1);
                        break;
                    case SubZone.RightPanel:
                        DrawCur(g, BaseRight, offsetZoneSz, charW, nOfDigitsR, s2);
                        break;
                    default:
                        break;
                }

                g.DrawRectangle(caretPen, s1);
                g.DrawRectangle(caretPen, s2);
            }

            if (selectedZone == SubZone.OffsetMargin && curInputingOffset >= 0)
            {
                DrawInputingOffset(g, new Rectangle(offsetZone.X, offsetZone.Y, offsetZone.Width, lineHeight), charW, stringW);
            }

            /*///DEBUG
             g.DrawString(firstOffset.ToString(), Font, Brushes.Lime, 15, 15);
             g.DrawString(offsetMouse.ToString(), Font, Brushes.Lime, 15, 32);
             g.DrawString(lastOffset.ToString(), Font, Brushes.Orange, 15, 48);//*/
            RenderReperes(g);
        }

        /// <summary>
        /// Rendu du "numéro de ligne" (décalage)
        /// </summary>
        /// <param name="g"></param>
        /// <param name="stringW"></param>
        /// <param name="offsetCurX"></param>
        /// <param name="blockRectL"></param>
        /// <param name="p">Décalage de la ligne en cours</param>
        /// <param name="sBegin">Début de la sélection obtenu via <see cref="GetSelectionRange"/>.</param>
        private void DrawOffset(Graphics g, int stringW, int offsetCurX, Rectangle blockRectL, long p, long sBegin)
        {
            long number = p;
            Brush br = offsetForeBr;
            if (sBegin >= p && sBegin < p + lineLength)
            {
                if (sBegin > p)
                    br = offsetFocusForeBr;
                else
                    br = offsetCurrentLineForeBr;
                //dessin de la position actuelle
                number = sBegin;
            }

            g.DrawString(Utils.ToBaseString(number, offsetBase).PadLeft(stringW, '0'), Font, br, offsetCurX, blockRectL.Y);
        }

        /// <summary>
        /// Rendu du curseur (caret).
        /// </summary>
        /// <param name="g"></param>
        /// <param name="focusedBase">Base de numération de la zone active.</param>
        /// <param name="offsetZoneSz"></param>
        /// <param name="charW">Largeur d'un caractère</param>
        /// <param name="nOfDigits"></param>
        /// <param name="blockRect"></param>
        private void DrawCur(Graphics g, int focusedBase, Size offsetZoneSz, int charW, int nOfDigits, Rectangle blockRect)
        {
            // Le texte étant centré, il faut calculer la position de gauche de celui-ci
            // pour placer un caret horizontal sous le chiffre en cours.
            int baseX = blockRect.X + (blockW - nOfDigits * charW) / 2;
            int sx = baseX + selectionSubCaret * charW;
            int sy = blockRect.Y + (offsetZoneSz.Height + lineHeight) / 2 - (int)(caretPen.Width + subCaretPen.Width);
            if (focusedBase > 0 && selectionSubCaret > 0)
            {
                g.FillRectangle(backBr, baseX, blockRect.Y, charW * selectionSubCaret, blockRect.Height);
                g.DrawString(Utils.ToBaseString(curInputingByte, focusedBase).Substring(0, selectionSubCaret),
                    Font, foreBr, baseX, blockRect.Y);
            }

            g.DrawLine(subCaretPen, sx, sy, sx + charW, sy);
        }

        /// <summary>
        /// Affiche les chiffres du décalage en cours d'entrée, dans le cas où on saisit directement le décalage à gauche.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="blockoffsetZone"></param>
        /// <param name="charW"></param>
        /// <param name="nOfDigits"></param>
        private void DrawInputingOffset(Graphics g, Rectangle blockoffsetZone, int charW, int nOfDigits)
        {
            //On ne place pas de caret, juste les chiffres déjà entrés sur fond blanc, aligné à gauche dans le rectangle.
            g.FillRectangle(offsetBackBr, blockoffsetZone);
            g.DrawString(Utils.ToBaseString(curInputingOffset, offsetBase),
                Font, offsetForeBr, blockoffsetZone.Location);
        }

        /// <summary>
        /// Auxiliaire : charge la police spéciale servant à afficher les caractères de contrôle non imprimables.
        /// Il s'agit de la plage ASCII allant de 0 à 31 (0x1F).
        /// </summary>
        private void LoadControlCharsFont()
        {
            if (controlCharFont == null)
            {
                privateFontCollection = new PrivateFontCollection();
                string ppath = Path.Combine(Application.StartupPath, "plaux.ttf");
                if (File.Exists(ppath))
                {
                    privateFontCollection.AddFontFile(ppath);
                    if (privateFontCollection.Families.Length > 0)
                        controlCharFont = new Font(privateFontCollection.Families[0], Font.Size);
                    else
                        controlCharFont = new Font("Courier New", 12);
                }
                else
                {
                    controlCharFont = new Font("Courier New", 12);
                }
            }
            else
            {
                controlCharFont = new Font(controlCharFont.FontFamily, Font.Size);
            }
        }



        /// <summary>
        /// Rendu d'une ligne entière de données dans le contrôle.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="bs">Base de numération</param>
        /// <param name="line">Données à afficher</param>
        /// <param name="len">Longueur des données effectivement lus à partir du flux.</param>
        /// <param name="rect"></param>
        /// <param name="lineOffset">Position de départ</param>
        /// <param name="maxWidth">Limite graphique en largeur</param>
        protected unsafe void DrawLine(Graphics g, int bs, byte* line, int len, Rectangle rect, long lineOffset, int maxWidth, long sBegin, long sEnd)
        {
            int status;//ligne entière ou pas

            if (selectionLength == 0)
                status = 0;
            else
            {
                if (lineOffset > sEnd || lineOffset + len < sBegin)
                {
                    status = 0;
                }
                else if ((lineOffset >= sBegin && lineOffset + len <= sEnd)
                    || (lineOffset + len >= sBegin && lineOffset <= sEnd))
                {
                    status = 1;
                }
                else
                    status = 2;
            }
            switch (status)
            {
                case 0: //pas de sélection
                    for (long i = 0; i < len; i++)
                    {
                        DrawBlockValue(g, rect, line[i], bs, foreBr);

                        rect.X += rect.Width;
                    }
                    break;

                case 1: //Général 
                    int gOffset = (int)Max(0, sBegin - lineOffset) * blockW;
                    int eOffset = ((int)Min(lineLength, sEnd - lineOffset + 1)) * blockW;
                    g.FillRectangle(backBrSel, new Rectangle(rect.X + gOffset, rect.Y, eOffset - gOffset, lineHeight));
                    for (long i = 0; i < len; i++)
                    {
                        DrawBlockValue(g, rect, line[i], bs, IsOffsetSelected(lineOffset + i) ? foreBrSel : foreBr);
                        rect.X += rect.Width;
                    }
                    break;
                case 2://sélection entière
                    g.FillRectangle(backBrSel, new Rectangle(rect.X, rect.Y, maxWidth, lineHeight));
                    //Ligne entière
                    for (long i = 0; i < len; i++)
                    {
                        DrawBlockValue(g, rect, line[i], bs, foreBrSel);
                        rect.X += rect.Width;
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Rendu du contenu d'un bloc.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="r"></param>
        /// <param name="v">Valeur en cours</param>
        /// <param name="b">Base de numération</param>
        /// <param name="foreBr"></param>
        protected void DrawBlockValue(Graphics g, Rectangle r, byte v, int b, Brush foreColor)
        {
            if (b < 0)
            {
                DrawASCIIChar(g, r, v, foreColor);
            }
            else
                g.DrawString(Utils.ToBaseString(v, b), Font, foreColor, r, Utils.format);

        }

        /// <summary>
        /// Prend en charge le rendu d'un caractère simple sur un octet.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="r"></param>
        /// <param name="v"></param>
        /// <param name="foreColor"></param>
        protected void DrawASCIIChar(Graphics g, Rectangle r, byte v, Brush foreColor)
        {
            if (v < 0x21)
            {
                //Contrôle
                g.DrawString(new string((char)(v + 0x21), 1), ControlCharFont, foreColor, r, Utils.format);
            }
            else
                g.DrawString(Utils.ToBaseString(v, -1), Font, foreColor, r, Utils.format);
        }

        /// <summary>
        /// Rectangle vide si le décalage spécifié n'est pas visible à l'écran.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="left">Le rectangle dans la zone de gauche</param>
        /// <param name="right">Le rectangle dans la zone de droite</param>
        public virtual void GetRectanglesFromOffset(long offset, out Rectangle left, out Rectangle right)
        {
            Rectangle ret1 = Rectangle.Empty;
            Rectangle ret2 = Rectangle.Empty;

            if (IsOffsetVisible(offset)
                || (offset >= dataStream.Length && IsOffsetVisible(dataStream.Length - 1)))
            {
                int y = leftZone.Y + (int)(offset / lineLength - firstLine) * lineHeight;
                int x = (int)(offset % lineLength) * blockW;
                ret1 = ret2 = new Rectangle(x, y, blockW, lineHeight);
                ret2.X += rightZone.X;
                ret1.X += leftZone.X;
            }

            left = ret1; right = ret2;
        }

        #region Repères de texte
        private List<Repere> reperes;
        private Dictionary<Rectangle, int> renderedReperesG;
        private Dictionary<Rectangle, int> renderedReperesD;

        /// <summary>
        /// Les repères de texte actuellement définis dans les données.
        /// </summary>
        public List<Repere> Reperes { get => reperes; }

        /// <summary>
        /// Rendu des annotation (Repères) visibles dans le contrôle actuellement.
        /// </summary>
        /// <param name="g"></param>
        public virtual void RenderReperes(Graphics g)
        {
            renderedReperesG.Clear();
            renderedReperesD.Clear();
            for (int i = 0; i < reperes.Count; i++)
            {
                Repere r = reperes[i];
                if (IsOffsetVisible(r.Position))
                {
                    GetRectanglesFromOffset(r.Position, out Rectangle G, out Rectangle D);
                    r.Draw(g, G);
                    r.Draw(g, D);
                    renderedReperesG.Add(G, i);
                    renderedReperesD.Add(D, i);
                }
            }
        }

        /// <summary>
        /// Retourne l'indice du repère à la position graphique spécifiée en coordonnées relatif à ce contrôle, ou -1 s'il n'y en a pas.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public int GetRepereFromPoint(Point p)
        {
            if (leftZone.Contains(p))
            {
                foreach (var item in renderedReperesG)
                {
                    if (item.Key.Contains(p)) return item.Value;
                }
            }
            else if (rightZone.Contains(p))
            {
                foreach (var item in renderedReperesD)
                {
                    if (item.Key.Contains(p)) return item.Value;
                }
            }
            return -1;

        }

        /// <summary>
        /// Ajoute un repère de texte dans le contrôle.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="color"></param>
        /// <param name="pos"></param>
        public void AddRepère(string text, Color color, long pos)
        {
            reperes.Add(new Repere(text, color, pos));
        }

        /// <summary>
        /// Enregistre les repères en cours dans le fichier spécifié.
        /// </summary>
        /// <param name="file"></param>
        public void SaveReperes(string file)
        {
            Repere.Save(file, reperes);
        }

        /// <summary>
        /// Ajoute les repères contenus dans le fichier et enregistrés avec SaveReperes.
        /// </summary>
        /// <param name="file"></param>
        public void LoadReperes(string file)
        {
            reperes.AddRange(Repere.LoadFromFile(file));
        }
        #endregion
    }
}
