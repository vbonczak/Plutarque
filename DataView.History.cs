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
        #region Annuler/Répéter

        private int historyLength;

        /// <summary>
        /// Indice dans l'historique de l'action venant d'être ajoutée, ou venant d'être refaite (via Redo).
        /// </summary>
        protected int historyPoint = 0;

        /// <summary>
        /// Est à true pour ignorer l'enregistrement d'une action quand on navigue 
        /// dans le passé, ou quand on effectue des opérations composites.
        /// </summary>
        protected bool navigating = false;

        /// <summary>
        /// La pile de l'historique (plus récent -> au début).
        /// </summary>
        protected List<IAction> history;

        [DefaultValue(50)]
        public int HistoryLength { get => historyLength; set => historyLength = value; }

        /// <summary>
        /// Inscrit dans l'historique une nouvelle action annulable.
        /// </summary>
        /// <param name="action">L'action à ajouter.</param>
        protected void AddAction(IAction action)
        {
            if (action.IsModification)
                OnDataChanged(); //les données ont été modifiées de toute façon

            if (navigating) return; //Pas si on est en train d'annuler ou de refaire qqch.

            if (historyPoint > 0)
            {
                //Nous ne sommes pas au début, on oublie ce que l'on a annulé.
                history.RemoveRange(0, historyPoint);//historyPoint actuel exclu
                historyPoint = 0;
            }
            history.Insert(0, action);
            if (history.Count > historyLength)
                history.RemoveAt(history.Count - 1);//suppression de l'entrée la plus ancienne.
        }

        /// <summary>
        /// Revient à l'état antérieur à la dernière opération, si c'est possible.
        /// </summary>
        public void Undo()
        {
            if (CanUndo())
            {
                navigating = true;
                history[historyPoint].Undo(this);
                historyPoint++;
                navigating = false;
                Refresh();
            }
        }

        /// <summary>
        /// Indique s'il y a une opération à annuler dans l'historique.
        /// </summary>
        /// <returns></returns>
        public bool CanUndo()
        {
            return historyPoint >= 0 && historyPoint < history.Count;
        }

        /// <summary>
        /// Indique si une opération peut être réexécutée (qui aurait été annulée).
        /// </summary>
        /// <returns></returns>
        public bool CanRedo()
        {
            return historyPoint > 0;
        }

        /// <summary>
        /// Refait la dernière opération annulée, si une telle opération existe.
        /// </summary>
        public void Redo()
        {
            if (CanRedo())
            {
                navigating = true;
                historyPoint--;
                history[historyPoint].Redo(this);
                navigating = false;
                Refresh();
            }
        }
        #endregion

        /// <summary>
        /// Interface générale pour représenter une action à stocker dans l'historique pour l'annuler
        /// ou la refaire.
        /// </summary>
        protected interface IAction
        {
            bool IsModification { get; }
            void Undo(DataView dt);
            void Redo(DataView dt);

            string Description { get; }
        }

        /// <summary>
        /// Action d'écriture d'une valeur.
        /// </summary>
        protected class ByteWrittenAction : IAction
        {
            private byte after, before;
            private long offset;

            /// <summary>
            /// Créée une nouvelle action de type écriture d'un octet.
            /// </summary>
            /// <param name="firstValue">Valeur présente auparavant</param>
            /// <param name="newValue">Nouvelle valeur</param>
            /// <param name="pos">Position dans le flux</param>
            public ByteWrittenAction(byte firstValue, byte newValue, long pos)
            {
                after = newValue;
                before = firstValue;
                offset = pos;
            }

            public string Description => "écriture d'un octet";

            public bool IsModification => true;

            public void Redo(DataView dt)
            {
                lock (dt.dataStream)
                {
                    dt.dataStream.Seek(offset, SeekOrigin.Begin);
                    dt.dataStream.WriteByte(after);
                }
            }

            public void Undo(DataView dt)
            {
                lock (dt.dataStream)
                {
                    dt.dataStream.Seek(offset, SeekOrigin.Begin);
                    dt.dataStream.WriteByte(before);
                }
            }
        }

        /// <summary>
        /// Action d'insertion d'un tableau d'octets.
        /// </summary>
        protected class ArrayInsertedAction : IAction
        {
            private byte[] inserted;
            private long offset;

            public bool IsModification => true;
            public string Description => "insertion d'un tableau";

            /// <summary>
            /// Création de ArrayInsertedAction
            /// </summary>
            /// <param name="array">Tableau inséré</param>
            /// <param name="pos">Position d'insertion dans le flux</param>
            public ArrayInsertedAction(byte[] array, long pos)
            {
                inserted = array;
                offset = pos;
            }

            public void Redo(DataView dt)
            {
                dt.InsertArray(inserted, offset);
            }

            public void Undo(DataView dt)
            {
                dt.Delete(offset, inserted.Length);
            }
        }

        /// <summary>
        /// Action d'insertion d'un tableau d'octets.
        /// </summary>
        protected class ArrayMovedAction : IAction
        {
            private byte[] previousArray; //tableau qui fut remplacé.
            private long sourcePos;
            private long destPos;

            public bool IsModification => true;
            public string Description => "déplacement d'une zone";

            /// <summary>
            /// Création de ArrayMovedAction
            /// </summary>
            /// <param name="sourcePos">Emplacement où le tableau commençait auparavant.</param>
            /// <param name="destPos">Nouvelle position de ce tableau.</param>
            /// <param name="replacedArray">Tableau présent à la place auparavant.</param>
            public ArrayMovedAction(long sourcePos, long destPos, byte[] replacedArray)
            {
                previousArray = replacedArray;
                this.sourcePos = sourcePos;
                this.destPos = destPos;
            }

            public void Redo(DataView dt)
            {
                dt.MoveChunk(sourcePos, previousArray.Length, destPos);
            }

            public void Undo(DataView dt)
            {
                dt.MoveChunk(destPos, previousArray.Length, sourcePos);
                dt.WriteBytes(previousArray, destPos);
            }
        }

        /// <summary>
        /// Action de suppression d'un tableau d'octets.
        /// </summary>
        protected class ArrayDeletedAction : IAction
        {
            private byte[] previousArray; //tableau qui fut remplacé.
            private long pos;

            public bool IsModification => true;
            public string Description => "suppression d'une zone";

            /// <summary>
            /// Création de l'action correspondant à la suppression d'une étendue de données.
            /// </summary>
            /// <param name="offset">Position dans le flux.</param>
            /// <param name="array">Tableau supprimé.</param>
            public ArrayDeletedAction(long offset, byte[] array)
            {
                previousArray = array;
                pos = offset;
            }

            public void Redo(DataView dt)
            {
                dt.Delete(pos, previousArray.Length);
            }

            public void Undo(DataView dt)
            {
                dt.InsertArray(previousArray, pos);
            }
        }

        /// <summary>
        /// Action de déplacement de la sélection.
        /// </summary>
        protected class CursorMovedAction : IAction
        {
            public bool IsModification => false;
            private long prposition;
            private int prlgt;
            private long position;
            private int lgt;

            public string Description => "déplacement";

            /// <summary>
            /// Créée une nouvelle action de type déplacement de la zone de sélection.
            /// </summary>
            /// <param name="pos">Nouvelle position</param>
            /// <param name="len">Nouvelle longueur</param>
            /// <param name="prpos">Ancienne position</param>
            /// <param name="prlen">Ancienne longueur</param>
            public CursorMovedAction(long pos, int len, long prpos, int prlen)
            {
                position = pos;
                lgt = len;
                prposition = prpos;
                prlgt = prlen;

            }

            public void Redo(DataView dt)
            {
                dt.Select(position, lgt);
            }

            public void Undo(DataView dt)
            {
                dt.Select(prposition, prlgt);
            }
        }

    }
}
