using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Drawing;
using System.Xml.Serialization;
using System.IO;
using static System.Math;
using System.Windows.Forms;
using System.Drawing.Text;

namespace Plutarque
{
    public partial class DataView
    {
        /// <summary>
        /// Les différentes zones d'affichage du contrôle
        /// </summary>
        public enum SubZone
        {
            /// <summary>
            /// Le panneau de gauche
            /// </summary>
            LeftPanel, 
            /// <summary>
            /// Le panneau de droite
            /// </summary>
            RightPanel, 
            /// <summary>
            /// Marge de séparation du milieu
            /// </summary>
            MiddleMargin, 
            /// <summary>
            /// Marge de gauche indiquant le décalage des lignes
            /// </summary>
            OffsetMargin, 
            /// <summary>
            /// En dehors du contrôle.
            /// </summary>
            OutsideBorders
        }
        public enum ReturnModeConvention
        {
            crlf, lf, cr
        }

        /// <summary>
        /// Un repère de texte dans les données.
        /// </summary>
        public struct Repere
        {
            private string title;
            private Color color;
            private long position;
            private Brush clr;

            [XmlElement(Type = typeof(XmlColor))]
            public Color Color
            {
                get => color; set
                {
                    color = value;
                    clr = new SolidBrush(color);
                }
            }

            public string Title { get => title; set => title = value; }
            public long Position { get => position; set => position = value; }

            public Repere(string title, Color color, long position)
            {
                this.title = title;
                this.color = color;
                this.position = position;
                clr = new SolidBrush(color);
            }

            public void Draw(Graphics g, Rectangle r/*, bool focused = false // moins simple : il faut savoir où le dessiner...*/)
            {
                g.FillRectangle(clr, r.X, r.Y, (int)(0.1f * r.Width), r.Height);
            }

            /// <summary>
            /// Retourne une liste de structures Repere contenues dans le fichier spécifié.
            /// </summary>
            /// <param name="file"></param>
            /// <returns></returns>
            public static List<Repere> LoadFromFile(string file)
            {
                XmlSerializer x = new XmlSerializer(typeof(List<Repere>), new XmlRootAttribute("Reperes"));
                StreamReader r = new StreamReader(file);
                List<Repere> l = x.Deserialize(r) as List<Repere>;
                r.Close();
                if (l != null)
                {
                    return l;
                }
                else
                    throw new FormatException("Le fichier ne possède pas le bon format XML.");
            }

            /// <summary>
            /// Enregistre la liste de repères dans le fichier spécifié.
            /// </summary>
            /// <param name="file"></param>
            /// <param name="l"></param>
            public static void Save(string file, List<Repere> l)
            {
                XmlSerializer x = new XmlSerializer(typeof(List<Repere>), new XmlRootAttribute("Reperes"));

                StreamWriter s = new StreamWriter(file);
                x.Serialize(s, l);
                s.Close();
            }
        }


    }

    /// <summary>
    /// Classe permettant la double mise en mémoire tampon de l'objet Panel.
    /// </summary>
    public class TextZone : Panel
    {
        public TextZone()
        {
            DoubleBuffered = true;
        }
    }
}
