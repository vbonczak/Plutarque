using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Plutarque
{
    public class Utils
    {
        /// <summary>
        /// Indique la base maximale qui pourra donner un résultata affichable
        /// dans les fonctions telles que ToBaseString(...)
        /// </summary>
        /// <returns></returns>
        public static int GetMaxSupportedBase()
        {
            return alph.Length;
        }

        public static string alph = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static StringFormat format = new StringFormat()
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.None,
            FormatFlags = StringFormatFlags.NoWrap
        };
        public static Color GetForeColorFromBackColor(Color clr)
        {
            //Idée de Gacek : https://stackoverflow.com/questions/1855884/determine-font-color-based-on-background-color
            double lum = (0.299 * clr.R + 0.587 * clr.G + 0.114 * clr.B) / 255;

            if (lum > 0.5)
                return Color.Black;
            else
                return Color.White;
        }
        public static Brush defBr = new SolidBrush(SystemColors.WindowText);
        public static Brush selBr = new SolidBrush(SystemColors.HighlightText);
        public static Brush selBackBr = new SolidBrush(SystemColors.Highlight);
        public static Brush repBr = new SolidBrush(Color.Aqua);
        public static HatchBrush delBr = new HatchBrush(HatchStyle.BackwardDiagonal, Color.Red, Color.Transparent);
        /// <summary>
        /// Retourne la longueur maximale de la représentation d'un octet en base b (le nombre de chiffres)
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int GetMaxLengthOfByte(int b)
        {
            return (int)Math.Ceiling(Math.Log(255, b));
        }

        public static string ToBaseString(double n, int b)
        {
            return n < 0 ? ToBaseString((long)n, b) : ToBaseString((ulong)n, b);
        }

        /// <summary>
        /// Fournit la représentation sous forme de chaîne de caractères
        /// de l'octet spécifié avec la base indiquée.
        /// </summary>
        /// <param name="n">Octet à convertir</param>
        /// <param name="b">Base de numération</param>
        /// <returns></returns>
        public static string ToBaseString(byte n, int b)
        {
            if (b > 0)
            {
                int cur = Math.Abs(n);
                int rem;
                string ret = "";
                while (cur > 0)
                {
                    cur = Math.DivRem(cur, b, out rem);
                    ret = alph[rem] + ret;
                }
                if (n < 0)
                    ret = "-" + ret;
                return ret.PadLeft(GetMaxLengthOfByte(b), '0');
            }
            else
            {
                //ascii latin1
                return new string((char)Math.Max((byte)32, n), 1);
            }
        }

        /// <summary>
        /// Fournit la représentation sous forme de chaîne de caractères
        /// de l'entier spécifié avec la base indiquée.
        /// </summary>
        /// <param name="n">Octet à convertir</param>
        /// <param name="b">Base de numération</param>
        /// <returns></returns>
        public static string ToBaseString(long n, int b)
        {
            long cur = Math.Abs(n);
            long rem = 0;
            string ret = "";
            while (cur > 0)
            {
                cur = Math.DivRem(cur, b, out rem);
                ret = alph[(int)(rem % alph.Length)] + ret;
            }
            if (n < 0)
                ret = "-" + ret;
            return ret.PadLeft(GetMaxLengthOfByte(b) * GetNbOfBytes(n), '0');
        }

        /// <summary>
        /// Retourne une chaîne dans la base de numération indiquée.
        /// </summary>
        /// <param name="b">Base, -1 pour l'ASCII</param>
        /// <param name="array">Le tableau d'où tirer une représentation textuelle.</param>
        /// <param name="lineLength">Longueur de la ligne, 0 pour une ligne non limitée.</param>
        /// <returns></returns>
        public static string ArrayToString(int b, byte[] array, int lineLength = 0)
        {
            string ret = "";
            if (lineLength == 0)
                foreach (byte item in array)
                {
                    ret += ToBaseString(item, b) + (b > 0 ? " " : "");
                }
            else
                for (int i = 0; i < array.Length; i += lineLength)
                {
                    for (int j = i; j < array.Length && j < i + lineLength; j++)
                    {
                        byte item = array[i];
                        ret += ToBaseString(item, b) + (b > 0 ? " " : "");
                    }

                    ret += "\r\n";
                }

            return ret;
        }

        /// <summary>
        /// Fournit la représentation sous forme de chaîne de caractères
        /// de l'entier spécifié avec la base indiquée.
        /// </summary>
        /// <param name="n">Octet à convertir</param>
        /// <param name="b">Base de numération</param>
        /// <returns></returns>
        public static string ToBaseString(ulong n, int b)
        {
            ulong cur = n;
            ulong rem = 0;
            string ret = "";
            while (cur > 0)
            {
                cur = DivRem(cur, (ulong)b, out rem);
                ret = alph[((int)rem % alph.Length)] + ret;
            }

            if (n < 0)
                ret = "-" + ret;
            return ret.PadLeft(GetMaxLengthOfByte(b) * GetNbOfBytes(n), '0');
        }

        public static ulong DivRem(ulong a, ulong b, out ulong result)
        {
            result = a % b;
            return a / b;
        }

        public static int GetNbOfBytes(long n)
        {
            return n == 0 ? 1 : (int)Math.Log(n, 256.0);
        }

        public static int GetNbOfBytes(double n)
        {
            return n == 0 ? 1 : (int)Math.Log(n, 256.0);
        }

        /*public static double ToBigNum(object o)
        {
            double d = 0;
            if (double.TryParse(o.ToString(), out d))
            {
                return d;
            }
            else
                return 0;
        }*/

        /// <summary>
        /// Convertit la chaîne de caractères spécifiée (UTF8) en bloc d'octets.
        /// </summary>
        /// <param name="str">Chaîne de caractères encodée en UTF8</param>
        /// <returns>Bloc d'octets correspondant</returns>
        public static byte[] StrToBytes(string str)
        {
            if (str == null)
            {
                byte[] b = new byte[0];
                return b;
            }
            else
                return System.Text.Encoding.UTF8.GetBytes(str);
        }

        /// <summary>
        /// Retourne la chaîne de caractères UTF8 correspondant au bloc d'octets spécifié.
        /// </summary>
        /// <param name="data">Bloc d'octets encodant une chaîne de caractères UTF8</param>
        /// <returns>Chaîne de caractères correspondante</returns>
        public static string BytesToStr(byte[] data)
        {
            return new string(System.Text.Encoding.UTF8.GetChars(data));
        }

        /// <summary>
        /// Convertit la chaîne de caractères spécifiée (Unicode) en bloc d'octets.
        /// </summary>
        /// <param name="str">Chaîne de caractères encodée en Unicode</param>
        /// <returns>Bloc d'octets correspondant</returns>
        public static byte[] UniStrToBytes(string str)
        {
            if (str == null)
            {
                byte[] b = new byte[0];
                return b;
            }
            else
                return System.Text.Encoding.Unicode.GetBytes(str);
        }

        /// <summary>
        /// Retourne la chaîne de caractères Unicode correspondant au bloc d'octets spécifié.
        /// </summary>
        /// <param name="data">Bloc d'octets encodant une chaîne de caractères Unicode</param>
        /// <returns>Chaîne de caractères correspondante</returns>
        public static string BytesToUniStr(byte[] data)
        {
            return new string(System.Text.Encoding.Unicode.GetChars(data));
        }

        /// <summary>
        /// Convertit un entier 64 bits non signé en bloc d'octets
        /// </summary>
        /// <param name="nbr">Entier 64 bits</param>
        /// <returns>Bloc d'octets</returns>
        public static byte[] ULongToBytes(ulong nbr)
        {
            return BitConverter.GetBytes(nbr);
        }

        /// <summary>
        /// Convertit un entier 32 bits non signé en bloc d'octets
        /// </summary>
        /// <param name="nbr">Entier 32 bits</param>
        /// <returns>Bloc d'octets</returns>
        public static byte[] UInt32ToBytes(uint nbr)
        {
            return BitConverter.GetBytes(nbr);
        }

        /// <summary>
        /// Convertit un entier 32 bits signé en bloc d'octets
        /// </summary>
        /// <param name="nbr">Entier 32 bits</param>
        /// <returns>Bloc d'octets</returns>
        public static byte[] IntToBytes(int nbr)
        {
            return BitConverter.GetBytes(nbr);
        }

        /// <summary>
        /// Convertit une couleur en un bloc d'octet équivalent à la valeur ARGB 32 bits.
        /// </summary>
        /// <param name="clr">Couleur à convertir</param>
        /// <returns>Bloc d'octets</returns>
        public static byte[] ColorToBytes(Color clr)
        {
            return BitConverter.GetBytes(clr.ToArgb());
        }

        /// <summary>
        /// Convertit un entier 16 bits non signé en bloc d'octets
        /// </summary>
        /// <param name="nbr">Entier 16 bits</param>
        /// <returns>Bloc d'octets</returns>
        public static byte[] UShortToBytes(ushort nbr)
        {
            return BitConverter.GetBytes(nbr);
        }

        /// <summary>
        /// Convertit un entier 64 bits signé en bloc d'octets
        /// </summary>
        /// <param name="nbr">Entier 64 bits</param>
        /// <returns>Bloc d'octets</returns>
        public static byte[] LongToBytes(long nbr)
        {
            return BitConverter.GetBytes(nbr);
        }

        /// <summary>
        /// Convertit une date/heure en bloc d'octets
        /// </summary>
        /// <param name="nbr">Objet DateTime à convertir</param>
        /// <returns>Bloc d'octets</returns>
        public static byte[] DateToBytes(DateTime dT)
        {
            return LongToBytes(dT.ToBinary());
        }

        /// <summary>
        /// Convertit le bloc d'octet spécifié en date/heure
        /// </summary>
        /// <param name="nbr">Bloc d'octets (64 bits)</param>
        /// <returns>Objet DateTime correspondant</returns>
        public static DateTime BytesToDate(byte[] b)
        {
            return DateTime.FromBinary(BytesToLong(b));
        }

        /// <summary>
        /// Modes d'édition dans un RTFControl
        /// </summary>
        public enum TextMode : byte
        {
            RawText = 0x10,
            RTF = 0x11,
            HTML = 0x12
        }

        /// <summary>
        /// Retourne l'octet correspondant à la verion spécifiée.
        /// </summary>
        /// <param name="majeure">Composante majeure</param>
        /// <param name="mineure">Composante mineure</param>
        /// <returns>Octet de version</returns>
        /// <remarks>Notez que la composante mineure comporte le poids le plus faible.</remarks>
        public static byte GetVersion(byte majeure, byte mineure)
        {
            return (byte)(majeure * (byte)0x10 + mineure);
        }

        /// <summary>
        /// Convertit le bloc d'octets spécifié en entier 32 bits non signé.
        /// </summary>
        /// <param name="data">Bloc d'octets à convertir</param>
        /// <returns>Entier 32 bits non signé</returns>
        public static uint BytesToUInteger(byte[] data)
        {
            return BitConverter.ToUInt32(data, 0);
        }

        /// <summary>
        /// Convertit le bloc d'octets spécifié en entier 32 bits signé.
        /// </summary>
        /// <param name="data">Bloc d'octets à convertir</param>
        /// <returns>Entier 32 bits signé</returns>
        public static int BytesToInteger(byte[] data)
        {
            return BitConverter.ToInt32(data, 0);
        }

        /// <summary>
        /// Convertit le bloc d'octets spécifié en couleur.
        /// </summary>
        /// <param name="data">Bloc d'octets à convertir</param>
        /// <returns>Couleur correspondante</returns>
        public static Color BytesToColor(byte[] data)
        {
            return Color.FromArgb(BitConverter.ToInt32(data, 0));
        }

        /// <summary>
        /// Convertit le bloc d'octets spécifié en entier 64 bits non signé.
        /// </summary>
        /// <param name="data">Bloc d'octets à convertir</param>
        /// <returns>Entier 64 bits non signé</returns>
        public static ulong BytesToULong(byte[] data)
        {
            return BitConverter.ToUInt64(data, 0);
        }

        /// <summary>
        /// Convertit le bloc d'octets spécifié en entier 16 bits non signé.
        /// </summary>
        /// <param name="data">Bloc d'octets à convertir</param>
        /// <returns>Entier 16 bits non signé</returns>
        public static ushort BytesToUShort(byte[] data)
        {
            return BitConverter.ToUInt16(data, 0);
        }

        /// <summary>
        /// Convertit le bloc d'octets spécifié en entier 16 bits signé.
        /// </summary>
        /// <param name="data">Bloc d'octets à convertir</param>
        /// <returns>Entier 16 bits signé</returns>
        public static short BytesToShort(byte[] data)
        {
            return BitConverter.ToInt16(data, 0);
        }

        /// <summary>
        /// Convertit le bloc d'octets spécifié en entier 64 bits signé.
        /// </summary>
        /// <param name="data">Bloc d'octets à convertir</param>
        /// <returns>Entier 64 bits signé</returns>
        public static long BytesToLong(byte[] data)
        {
            return BitConverter.ToInt64(data, 0);
        }

        /// <summary>
        /// Retourne la date/heure correspondant au bloc d'octets spécifié.
        /// </summary>
        /// <param name="data">Bloc d'octets (64 bits) à convertir</param>
        /// <returns>Objet DateTime</returns>
        public static DateTime DateFromBytes(byte[] data)
        {
            return DateTime.FromBinary(BytesToLong(data));
        }
    }
}
