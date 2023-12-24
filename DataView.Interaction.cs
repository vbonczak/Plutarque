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
        #region Clavier
        /// <summary>
        /// Entrée manuelle : effacement d'un caractère (octet) en conservant le poids des autres
        /// </summary>
        /// <param name="inputBuffer"></param>
        /// <param name="bufferCaret"></param>
        /// <param name="b"></param>
        protected virtual void BackspaceOnInput(ref byte inputBuffer, ref int bufferCaret, byte maxValue, int b)
        {
            int nd = GetNbofDigits(maxValue, b);
            if (bufferCaret > 0)
            {
                bufferCaret--;
                inputBuffer = (byte)(inputBuffer - (inputBuffer % Power(b, nd - bufferCaret)));
            }
        }

        /// <summary>
        /// Entrée manuelle : effacement d'un caractère des unités, retour à un chiffre divisé par la base.
        /// </summary>
        /// <param name="inputBuffer"></param> 
        /// <param name="b"></param>
        protected virtual void BackspaceOnOffset(ref long inputBuffer, int b)
        {
            inputBuffer /= b;
        }


        /// <summary>
        /// Sous routine d'entrée dans une base de numération donnée.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="e"></param>
        protected void KeyInBase(int b, KeyPressEventArgs e)
        {
            if (ReadOnly) return;
            if (selectionStart >= dataStream.Length)
                InsertZeros(selectionStart, 1);//On crée un octet ou plusieurs, en zéros.
            if (b == -1)
            {
                //Caractère en tant que tel
                short v = (short)e.KeyChar;
                switch (e.KeyChar)
                {
                    case '\r':
                        switch (ReturnMode)
                        {
                            case ReturnModeConvention.crlf:
                                WriteByte((byte)(v & 0xff), SelectionStart);
                                SelectionStart++;
                                v = (short)'\n';
                                WriteByte((byte)(v & 0xff), SelectionStart);
                                break;
                            case ReturnModeConvention.lf:
                                v = (short)'\n';
                                WriteByte((byte)(v & 0xff), SelectionStart);
                                break;
                            case ReturnModeConvention.cr:
                                WriteByte((byte)(v & 0xff), SelectionStart);
                                break;
                            default:
                                break;
                        }
                        SelectionStart++;
                        break;
                    default:
                        WriteChar(e.KeyChar);
                        break;
                }

            }
            else if (b > 0)
            {
                if (e.KeyChar == '\b')
                {
                    //effacer le dernier
                    BackspaceOnInput(ref curInputingByte, ref selectionSubCaret, 255, b);
                }
                else if (e.KeyChar == '\r')
                {
                    ValidateInput();
                }
                else
                    try
                    {
                        int nd = GetNbofDigits(255, b);
                        int v = Convert.ToInt32(new string(e.KeyChar, 1), b);

                        if (v < b)
                        {
                            curInputingByte += (byte)(v * Power(b, nd - selectionSubCaret - 1));//coupage au bit près
                            selectionSubCaret++;
                            if (selectionSubCaret >= nd)
                            {
                                //On valide la case active
                                // ValidateInput();// WriteByte(curInputingByte, selectionStart);
                                SelectionStart++;
                                curInputingByte = 0;
                                // selectionSubCaret = 0; --> appel à validate Input dans SelectionStart++
                            }
                        }
                    }
                    catch (Exception)//parse error
                    {
                    }
            }

            Refresh();
        }

        private void View_Sub_KeyPress(object sender, KeyPressEventArgs e)
        {
            selectionLength = 0;

            switch (selectedZone)
            {
                case SubZone.LeftPanel:
                    KeyInBase(BaseLeft, e);
                    break;
                case SubZone.RightPanel:
                    KeyInBase(BaseRight, e);
                    break;
                case SubZone.OffsetMargin:
                    KeyInMargin(e);
                    break;
                case SubZone.MiddleMargin:
                    break;
                case SubZone.OutsideBorders://Normalement n'arrive pas : dans ce cas le contrôle n'a pas le focus.
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Gestion de la saisie du décalage par l'utilisateur (défilement manuel)
        /// </summary>
        /// <param name="e"></param>
        protected void KeyInMargin(KeyPressEventArgs e)
        {
            if (curInputingOffset == -1)
                curInputingOffset = 0;
            if (e.KeyChar == '\b')
            {
                //effacer le dernier
                BackspaceOnOffset(ref curInputingOffset, offsetBase);
            }
            else if (e.KeyChar == '\r')
            {
                ValidateOffsetInput();
            }
            else
                try
                {
                    int nd = GetNbofDigits(dataStream.Length, offsetBase);
                    int v = Convert.ToInt32(new string(e.KeyChar, 1), offsetBase);

                    if (v < offsetBase)
                    {
                        curInputingOffset = OffsetBase * curInputingOffset + v;

                        if (curInputingOffset >= dataStream.Length)
                        {
                            //On valide le décalage entré, quand on arrive à la limite
                            ValidateOffsetInput();
                        }
                    }
                }
                catch (Exception)//parse error (silent)
                {
                }

            Refresh();
        }

        /// <summary>
        /// Auxiliaire : valide la saisie en cours dans son bloc.
        /// </summary>
        private void ValidateOffsetInput()
        {
            firstLine = Min(curInputingOffset, dataStream.Length) / lineLength;
            SetScrollFromLine(firstLine);
            curInputingOffset = -1;
            Refresh();
        }

        private void View_Sub_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ControlKey)
                Tip.Active = true;//rétablir l'infobulle
        }

        private void View_Sub_KeyDown(object sender, KeyEventArgs e)
        {
            /** 
             * Algo de la saisie :
             * Je clique ou utilise les touches <- -> PgUp PgDown End Begin... pour sélectionner une case
             * Entrée d'une lettre ou d'un chiffre compris entre 0 et base -1
             * on l'ajoute chiffre par chiffre (non trivial?)
             * Quand le "nombre" ( la case) est remplie, on passe au suivant.
             * Attention traitement spécial pour le mode ASCII (cf keypress)
             * On peut entrer des Alt-Codes (transcription sans UTF-8 pour l'instant [TODO]).
             */
            //Les raccourcis claviers sont gérés ici uniquement.
            if (e.Control || e.Shift)
                e.SuppressKeyPress = true;

            bool shiftPressed = e.Modifiers == Keys.Shift;
            if (!shiftPressed && e.Control)
            {
                return;
            }//QUOI?? [todo] : "Gênant si on veut garder la sélection mais utiliser Ctrl+.."

            if (e.KeyCode == Keys.Left)
            {
                Navigate(shiftPressed, -1);
            }
            else if (e.KeyCode == Keys.Right)
            {
                Navigate(shiftPressed, 1);
            }
            else if (e.KeyCode == Keys.Up)      //Défilement haut
            {
                Navigate(shiftPressed, -lineLength);
            }
            else if (e.KeyCode == Keys.Down)    //Défilement bas
            {
                Navigate(shiftPressed, lineLength);
            }
            else if (e.KeyCode == Keys.PageUp)
            {
                Navigate(shiftPressed, -(Height / lineHeight + 1) * lineLength);
            }
            else if (e.KeyCode == Keys.PageDown)
            {
                Navigate(shiftPressed, (Height / lineHeight + 1) * lineLength);
            }
            else if (e.KeyCode == Keys.End)
            {
                if (shiftPressed)
                    SetSelectionBound(SelectionEnd + SelectionEnd % lineLength + (lineLength - 1), true);
                else
                    SelectionStart += (int)(selectionStart % lineLength + (lineLength - 1));
            }
            else if (e.KeyCode == Keys.Home)
            {
                if (shiftPressed)
                    SetSelectionBound(SelectionEnd - SelectionEnd % lineLength, true);
                else
                    SelectionStart -= (int)(selectionStart % lineLength);
            }

            if (!shiftPressed)
                selectionLength = 0;


            Refresh();
            //pour l'expérience

            /*  System.Text.StringBuilder messageBoxCS = new System.Text.StringBuilder();
               messageBoxCS.AppendFormat("{0} = {1}", "Alt", e.Alt);
               messageBoxCS.AppendLine();
               messageBoxCS.AppendFormat("{0} = {1}", "Control", e.Control);
               messageBoxCS.AppendLine();
               messageBoxCS.AppendFormat("{0} = {1}", "Handled", e.Handled);
               messageBoxCS.AppendLine();
               messageBoxCS.AppendFormat("{0} = {1}", "KeyCode", e.KeyCode);
               messageBoxCS.AppendLine();
               messageBoxCS.AppendFormat("{0} = {1}", "KeyValue", e.KeyValue);
               messageBoxCS.AppendLine();
               messageBoxCS.AppendFormat("{0} = {1}", "KeyData", e.KeyData);
               messageBoxCS.AppendLine();
               messageBoxCS.AppendFormat("{0} = {1}", "Modifiers", e.Modifiers);
               messageBoxCS.AppendLine();
               messageBoxCS.AppendFormat("{0} = {1}", "Shift", e.Shift);
               messageBoxCS.AppendLine();
               messageBoxCS.AppendFormat("{0} = {1}", "SuppressKeyPress", e.SuppressKeyPress);
               messageBoxCS.AppendLine();
               MessageBox.Show(messageBoxCS.ToString(), "KeyDown Event");*/
        }

        /// <summary>
        /// Fonction interne utile pour la sélection au clavier.
        /// Si on va en haut, et qu'on saute de page, il faut que la ligne visible soit en haut (ancrée en haut), 
        /// dans l'autre cas (vers le bas), on se base sur la visibilité de la ligne ancrée en bas. Voir SelectionStart pour ce procédé.
        /// </summary>
        /// <param name="shiftPressed"></param>
        /// <param name="offset"></param>
        protected void Navigate(bool shiftPressed, int offset)
        {
            if (shiftPressed)
            {
                SelectionLength += offset;
            }
            else
            {
                SelectionStart += offset;
            }
        }

        private void View_Sub_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.Control)
            {
                Tip.Active = false;//Pour pouvoir zoomer en étant sur un repère
                e.IsInputKey = false;
            }
            else
                e.IsInputKey = true;//On prend toutes les touches de contrôle (échap, les flèches, retour, etc.)
        }

        /// <summary>
        /// Provoque l'écriture dans le flux de la donnée en cours d'entrée au clavier.
        /// </summary>
        public void ValidateInput()
        {
            if (ReadOnly) return;
            switch (SelectedZone)
            {
                case SubZone.LeftPanel:
                    ValidateInputInBase(BaseLeft);
                    break;
                case SubZone.RightPanel:
                    ValidateInputInBase(BaseRight);
                    break;
                default:
                    break;
            }
            selectionSubCaret = 0;
            curInputingByte = 0;
        }

        /// <summary>
        /// Sous-routine de validation d'entrée utilisateur selon la base de numération.
        /// </summary>
        /// <param name="b"></param>
        protected void ValidateInputInBase(int b)
        {
            if (b > 0 && selectionSubCaret > 0)
            {
                byte oldVal = ReadByte(selectionStart);
                int n = GetNbofDigits(255, b); //n chiffres dans le bloc
                int diff = oldVal % Power(b, n - selectionSubCaret);//<256
                byte newVal = (byte)(curInputingByte + diff);
                WriteByte(newVal, selectionStart);
            }
        }
        #endregion

        #region Souris
        protected long offsetMouse;//la position sous le pointeur de souris

        /// <summary>
        /// Tip affiché précedemment, pour éviter de l'afficher une 2e fois
        /// </summary>
        private int prevTip = -1;

        /// <summary>
        /// Mouvement de curseur
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void View_Main_MouseMove(object sender, MouseEventArgs e)
        {
            offsetMouse = GetOffsetFromPoint(e.Location);
            if (offsetZone.Contains(e.Location))
            {
                Cursor = Cursors.Default;
            }
            else if (leftZone.Contains(e.Location) || rightZone.Contains(e.Location))
            {
                Cursor = Cursors.IBeam;
                //Repères sous la souris
                int rep = GetRepereFromPoint(e.Location);
                if (rep >= 0 && prevTip != rep)
                {
                    Tip.Show(reperes[rep].Title, this, e.Location, 1500);
                }
                prevTip = rep;
            }
            else
            {
                Cursor = Cursors.Default;
            }

            if (e.Button == MouseButtons.Left)
            {

                if (ModifierKeys == Keys.None)
                {
                    SetSelectionBound(offsetMouse);
                }
            }
            else
            {

            }

            Refresh();
        }

        private void View_Main_MouseLeave(object sender, EventArgs e)
        {
            Cursor = Cursors.Default;
        }

        /// <summary>
        /// Se produit lorsque l'utilisateur utilise la molette de défilement.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void View_Sub_MouseWheel(object sender, MouseEventArgs e)
        {
            if (ModifierKeys == Keys.Control)
            {
       

                //Zoom
                if (e.Delta < 0)
                {
                    //arrière
                    zoom = Max(MaxZoomFactor, zoom - 0.1f);
                }
                else if (e.Delta > 0)
                {
                    //avant
                    zoom = Min(10f, zoom + 0.1f);
                }
                subCaretPen.Width = Max(1.0f, zoom);
                caretPen.Width = Max(1.0f, zoom);

                RefreshConsistently();
            }
            else
            {
                if (ModifierKeys == Keys.Shift)
                {
                    //Scroll
                    if (!scrollBar.Visible)
                        return;
                    if (e.Delta < 0)
                    {
                        scrollBar.Value = Min(GetMaxScroll(), scrollBar.Value + 3);
                    }
                    else if (e.Delta > 0)
                    {
                        scrollBar.Value = Max(scrollBar.Minimum, scrollBar.Value - 3);
                    }
                    scrollBar_Scroll(null, null);//comme si on avait défilé à la main
                }
                else
                {
                    //Sans maj, on défile doucement (3 par 3)
                    firstLine = Max(0, Min(firstLine - 3 * Sign(e.Delta), GetNbOflines()));
                    SetScrollFromLine(firstLine);
                }
                Refresh();
            }

        }

        private void View_Main_MouseDown(object sender, MouseEventArgs e)
        {
            long hoverPos = GetOffsetFromPoint(e.Location);
            if (e.Button == MouseButtons.Left)
            {
                if (ModifierKeys == Keys.Shift)
                {
                    //Sélection d'une étendue de données
                    SetSelectionBound(hoverPos, true);
                }
                else
                {
                    Select(hoverPos, 0);
                }

                SubZone newZone = GetSubZoneAtPoint(e.Location);
                if (selectedZone != newZone)
                {
                    //Les modifs, si elles ne sont pas déjà validées dans le set de SelectionStart, le seront ici
                    ValidateInput();
                    curInputingOffset = -1;
                }
                selectedZone = newZone;
            }
        }

         
        #endregion

    }
}
