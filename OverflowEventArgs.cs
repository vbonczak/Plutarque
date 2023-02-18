namespace Plutarque
{
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