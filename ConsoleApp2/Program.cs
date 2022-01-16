using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.*", SearchOption.AllDirectories);
            // Getting Files
            Console.WriteLine("Getting Files");
            foreach (string file in files)
            {
                if (file.Contains(".nut") || file.Contains(".kwt"))
                {
                    string fileName = Path.GetFullPath(file);
                    Console.WriteLine("Processing " + fileName);

                    BinaryReader br = new BinaryReader(File.OpenRead(fileName));
                    string format = null;
                    string length = null;
                    string replaceText = null;
                    string encryptedText = null;
                    for (int i = 0; i <= 1; i++)
                    // Get Format
                    {
                        br.BaseStream.Position = i;
                        format += br.ReadByte().ToString("X2");
                    }
                    if (format == "FBFB")
                    {
                        // Get Length String
                        for (int i = 2; i <= 5; i++)
                        {
                            br.BaseStream.Position = i;
                            length += br.ReadByte().ToString("X2");
                        }

                        int lengthConvert = int.Parse(length, System.Globalization.NumberStyles.HexNumber);
                        lengthConvert = lengthConvert + 8;

                        Console.WriteLine("Getting Text Value");
                        for (int i = lengthConvert; i < br.BaseStream.Length; i++)
                        {
                            br.BaseStream.Position = i;
                            replaceText += br.ReadByte().ToString("X2");
                        }

                        replaceText = Regex.Replace(replaceText, @"(.{2})", "$1 ");

                        // Get Text Value
                        //09 = tab line
                        List<string> textList = new List<string>(replaceText.Split(new string[] { "00" }, StringSplitOptions.None));
                        var textArray = textList.ToArray();
                        //Console.WriteLine(textArray.Length); //207
                        //Console.WriteLine(textArray[0]);
                        //Console.WriteLine(textArray[205]);

                        // Get Encrypted Value
                        string test = "";
                        Console.WriteLine("Getting Encrypted Value");
                        int loop = 8;
                        while(loop < lengthConvert)
                        {
                            string temp = "";
                            br.BaseStream.Position = loop;
                            string currentByte = br.ReadByte().ToString("X2");
                            int current = loop;
                            switch (currentByte)
                            {
                                case "80":
                                    for (int x = current; x <= current + 2; x++)
                                    {
                                        br.BaseStream.Position = x;
                                        temp += br.ReadByte().ToString("X2");
                                    }
                                    int index = temp.IndexOf("80");
                                    string intValue = temp.Substring(index, 6);
                                    string intfix = intValue.Replace("80", "");
                                    intfix = intfix.Replace(" ", "");
                                    int intint = int.Parse(intfix, System.Globalization.NumberStyles.HexNumber);
                                    encryptedText += textArray[intint].TrimStart();
                                    loop = loop + 3;
                                    break;
                                case "81":
                                    for (int x = current; x <= current + 2; x++)
                                    {
                                        br.BaseStream.Position = x;
                                        temp += br.ReadByte().ToString("X2");
                                    }
                                    int index2 = temp.IndexOf("81");
                                    string intValue2 = temp.Substring(index2, 6);
                                    string intfix2 = intValue2.Replace("81", "");
                                    intfix2 = intfix2.Replace(" ", "");
                                    int intint2 = int.Parse(intfix2, System.Globalization.NumberStyles.HexNumber);
                                    encryptedText += "22 " + textArray[intint2].TrimStart() + "22 ";
                                    loop = loop + 3;
                                    break;
                                case "82":
                                    for (int x = current; x <= current + 4; x++)
                                    {
                                        br.BaseStream.Position = x;
                                        temp += br.ReadByte().ToString("X2");
                                    }
                                    int index3 = temp.IndexOf("82");
                                    string intValue3 = temp.Substring(index3, 10);
                                    string intfix3 = intValue3.Replace("82", "");
                                    intfix3 = intfix3.Replace(" ", "");
                                    int intint3 = int.Parse(intfix3, System.Globalization.NumberStyles.HexNumber);
                                    byte[] intbyte = Encoding.UTF8.GetBytes(intint3.ToString());
                                    var intvar = BitConverter.ToString(intbyte);
                                    intvar = intvar.Replace("-", "");
                                    intvar = Regex.Replace(intvar, @"(.{2})", "$1 ");
                                    encryptedText += intvar.TrimEnd();
                                    loop = loop + 5;
                                    break;
                                case "83":
                                    for (int x = current; x <= current + 4; x++)
                                    {
                                        br.BaseStream.Position = x;
                                        temp += br.ReadByte().ToString("X2");
                                    }
                                    int index4 = temp.IndexOf("83");
                                    string intValue4 = temp.Substring(index4, 10);
                                    string intfix4 = intValue4.Replace("83", "");
                                    intfix4 = intfix4.Replace(" ", "");
                                    uint num = uint.Parse(intfix4, System.Globalization.NumberStyles.AllowHexSpecifier);
                                    byte[] floatVals = BitConverter.GetBytes(num);
                                    float f = BitConverter.ToSingle(floatVals, 0);
                                    int fLength = f.ToString().Length - f.ToString().IndexOf(".") - 1;
                                    byte[] intbyte2;
                                    switch (fLength)
                                    {
                                        case 1:
                                            intbyte2 = Encoding.UTF8.GetBytes(f.ToString("0.00"));
                                            break;
                                        case 2:
                                            intbyte2 = Encoding.UTF8.GetBytes(f.ToString("0.00"));
                                            break;
                                        default:
                                            intbyte2 = Encoding.UTF8.GetBytes(f.ToString("0.000"));
                                            break;
                                    }
                                    var intvar2 = BitConverter.ToString(intbyte2);
                                    intvar2 = intvar2.Replace("-", "");
                                    intvar2 = Regex.Replace(intvar2, @"(.{2})", "$1 ");
                                    encryptedText += intvar2.TrimEnd();
                                    loop = loop + 5;
                                    break;
                                case "84": //delegate
                                    encryptedText += "20 64 65 6C 65 67 61 74 65 20";
                                    loop++;
                                    break;
                                case "85": //delete
                                    encryptedText += "64 65 6C 65 74 65";
                                    loop++;
                                    break;
                                case "86": //==
                                    encryptedText += "20 3D 3D 20";
                                    loop++;
                                    break;
                                case "87": //!=
                                    encryptedText += "20 21 3D 20";
                                    loop++;
                                    break;
                                case "88": //<=
                                    encryptedText += "20 3C 3D 20";
                                    loop++;
                                    break;
                                case "89": //>=
                                    encryptedText += "20 3E 3D 20";
                                    loop++;
                                    break;
                                case "8A": //switch
                                    encryptedText += "73 77 69 74 63 68 20";
                                    loop++;
                                    break;
                                //encryptedText = encryptedText.Replace("8B", ""); //??? (Arrow) {NC}
                                case "8C": //&&
                                    encryptedText += "20 26 26 20";
                                    loop++;
                                    break;
                                case "8D": //||
                                    encryptedText += "20 7C 7C 20";
                                    loop++;
                                    break;
                                case "8E": //if
                                    encryptedText += "69 66 20";
                                    loop++;
                                    break;
                                case "8F": //else
                                    encryptedText += "20 65 6C 73 65 20";
                                    loop++;
                                    break;
                                case "90": //while
                                    encryptedText += "77 68 69 6C 65 20";
                                    loop++;
                                    break;
                                case "91": //break
                                    encryptedText += "62 72 65 61 6B";
                                    loop++;
                                    break;
                                case "92": //for
                                    encryptedText += "66 6F 72 20";
                                    loop++;
                                    break;
                                case "93": //do
                                    encryptedText += "64 6F 20";
                                    loop++;
                                    break;
                                case "94": //null
                                    encryptedText += "6E 75 6C 6C";
                                    loop++;
                                    break;
                                case "95": //foreach
                                    encryptedText += "66 6F 72 65 61 63 68 20";
                                    loop++;
                                    break;
                                case "96": //in
                                    encryptedText += "20 69 6E 20";
                                    loop++;
                                    break;
                                case "97": //<-
                                    encryptedText += "20 3C 2D 20";
                                    loop++;
                                    break;
                                //encryptedText = encryptedText.Replace("98", "20 25 20"); //% (Modulo) {NC}
                                case "99": //local
                                    encryptedText += "6C 6F 63 61 6C 20";
                                    loop++;
                                    break;
                                case "9A": //clone
                                    encryptedText += "63 6C 6F 6E 65 20";
                                    loop++;
                                    break;
                                case "9B": //function
                                    encryptedText += "66 75 6E 63 74 69 6F 6E 20";
                                    loop++;
                                    break;
                                case "9C": //return
                                    encryptedText += "72 65 74 75 72 6E 20";
                                    loop++;
                                    break;
                                case "9D": //typeof
                                    encryptedText += "74 79 70 65 6F 66 20";
                                    loop++;
                                    break;
                                //encryptedText = encryptedText.Replace("9E", ""); //??? (UMINUS) {NC}
                                case "9F": //+=
                                    encryptedText += "20 2B 3D 20";
                                    loop++;
                                    break;
                                case "A0": //-=
                                    encryptedText += "20 2D 3D 20";
                                    loop++;
                                    break;
                                case "A1": //continue
                                    encryptedText += "63 6F 6E 74 69 6E 75 65";
                                    loop++;
                                    break;
                                case "A2": //yield
                                    encryptedText += "79 69 65 6C 64 20";
                                    loop++;
                                    break;
                                case "A3": //try
                                    encryptedText += "74 72 79 20";
                                    loop++;
                                    break;
                                case "A4": //catch
                                    encryptedText += "20 63 61 74 63 68 20";
                                    loop++;
                                    break;
                                case "A5": //throw
                                    encryptedText += "74 68 72 6F 77 20";
                                    loop++;
                                    break;
                                case "A6": //<<
                                    encryptedText += "20 3C 3C 20";
                                    loop++;
                                    break;
                                case "A7": //>>
                                    encryptedText += "20 3E 3E 20";
                                    loop++;
                                    break;
                                case "A8": //resume
                                    encryptedText += "72 65 73 75 6D 65";
                                    loop++;
                                    break;
                                case "A9": //::
                                    encryptedText += "20 3A 3A";
                                    loop++;
                                    break;
                                case "AA": //case
                                    encryptedText += "63 61 73 65 20";
                                    loop++;
                                    break;
                                case "AB": //default
                                    encryptedText += "64 65 66 61 75 6C 74";
                                    loop++;
                                    break;
                                case "AC": //this
                                    encryptedText += "74 68 69 73";
                                    loop++;
                                    break;
                                case "AD": //++
                                    encryptedText += "2B 2B";
                                    loop++;
                                    break;
                                case "AE": //--
                                    encryptedText += "2D 2D";
                                    loop++;
                                    break;
                                case "AF": //parent
                                    encryptedText += "70 61 72 65 6E 74";
                                    loop++;
                                    break;
                                case "B0": //>>>
                                    encryptedText += "20 3E 3E 3E 20";
                                    loop++;
                                    break;
                                case "B1": //class
                                    encryptedText += "63 6C 61 73 73 20";
                                    loop++;
                                    break;
                                case "B2": //extends
                                    encryptedText += "20 65 78 74 65 6E 64 73 20";
                                    loop++;
                                    break;
                                //encryptedText = encryptedText.Replace("B3", ""); //??? {NC}
                                case "B4": //constructor + 2bytes ? No Idea
                                    encryptedText += "63 6F 6E 73 74 72 75 63 74 6F 72";
                                    loop = loop + 3;
                                    break;
                                case "B5": //instanceof
                                    encryptedText += "69 6E 73 74 61 6E 63 65 6F 66 20";
                                    loop++;
                                    break;
                                case "B6": //...
                                    encryptedText += "2E 2E 2E";
                                    loop++;
                                    break;
                                case "B7": //vargc
                                    encryptedText += "76 61 72 67 63";
                                    loop++;
                                    break;
                                case "B8": //vargv
                                    encryptedText += "76 61 72 67 76";
                                    loop++;
                                    break;
                                //encryptedText = encryptedText.Replace("B8", ""); //??? (___FILE___) {NC}
                                case "B9": //true
                                    encryptedText += "74 72 75 65";
                                    loop++;
                                    break;
                                case "BA": //false
                                    encryptedText += "66 61 6C 73 65";
                                    loop++;
                                    break;
                                case "BB": //*=
                                    encryptedText += "20 2A 3D 20";
                                    loop++;
                                    break;
                                case "BC": ///=
                                    encryptedText += "20 2F 3D 20";
                                    loop++;
                                    break;
                                case "BD": //%=
                                    encryptedText += "20 25 3D 20";
                                    loop++;
                                    break;
                                //encryptedText = encryptedText.Replace("BE", ""); //??? (ATTR_OPEN) {NC}
                                //encryptedText = encryptedText.Replace("BF", ""); //??? (ATTR_CLOSE) {NC}
                                case "C8": //</
                                    encryptedText += "20 3C 2F";
                                    loop++;
                                    break;
                                case "C9": ///>
                                    encryptedText += "2F 3E 20";
                                    loop++;
                                    break;
                                case "CA": //static
                                    encryptedText += "73 74 61 74 69 63 20";
                                    loop++;
                                    break;
                                case "CB": //enum
                                    encryptedText += "65 6E 75 6D 20";
                                    loop++;
                                    break;
                                case "CC": //const
                                    encryptedText += "63 6F 6E 73 74 20";
                                    loop++;
                                    break;
                                case "CD": //super
                                    encryptedText += "73 75 70 65 72";
                                    loop++;
                                    break;
                                case "CE": //|=
                                    encryptedText += "20 7C 3D 20";
                                    loop++;
                                    break;
                                case "CF": //&=
                                    encryptedText += "20 26 3D 20";
                                    loop++;
                                    break;
                                case "D0": //strict
                                    encryptedText += "73 74 72 69 63 74 20";
                                    loop++;
                                    break;
                                case "D1": //removei
                                    encryptedText += "72 65 6D 6F 76 65 69";
                                    loop++;
                                    break;
                                case "FD": //newline
                                    encryptedText += "0D 0A 09";
                                    loop++;
                                    break;
                                case "FE": //multiline
                                    for (int x = current; x <= current + 1; x++)
                                    {
                                        br.BaseStream.Position = x;
                                        temp += br.ReadByte().ToString("X2");
                                    }
                                    int index5 = temp.IndexOf("FE");
                                    string intValue5 = temp.Substring(index5, 4);
                                    string intfix5 = intValue5.Replace("FE", "");
                                    intfix5 = intfix5.Replace(" ", "");
                                    int intint5 = int.Parse(intfix5, System.Globalization.NumberStyles.HexNumber);
                                    for (int x = 0; x < intint5; x++)
                                    {
                                        encryptedText += "0D 0A 09 ";
                                    }
                                    encryptedText.TrimEnd();
                                    loop = loop + 2;
                                    break;
                                default:
                                    encryptedText += currentByte;
                                    loop++;
                                    break;
                            }
                        }
                        encryptedText = encryptedText.Replace(" ", "");
                        encryptedText = Regex.Replace(encryptedText, @"(.{2})", "$1 ").TrimEnd();

                        // Hex -> String
                        encryptedText = encryptedText.Replace(" ", "");
                        encryptedText = "EFBBBF" + encryptedText;
                        //File.WriteAllText("hex_" + fileName, encryptedText);
                        byte[] rawText = new byte[encryptedText.Length / 2];
                        for (int i = 0; i < rawText.Length; i++)
                        {
                            rawText[i] = Convert.ToByte(encryptedText.Substring(i * 2, 2), 16);
                        }
                        encryptedText = Encoding.UTF8.GetString(rawText);
                        br.Close();

                        encryptedText = encryptedText.Replace("return ;", "return;");
                        encryptedText = encryptedText.Replace("){", ") {");
                        encryptedText = encryptedText.Replace("}e", "} e");
                        encryptedText = encryptedText.Replace("}c", "} c");
                        encryptedText = encryptedText.Replace("}w", "} w");
                        encryptedText = encryptedText.Replace(")c", ") c");
                        encryptedText = encryptedText.Replace(")r", ") r");
                        encryptedText = encryptedText.Replace(")e", ") e");
                        encryptedText = encryptedText.Replace(")t", ") t");
                        encryptedText = encryptedText.Replace(")b", ") b");
                        encryptedText = encryptedText.Replace(")>", ") > ");
                        encryptedText = encryptedText.Replace("):", ") : ");
                        encryptedText = encryptedText.Replace("<", " < ");
                        encryptedText = encryptedText.Replace("< =", "<=");
                        encryptedText = encryptedText.Replace("= ", " = ");
                        encryptedText = encryptedText.Replace("= =", "==");
                        encryptedText = encryptedText.Replace("?", " ? ");
                        encryptedText = encryptedText.Replace("\"+", "\" + ");
                        encryptedText = encryptedText.Replace("+\"", " + \"");
                        encryptedText = Regex.Replace(encryptedText, @"(,{1})", "$1 ");
                        encryptedText = encryptedText.Replace("( :", "(:");
                        encryptedText = encryptedText.Replace("< -", "<-");
                        encryptedText = encryptedText.Replace("+ =", "+=");
                        encryptedText = encryptedText.Replace("- =", "-=");
                        encryptedText = encryptedText.Replace("> =", ">=");
                        encryptedText = encryptedText.Replace("< =", "<=");
                        encryptedText = encryptedText.Replace("! =", "!=");
                        encryptedText = encryptedText.Replace("| =", "|=");
                        encryptedText = encryptedText.Replace("+ +", "++");
                        encryptedText = encryptedText.Replace("< <", "<<");
                        encryptedText = encryptedText.Replace("! ::", "!::");
                        encryptedText = encryptedText.Replace(";b", "; b");
                        encryptedText = encryptedText.Replace(";i", "; i");
                        encryptedText = encryptedText.Replace(";l", "; l");
                        encryptedText = encryptedText.Replace("instanceof", " instanceof");
                        //encryptedText = encryptedText.Replace(" ::", "::");
                        encryptedText = encryptedText.Replace("  ", " ");
                        encryptedText = encryptedText.Replace(", " + Environment.NewLine, "," + Environment.NewLine);

                        File.WriteAllText(fileName, encryptedText);
                        Console.WriteLine(fileName + " Done" + Environment.NewLine);
                    }
                    else
                    {
                        Console.WriteLine("Not Encrypt Onigiri File, Skip " + fileName + Environment.NewLine);
                    }
                }
            }
            MessageBox.Show("Decrypting Files Done...", "Prompt", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            Console.WriteLine("Decrypting Files Done...");
            //Console.ReadKey();
        }
    }
}
