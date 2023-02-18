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
        /// Gestion de l'événement selection changed.
        /// </summary>
        /// <param name="sender"></param>
        public delegate void EventHandler(object sender);

        /// <summary>
        /// Se produit lorsque le curseur est déplacé.
        /// </summary>
        [Description("Se produit lorsque le curseur est déplacé.")]
        public event EventHandler SelectionChanged;

        /// <summary>
        /// Se produit lorsque l'utilisateur effectue une action qui provoque un dépassement de capacité 
        /// générique.
        /// </summary>
        /// <param name="sender"></param>
        public delegate void OverflowHandler(object sender, OverflowEventArgs ea);

        /// <summary>
        /// Se produit lorsque l'utilisateur effectue une action qui provoque un dépassement de capacité.
        /// </summary>
        [Description("Se produit lorsque l'utilisateur effectue une action qui provoque un dépassement de capacité.")]
        public event OverflowHandler UserInducedOverflow;

        /// <summary>
        /// Se produit lorsque les bornes de sélection sont modifiées.
        /// </summary>
        protected void OnSelectionChanged()
        {
            if (SelectionChanged != null) SelectionChanged(this);
        }

        /// <summary>
        /// Se produit lorsque l'utilisateur effectue une action qui provoque un dépassement de capacité.
        /// </summary>
        /// <param name="ea"></param>
        protected void OnUserInducedOverflow(OverflowEventArgs ea)
        {
            if (UserInducedOverflow != null) UserInducedOverflow(this, ea);
        }

       /// <summary>
       /// Se produit en cas de modification des données chargées dans le contrôle.
       /// </summary>
       [Description("Se produit en cas de modification des données chargées dans le contrôle.")]
        public event EventHandler DataChanged;
        /// <summary>
        /// Se produit lorsque les données chargées sont modifiées.
        /// </summary>
        protected void OnDataChanged()
        {
            if (DataChanged != null) DataChanged(this);
        }

    }

    public enum OverflowReasons
    {
        /// <summary>
        /// La taille des données sélectionnées dépasse la valeur maximale admise (int32).
        /// </summary>
        SelectionRangeTooBig
    }

    /// <summary>
    /// Encapsule les données d'un événement de type UserInducedOverflow, se produisant
    /// lorsque l'utilisateur effectue une action qui provoque un dépassement de capacité 
    /// générique.
    /// </summary>
    public class OverflowEventArgs
    {
        private OverflowReasons reason;

        public OverflowEventArgs(OverflowReasons r)
        {
            reason = r;
        }

        /// <summary>
        /// La raison de ce dépassement.
        /// </summary>
        protected OverflowReasons Reason { get => reason; set => reason = value; }
    }
}
