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

        protected long firstLine; //première ligne affichée
        protected int lineLength; //longueur de la ligne en octets
        protected int lineHeight; // hauteur de la ligne graphique en pixels
        protected int blockW; //largeur d'un bloc

        protected Rectangle leftZone = Rectangle.Empty;//rectangles calculés au rendu
        protected Rectangle rightZone = Rectangle.Empty;
        protected Rectangle offsetZone = Rectangle.Empty;

        protected float zoom = 1f;

        protected int innerMargin = 5; //Marge (totale) dans un bloc en pixels

        protected long lastOffset;//Dernière position affichée dans le contrôle (incluse)
        protected long firstOffset;//première (incluse)

        private int scrollLines = 3; //lignes défilés avec la molette (et unité de défilement de la barre)

        private long selectionStart;

        protected byte curInputingByte = 0; //nombre en cours
        protected int selectionSubCaret = 0; //Position actuelle dans le nombre en cours (entre 0 et base-1)
        private int selectionLength;//peut être négative en cas de sélection 'à l'envers'

        private int offsetBase;
        protected long curInputingOffset = 0; //décalage en cours de saisie


        private bool ro;//lecture seule ou non

        private int middleMarginWidth;//taille de la marge au milieu

        //bases de numération de chaque côté
        private int baseRight;
        private int baseLeft;

        //La police pour afficher différemment les caractères de contrôle (00 - 31)
        protected PrivateFontCollection privateFontCollection;
        private Font controlCharFont = null;

        //Remplissages rapides
        private Brush backBr;
        private Brush foreBr;
        private Brush foreBrSel;
        private Brush offsetForeBr;
        private Brush offsetBackBr;
        private Brush middleMarginBr;
        private Brush backBrSel;

        private Color selectionColor;
        private Color selectionBackColor;
        private Color offsetColor;
        private Color offsetBackColor;
        private Color middleMarginColor;

        private Pen caretPen;
        private Pen subCaretPen;
        private Color caretColor;
        private Color subCaretColor;

        private long maxSupportedArrayLength;

        /// <summary>
        /// Variable de stockage : mode de passage à la ligne.
        /// </summary>
        private ReturnModeConvention returnMode;

        /// <summary>
        /// Zone du contrôle actuellement active, parmi la marge, les volets gauche ou droite, le séparateur du milieu ou une zone
        /// en dehors des limites du contrôle.
        /// </summary>
        private SubZone selectedZone;

        /// <summary>
        /// Le flux en lecture actuel
        /// </summary>
        private Stream dataStream;

        /// <summary>
        /// Couleur de texte de la sélection.
        /// </summary>
        [DefaultValue(typeof(Color), "HighlightText")]
        public Color SelectionColor
        {
            get => selectionColor; set
            {
                foreBrSel = new SolidBrush(value);
                selectionColor = value;
            }
        }

        /// <summary>
        /// Couleur du rectangle de sélection bloc par bloc.
        /// </summary>
        [DefaultValue(typeof(Color), "Red")]
        public Color CaretColor
        {
            get => caretColor; set
            {
                caretPen = new Pen(value, 1f);
                caretColor = value;
            }
        }

        /// <summary>
        /// Couleur du trait du curseur de sélection chiffre par chiffre.
        /// </summary>
        [DefaultValue(typeof(Color), "Gray")]
        public Color SubCaretColor
        {
            get => subCaretColor; set
            {
                subCaretPen = new Pen(value, 1f);
                subCaretColor = value;
            }
        }

        /// <summary>
        /// La couleur de fond du texte sélectionné.
        /// </summary>
        [DefaultValue(typeof(Color), "Highlight")]
        public Color SelectionBackColor
        {
            get => selectionBackColor; set
            {
                backBrSel = new SolidBrush(value);
                selectionBackColor = value;
            }
        }

        /// <summary>
        /// Obtient ou définit la couleur d'arrière-plan du contrôle.
        /// </summary>
        [DefaultValue(typeof(Color), "Window"), Description("La couleur d'arrière-plan de la zone principale")]
        public override Color BackColor
        {
            get => base.BackColor; set
            {
                backBr = new SolidBrush(value);
                base.BackColor = value;
            }
        }

        /// <summary>
        /// Obtient ou définit la couleur de texte du contrôle.
        /// </summary>
        [DefaultValue(typeof(Color), "WindowText"), Description("La couleur de texte de la zone principale")]
        public override Color ForeColor
        {
            get => base.ForeColor; set
            {
                foreBr = new SolidBrush(value);
                base.ForeColor = value;
            }
        }

        /// <summary>
        /// Longueur des données actuellement chargées dans le DataView.
        /// </summary>
        [Browsable(false)]
        public long DataLength
        {
            get => dataStream.Length;
        }

        /// <summary>
        /// Obtient une valeur indiquant si les données peuvent être modifiées.
        /// </summary>
        public bool ReadOnly { get => ro && !dataStream.CanWrite; set => ro = value; }

        /// <summary>
        /// Indique l'indice de fin de la sélection.
        /// </summary>
        [Browsable(false)]
        public long SelectionEnd { get => selectionStart + selectionLength; }

        /// <summary>
        /// Obtient ou définit la base de numération dans laquelle sont affichés les décalages dans la marge gauche.
        /// </summary>
        protected int OffsetBase { get => offsetBase; set => offsetBase = value; }

        /// <summary>
        /// La police utilisée pour afficher les caractères de contrôle non imprimables (0x00 - 0x1F).
        /// </summary>
        protected Font ControlCharFont
        {
            get
            {
                if (zoom == 1)
                    return controlCharFont;
                else
                {
                    return new Font(controlCharFont.FontFamily, controlCharFont.Size * zoom);
                }
            }
            set => controlCharFont = value;
        }

        /// <summary>
        /// Indice dans le flux de début de la sélection active. 
        /// Cet indice peut être postérieur à l'indice de fin de la sélection si la sélection
        /// est à l'envers.
        /// Le changement de cette valeur provoque un défilement centré sur celle-ci.
        /// </summary>
        [Browsable(false)]
        public long SelectionStart
        {
            get => selectionStart; set
            {
                //à chaque fois que l'on modifie cette valeur, on valide le bloc en cours (seulement en cas de changement)
                if (value != selectionStart)
                {
                    //Validation de l'entrée en cours
                    ValidateInput();

                    if (value <= dataStream.Length && value >= 0)
                    {
                        selectionStart = value;
                    }
                    //TODO caret dans le vide
                    OnSelectionChanged();
                }
                if (value < dataStream.Length && (value > lastOffset || value < firstOffset))//scroll dans tous les cas
                    ScrollToCaret();
            }
        }

        /// <summary>
        /// Spécifie la longueur de la sélection à partir du point SelectionStart. C'est une valeur
        /// algébrique : positive lorsque la sélection s'arrête après le curseur, négative lorsque
        /// la sélection va vers le début du flux.
        /// </summary>
        [Browsable(false)]
        public int SelectionLength
        {
            get => selectionLength; set
            {
                if (selectionLength == value) return;
                long v = selectionStart + value;

                v = Min(dataStream.Length - 1, Max(0, v));

                if (v > lastOffset)
                {
                    EnsureVisible(v);
                }
                else if (v < firstOffset)
                {
                    //scénario on sélectionne vers le haut
                    EnsureVisible(v);
                }
                selectionLength = (int)(v - selectionStart);

            }
        }

        /// <summary>
        /// Couleur de police de la colonne de décalage.
        /// </summary>
        [DefaultValue(typeof(Color), "DarkViolet")]
        public Color OffsetColor
        {
            get => offsetColor; set
            {
                offsetForeBr = new SolidBrush(value);
                offsetColor = value;
            }
        }

        /// <summary>
        /// Couleur de fond de la colonne de décalage.
        /// </summary>
        [DefaultValue(typeof(Color), "Control"), Description("Couleur de fond de la colonne de décalage")]
        public Color OffsetBackColor
        {
            get => offsetBackColor; set
            {
                offsetBackBr = new SolidBrush(value);
                offsetBackColor = value;
            }
        }

        /// <summary>
        /// Spécifie la convention à utiliser lorsque l'utilisateur entre un retour à la ligne.
        /// </summary>
        public ReturnModeConvention ReturnMode { get => returnMode; set => returnMode = value; }

        /// <summary>
        /// Marge de séparation entre les deux panneaux.
        /// </summary>
        [DefaultValue(10)]
        public int MiddleMarginWidth { get => middleMarginWidth; set => middleMarginWidth = value; }

        /// <summary>
        /// Couleur de fond de la marge médiane.
        /// </summary>
        [DefaultValue(typeof(Color), "Control"), Description("Couleur de fond de la séparation médiane")]
        public Color MiddleMarginColor
        {
            get => middleMarginColor; set
            {
                middleMarginBr = new SolidBrush(value);
                middleMarginColor = value;
            }
        }

        /// <summary>
        /// La zone actuellement sélectionnée (qui possède l'entrée/Focus)
        /// </summary>
        [Browsable(false)]
        public SubZone SelectedZone { get => selectedZone; }

        /// <summary>
        /// La base de numération utilisée dans la partie droite.
        /// </summary>
        [DefaultValue(-1), Description("La base de numération utilisée dans la partie droite.")]
        public int BaseRight
        {
            get => baseRight; set
            {
                baseRight = ValidateBase(value);
            }
        }

        /// <summary>
        /// La base de numération utilisée dans la partie gauche.
        /// </summary>
        [DefaultValue(16), Description("La base de numération utilisée dans la partie gauche.")]
        public int BaseLeft
        {
            get => baseLeft; set
            {
                baseLeft = ValidateBase(value);
            }
        }

        /// <summary>
        /// Validation de base, vérification que nous sommes dans les valeurs autorisées.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private int ValidateBase(int input)
        {
            if (input > Utils.GetMaxSupportedBase())
                throw new ArgumentException("La base est trop élevée.");
            if (input > 0 || Enum.IsDefined(typeof(SpecialBases), input))
                return input;
            else throw new ArgumentException("La base doit être non nulle ou une des valeurs spéciales.");
        }

        /// <summary>
        /// Obtient ou définit la police du texte affiché par le contrôle.
        /// </summary>
        public override Font Font
        {
            get
            {
                if (zoom == 1)
                    return base.Font;
                else
                {
                    return new Font(base.Font.FontFamily, base.Font.Size * zoom);
                }
            }
            set => base.Font = value;
        }

        /// <summary>
        /// Taille de tableau maximale pour les opérations nécessitant des créations de tableaux d'octets (historique,
        /// presse-papiers,...).
        /// C'est un entier 64 bits, mais en pratique pour l'instant compris comme un 32 bits signé.
        /// </summary>
        [Browsable(false)]
        public long MaxSupportedArrayLength { get => maxSupportedArrayLength; set => maxSupportedArrayLength = value; }
    }
}
