using System;
using System.Drawing;
using System.Xml.Serialization;

namespace Plutarque
{
    /// <summary>
    /// Permet de sérializer les couleurs.
    /// Merci à https://stackoverflow.com/a/4322461
    /// </summary>
    public class XmlColor
    {
        private Color clr = Color.Black;

        public XmlColor() { }
        public XmlColor(Color c) { clr = c; }


        public Color ToColor()
        {
            return clr;
        }

        public void FromColor(Color c)
        {
            clr = c;
        }

        public static implicit operator Color(XmlColor x)
        {
            return x.ToColor();
        }

        public static implicit operator XmlColor(Color c)
        {
            return new XmlColor(c);
        }

        [XmlAttribute]
        public string Web
        {
            get { return ColorTranslator.ToHtml(clr); }
            set
            {
                try
                {
                    if (Alpha == 0xFF) // preserve named color value if possible
                        clr = ColorTranslator.FromHtml(value);
                    else
                        clr = Color.FromArgb(Alpha, ColorTranslator.FromHtml(value));
                }
                catch (Exception)
                {
                    clr = Color.Black;
                }
            }
        }

        [XmlAttribute]
        public byte Alpha
        {
            get { return clr.A; }
            set
            {
                if (value != clr.A) // avoid hammering named color if no alpha change
                    clr = Color.FromArgb(value, clr);
            }
        }

        public bool ShouldSerializeAlpha() { return Alpha < 0xFF; }
    }
}