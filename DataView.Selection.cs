using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        /// <summary>
        /// Sélectionne une zone de données. Cette fonction prend en charge l'historique.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="length"></param>
        public void Select(long start, int length)
        {
            if (start < dataStream.Length)
            {
                AddAction(new CursorMovedAction(start, length, SelectionStart, SelectionLength));

                SelectionStart = start;
                SelectionLength = length;
            }
        }

        /// <summary>
        /// Indique si l'indice spécifié est dans les bornes de la sélection.
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        protected bool IsOffsetSelected(long offset)
        {
            GetSelectionRange(out long b, out long e);
            return offset >= b && offset <= e;
        }
         

        /// <summary>
        /// Inscrit à l'emplacement sélectionné les données contenues dans le presse-papier
        /// si celui-ci contient du texte.
        /// </summary>
        public void Paste()//TODO selection length est à 0 parfois?
        {
            if (IsInternalClipboard())
            {
                GetSelectionRange(out long begin, out _);
                WriteBytes(pressePapiers, begin);
            }
            else if (Clipboard.ContainsText())
            {
                string dt = Clipboard.GetText();
                foreach (char c in dt)
                {
                    WriteChar(c);
                }
                Refresh();
            }

        }
        /// <summary>
        /// Gestion du presse-papiers interne.
        /// </summary>
        protected byte[] pressePapiers;
        protected string lastCopiedText = "";

        /// <summary>
        /// Indique si le texte contenu dans le presse-papiers est celui copié à partir de ce contrôle,
        /// ou si au contraire il provient d'un autre programme.
        /// </summary>
        /// <returns></returns>
        private bool IsInternalClipboard()
        {
            return Clipboard.ContainsText() && Clipboard.GetText() == lastCopiedText;
        }
        /// <summary>
        /// Copie dans le presse-papiers interne les données sélectionnées, et dans le presse papier
        /// système les données dans la base de numération actuellement active.
        /// 
        /// Le presse-papier ne peut pas contenir plus de int.MaxValue octets.
        /// </summary>
        public void Copy()
        {
            pressePapiers = new byte[Min(Max(1, Abs(selectionLength)), int.MaxValue)];
            GetSelectionRange(out long begin, out long end);
            lock (dataStream)
            {
                dataStream.Seek(begin, SeekOrigin.Begin);
                dataStream.Read(pressePapiers, 0, pressePapiers.Length);
            }
            string textToStore = "";//que va t-on mettre dans le presse papier du système ?
            switch (SelectedZone)
            {
                case SubZone.LeftPanel:
                    textToStore = Utils.ArrayToString(baseLeft, pressePapiers);
                    break;
                case SubZone.RightPanel:
                    textToStore = Utils.ArrayToString(baseRight, pressePapiers);
                    break;
                default:
                    //Les deux, avec le décalage
                    textToStore += Utils.ToBaseString(begin, offsetBase) + "\r\n";
                    textToStore += Utils.ArrayToString(baseLeft, pressePapiers) + "\r\n";
                    textToStore += Utils.ArrayToString(baseRight, pressePapiers) + "\r\n";
                    textToStore += "Position suivante (exclue) : " + Utils.ToBaseString(end + 1, offsetBase);
                    break;
            }

            Clipboard.SetText(textToStore);
            lastCopiedText = textToStore;//Pour savoir si c'est à nous
        }

        /// <summary>
        /// Coupe les données sélectionnées : supprime la partie sélectionnée après
        /// l'avoir copiée dans le presse-papiers (comme Copy).
        /// </summary>
        public void Cut()
        {
            if (selectionLength != 0)
            {
                GetSelectionRange(out long a, out long b);
                Copy();
                Delete(a, b - a + 1);
            }
        }

        /// <summary>
        /// Auxiliaire : à partir de la position donnée, calcule la longueur de la sélection
        /// et la met à jour en conséquence. Cette fonction ne prend pas en charge l'historique.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="historyEnabled">Si l'action doit être stockée dans l'historique pour être annulée
        /// ultérieurement ou pas. Les actions composites et d'animation (p. ex. le curseur glissant pour sélectionner
        /// une zone) ne doivent pas faire l'objet d'un enregistrement.</param>
        private void SetSelectionBound(long pos, bool historyEnabled = false)
        {
            long l = pos - SelectionStart;
            if (l > int.MaxValue || l < int.MinValue)
            {
                OnUserInducedOverflow(new OverflowEventArgs(OverflowReasons.SelectionRangeTooBig));
            }
            else
            {
                if (historyEnabled)
                    AddAction(new CursorMovedAction(selectionStart, (int)l, selectionStart, SelectionLength));
                selectionLength = (int)l;
            }
        }

        /// <summary>
        /// Retourne les positions de début et de fin de la sélection.
        /// </summary>
        /// <param name="begin">Début de la sélection (inclus)</param>
        /// <param name="end">Fin de la sélection (inclus)</param>
        public void GetSelectionRange(out long begin, out long end)
        {
            if (selectionLength < 0)
            {
                begin = selectionStart + selectionLength;
                end = selectionStart;
            }
            else
            {
                begin = selectionStart;
                end = selectionStart + selectionLength;
            }
        }
    }
}
