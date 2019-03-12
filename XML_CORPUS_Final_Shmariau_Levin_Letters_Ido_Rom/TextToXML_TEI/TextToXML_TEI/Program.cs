using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

/// <summary>
// TextToXML_TEI – A Program to analyze text and convert it to XML-TEI
// By Ido Rom
/// </summary>

namespace TextToXML_TEI
{
    class Program
    {
        private static Regex ToRX = null;
        private static Regex DateRX = null;
        private static Regex FromRX = null;

        static void Main(string[] args)
        {
            string SrcFile;
            string DstFile;

            InitRegEx();

            if (args.Length<1)
            {
                MessageBox.Show("Missing source file path argument\r\nUasge: TextToXML_TEI \"Source File path\"", "TextToXML_TEI"
                    , MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SrcFile = args[0];
            if (!System.IO.File.Exists(SrcFile))
            {
                MessageBox.Show("Could not find the file:\r\n" + SrcFile, "TextToXML_TEI"
                        , MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DstFile = DstenationFilePath(SrcFile);
            
            ReadSourceFile(SrcFile, DstFile);

            MessageBox.Show("Conversion finished successfully", "TextToXML_TEI"
                        , MessageBoxButtons.OK, MessageBoxIcon.Information);
            
        }


        //  DstenationFilePath - return destination file path from the source file path
        //  By replacing the extension with ".xml"

        private static string DstenationFilePath(string srcPath)
        {
            string dst = System.IO.Path.GetDirectoryName(srcPath);
            if (!dst.EndsWith("\\")) dst += "\\";
            dst = dst + System.IO.Path.GetFileNameWithoutExtension(srcPath);
            dst += ".xml";

            return dst;
        }


        // InitRegEx - Initialize the regular expressions used in the program
        // This is done using a function because the REGEX patterns are complicated
        // and contain a mix English and Hebrew characters, that need to be in exact order

        private static void InitRegEx()
        {
            string toPattern = @"^((אל )|(ל))";
            toPattern += @"(?<RecipientName>(" + HebWord() + "( +" + HebWord() + ")*(,( +" + HebWord() + ")+)??))";
            toPattern += @"(,(?<RecipientLocation>( +" + HebWord() + @")+))?\.?$";
            ToRX = new Regex(toPattern, RegexOptions.IgnoreCase);

            string datePatern = @"^ *";
            datePatern += @"((?<SentFrom>" + HebWord() + "( +" + HebWord() + @")*) *,?( *(?<Date>(\d\d?(\-|–))?(?<day>\d\d?),? (?<month>" + HebWord() + @") ((?<year>\d+)))))|";
            datePatern += @"((?<Date>(?<day>\d\d?) (?<month>" + HebWord() + @") (?<year>\d+))) *, *(?<SentFrom>" + HebWord() + "( +" + HebWord() + @")*)|";
            datePatern += @"((?<Date>" + HebWord() + @" +" + @"[א-ת][א-ת][א-ת]" + @"""[א-ת]) *(, *(?<SentFrom>" + HebWord() + @"))?" + @"\.*)|";
            datePatern += @"((?<SentFrom>" + HebWord() + "( +" + HebWord() + @")*) *,? *(?<Date>(?<day>\d+)\.(?<month>\d+)\.(?<year>\d+)))";
            datePatern += @" *$";
            DateRX = new Regex(datePatern);

            string fromPatern = @"((?<From>(שמריה הלוי)" + @"(( לעווין)?" + @"|" + @"( לוין)?)" + @"( לעווין)?)|";
            fromPatern += @"(?<From>(ש\. +)?" + @"(לוין)))";
            fromPatern += @" *$";
            FromRX = new Regex(fromPatern);
        }


        // HebWord - return REGEX pattern for Hebrew word
        private static string HebWord()
        {
            string pt = "(([א-ת]" + @"[\-'’""“”א-ת]*" + @"[’'א-ת])|([א-ת]\.))";
            return pt;
        }

        // ReadSourceFile - Read the text from the source file, split to separate letters 
        // and write output to XML-TEI file
        private static void ReadSourceFile(string srcFileName, string DstFileName)
        {
            Match m;

            string LetterText = "";
            string RecipientName = "";
            string RecipientLocation = "";
            string SentFrom = "";
            string DateSent = "";
            string SenderName = "";
            string SentDateStr = "";

            int NumOfLetters = 0;

            string OutputXml = XMLTop();
            SenderName = " ";

            // open the source file for reading
            using (StreamReader streamReader = File.OpenText(srcFileName))
            {
                string Line;
                // loop through each line in the source file
                while ((Line = streamReader.ReadLine()) != null)
                {
                    string s = Line;
                    Line = Line.Replace("#", "").Replace("$", "");

                    // check for "To" line
                    m = ToRX.Match(Line);
                    if (m.Success && !string.IsNullOrEmpty(SenderName))
                    {
                        // check if new letter starts in this line
                        if (!string.IsNullOrEmpty(LetterText))
                        {
                            NumOfLetters++;

                            OutputXml += OneLetterXML(NumOfLetters, SenderName, SentFrom, DateSent, RecipientName, RecipientLocation, LetterText, SentDateStr);

                            // reset metadata for next letter
                            LetterText = "";
                            RecipientName = "";
                            RecipientLocation = "";
                            SentFrom = "";
                            DateSent = "";
                            SenderName = "";
                            SentDateStr = "";
                        }

                        RecipientName = m.Groups["RecipientName"].Value.Trim();
                        RecipientLocation = m.Groups["RecipientLocation"].Value.Trim();
                        LetterText = Line;
                    }
                    else LetterText += "\r\n                        " + Line;

                    // Check for "Date" line
                    m = DateRX.Match(Line);
                    if (m.Success && string.IsNullOrEmpty(SentFrom))
                    {
                        SentFrom = m.Groups["SentFrom"].Value.Trim();
                        DateSent = m.Groups["Date"].Value.Trim();

                        string ds = m.Groups["day"].Value.Trim();
                        string ms = m.Groups["month"].Value.Trim();
                        string ys = m.Groups["year"].Value.Trim();
                        SentDateStr = ToDate(ds, ms, ys);
                    }                   

                    // check for "From" line
                    m = FromRX.Match(Line);
                    if (m.Success)
                    {
                        SenderName = m.Groups["From"].Value.Trim();
                    }

                }

                // Write last letter to XML
                if (!string.IsNullOrEmpty(RecipientName))
                {
                    NumOfLetters++;
                    OutputXml += OneLetterXML(NumOfLetters, SenderName, SentFrom, DateSent, RecipientName, RecipientLocation, LetterText, SentDateStr);
                }

                OutputXml += "</teiCorpus>";
                System.IO.File.WriteAllText(DstFileName, OutputXml);
            }
        }


        // ToDate - Convert date as it appears in the text to date format used in XML-TEI
        private static string ToDate(string d, string m, string y)
        {
            int day;
            int month;
            int year;

            if (!int.TryParse(d, out day)) day = 0;
            if (!int.TryParse(y, out year)) year = 0;
            if (year > 0 && year < 100) year += 1900;

            if (m.StartsWith("ב")) m = m.Substring(1);
            if (!int.TryParse(m, out month))
            {
                switch (m)
                {
                    case "ינואר":
                        month = 1;
                        break;
                    case "פברואר":
                        month = 2;
                        break;
                    case "מרץ":
                        month = 3;
                        break;
                    case "אפריל":
                        month = 4;
                        break;
                    case "מאי":
                        month = 5;
                        break;
                    case "יוני":
                        month = 6;
                        break;
                    case "יולי":
                        month = 7;
                        break;
                    case "אוגוסט":
                        month = 8;
                        break;
                    case "ספטמבר":
                        month = 9;
                        break;
                    case "אוקטובר":
                        month = 10;
                        break;
                    case "נובמבר":
                        month = 11;
                        break;
                    case "דצמבר":
                        month = 12;
                        break;
                    default:
                        month = 0;
                        break;

                }
            }

            if (day != 0 && month != 0 && year != 0) return year.ToString("0000") + "-" + month.ToString("00") + "-" + day.ToString("00");

            return "";
        }


        // OneLetterXML -Returns the XML of one letter section of the XML-TEI
        private static string OneLetterXML(int ID, string SenderName, string SentFrom, string DateSent, string RecipientName, string RecipientLocation, string LetterText, string SentDateStr)
        {
            string s = "    <TEI xml:id=\"Levin" + ID.ToString("000") + "\">\r\n";

            s += "        <teiHeader>\r\n";
            s += Hdr();
            s += "            <profileDesc>\r\n";
            s += "                <correspDesc>\r\n";

            s += "                    <correspAction type = \"sent\">\r\n";
            s += "                        <persName>שמריהו לוין</persName>\r\n";
            s += "                        <placeName>" + SentFrom + "</placeName>\r\n";
            s += "                        <date";
            if (!string.IsNullOrEmpty(SentDateStr)) s += " when=\"" + SentDateStr + "\"";
            s += ">" + DateSent + "</date>\r\n";
            s += "                    </correspAction >\r\n";

            s += "                    <correspAction type=\"received\" >\r\n";
            s += "                        <persName>" + RecipientName + "</persName>\r\n";
            s += "                        <placeName cert=\"medium\">" + RecipientLocation + "</placeName>\r\n";
            s += "                    </correspAction>\r\n";


            s += "                </correspDesc>\r\n";
            s += "            </profileDesc>\r\n";
            s += "        </teiHeader>\r\n";
            s += "        <text>\r\n";
            s += "            <body>\r\n";
            s += "                <div><p>\r\n";

            s += LetterText;

            s += "                        \r\n</p>" + "<closer><signed>" + SenderName + "</signed></closer>" + "</div>\r\n";
            s += "            </body>\r\n";
            s += "        </text>\r\n";
            s += "    </TEI>\r\n";

            return s;
        }


        // XMLTop - Returns the top part of the XML-TEI
        private static string XMLTop()
        {
            string s = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n";
            s += "<?xml-model href=\"http://www.tei-c.org/release/xml/tei/custom/schema/relaxng/tei_corpus.rng\" type=\"application/xml\" schematypens=\"http://relaxng.org/ns/structure/1.0\"?>\r\n";
            s += "<?xml-model href=\"http://www.tei-c.org/release/xml/tei/custom/schema/relaxng/tei_corpus.rng\" type=\"application/xml\" schematypens=\"http://purl.oclc.org/dsdl/schematron\"?>\r\n";
            s += "<teiCorpus xmlns=\"http://www.tei-c.org/ns/1.0\">\r\n";
            s += "    <teiHeader>\r\n";
            s += "        <fileDesc>\r\n";
            s += "            <titleStmt>\r\n";
            s += "                <title>title of corpus</title>\r\n";
            s += "                <author>author </author>\r\n";
            s += "            </titleStmt>\r\n";
            s += "            <publicationStmt>\r\n";
            s += "                <p>Publication Information </p>\r\n";
            s += "            </publicationStmt>\r\n";
            s += "            <sourceDesc>\r\n";
            s += "                <p>Information about the source </p>\r\n";
            s += "            </sourceDesc>\r\n";
            s += "        </fileDesc>\r\n";
            s += "    </teiHeader>\r\n";

            return s;
        }


        // Hdr - Returns the top part of one letter section of the XML-TEI
        private static string Hdr()
        {
            string s = "            <fileDesc>\r\n";
            s += "                <titleStmt>\r\n";
            s += "                    <title/>\r\n";
            s += "                    <author><persName>שמריהו לוין</persName></author>\r\n";
            s += "                </titleStmt>\r\n";
            s += "                <publicationStmt>\r\n";
            s += "                     <p>תויג על ידי עידו רום על בסיס המהדורה האלקטרונית: \"אגרות שמריהו לוין:\r\n";
            s += "                        פסיעות ראשונות\". פרויקט בן-יהודה. אוחזר בתאריך 2018-12-03\r\n";
            s = s.Replace("2018-12-03", DateTime.Now.ToString("yyyy-MM-dd"));
            s += "                        (http://bybe.benyehuda.org/read/8366)</p>\r\n";
            s += "                </publicationStmt>\r\n";
            s += "                <sourceDesc>\r\n";
            s += "                    <biblStruct>\r\n";
            s += "                        <monogr>\r\n";
            s += "                            <author/>\r\n";
            s += "                            <title>אגרות שמריהו לוין: מבחר</title>\r\n";
            s += "                            <imprint>\r\n";
            s += "                                <publisher>דביר</publisher>\r\n";
            s += "                                <date/>\r\n";
            s += "                                <pubPlace>תל אביב</pubPlace>\r\n";
            s += "                            </imprint>\r\n";
            s += "                        </monogr>\r\n";
            s += "                    </biblStruct>\r\n";
            s += "                </sourceDesc>\r\n";
            s += "            </fileDesc>\r\n";

            return s;
        }
    }
}
