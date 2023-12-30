using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using static System.Math;
using System.Windows.Forms;
using System.Drawing.Text;
using System.Diagnostics;

namespace Plutarque
{
    public class DataTextBox : DataView
    {
        /// <summary>
        /// Cette propriété n'est pas utilisée par ce contrôle.
        /// </summary>
        [Browsable(false)]
        public override int BaseRight { get => base.BaseRight; set => base.BaseRight = value; }

        public override void EnsureVisible(long offset)
        {
            //todo défiler horizontalement
            firstOffset = offset;//ÀF gérer les réfs à firstLine partout
        }

        protected override void RenderView(Graphics g, Rectangle r)
        {
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
            //pas de zone de droite

            /*blockWidth = (int)(offsetZoneSz.Width * Max(BaseLeft < 0 ? 2 : Log(256, BaseLeft),
                BaseRight < 0 ? 2 : Log(256, BaseRight)) / stringSz);*/

            int offsetCurX = r.X;
            int leftCurX = offsetCurX + offsetZoneSz.Width;
            int textZoneWidth = (r.Width - offsetZoneSz.Width - middleMarginWidth) / 2;
            //int rightCurX = leftCurX + textZoneWidth + middleMarginWidth;

            blockW = nOfDigitsL * charW + innerMargin;


            //Adaptation de la taille de ligne en fonction de la largeur du bloc et des zones
            lineLength = textZoneWidth / blockW;

            //Nous assurons la position actuelle dans le flux
            dataStream.Seek(firstOffset, SeekOrigin.Begin);

            //Fond général
            g.FillRectangle(backBr, r);

            //Colonne du décalage
            g.FillRectangle(offsetBackBr, new Rectangle(r.Location,
                new Size(offsetZoneSz.Width, r.Height)));

            //Marge du milieu
            //pas de marge du milieu

            //Zones pour l'intéraction avec la souris
            leftZone = new Rectangle(leftCurX, r.Y, textZoneWidth, r.Height);
            offsetZone = new Rectangle(offsetCurX, r.Y, offsetZoneSz.Width, r.Height);
            rightZone = Rectangle.Empty;
            //todo gros tampon puis parcours (besoin de calculer le total d'abord. Mettre le tampon dans la classe générale)


            Rectangle blockRectL = new Rectangle(leftCurX, r.Y, blockW, offsetZoneSz.Height);
            long p = 0;//Position actuelle dans le flux


            GetSelectionRange(out long sBegin, out long sEnd);

            //de gauche à droite uniquement
            //une seule ligne donc
            int L = dataStream.Read(buffer, 0, Min((int)dataStream.Length, bufferSz));
             
            DrawOffset(g, stringW, offsetCurX, blockRectL, p, sBegin);//offset
              
            DrawLine(g, BaseLeft, buffer, l, blockRectL, p, textZoneWidth, sBegin, sEnd);//Partie gauche
            blockRectL.Y += offsetZoneSz.Height;



            //Post condition: p vaut un nombre entier de lignes englobant la position finale (si on n'a pas dessiné une ligne entière)
            //p est donc une surapprox de lastOffset

            //p est le dernier *début* de ligne suivante. Donc la dernière pos est au plus p - 1, ou la fin du flux si inférieure.

            lastOffset = Min(dataStream.Length - 1, p - 1);

            lineHeight = offsetZoneSz.Height;

            SetScrollBarLength(
                firstLine == 0
                &&
                lastOffset == dataStream.Length ?
                                                            0
                        :       /* pas à la fin */          GetScrollTicks());

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

    }
}
