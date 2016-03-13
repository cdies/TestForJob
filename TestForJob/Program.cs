using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Xml.Serialization;

namespace TestForJob
{
    class Program
    {
        static void Main(string[] args)
        {
            const string Command = "git log --no-walk --tags --pretty=\"%H %d %ai\" --decorate=full";
            string FilesSavePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            const string LocalProjGit = @"C:\Users\MID\preact";

            var startInfo = new ProcessStartInfo
            {
                WorkingDirectory = LocalProjGit,
                FileName = "cmd.exe",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            var proc = Process.Start(startInfo);

            proc.StandardInput.WriteLine(Command);
            proc.StandardInput.WriteLine("exit");

            string DataForParsing = proc.StandardOutput.ReadToEnd();

            proc.WaitForExit();

            List<data> TmpArrayForParsing = new List<data>();

            string[] lines = DataForParsing.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

            lines = lines.Skip(4).ToArray();

            foreach (var item in lines)
            {
                data tmp = new data();

                tmp.hash = RegexUtil.MatchKey(item, 0);
                tmp.ver = RegexUtil.MatchKey(item, 1);
                tmp.date = RegexUtil.MatchKey(item, 2);

                TmpArrayForParsing.Add(tmp);

            }

            XML_Serialize(TmpArrayForParsing, FilesSavePath);

            Json_Serialize(TmpArrayForParsing, FilesSavePath);

            Console.WriteLine("Press key...");
            Console.ReadLine();
        }

        private static void Json_Serialize(List<data> ar, string mydocpath)
        {
            string json = JsonConvert.SerializeObject(ar);

            using (StreamWriter outputFile = new StreamWriter(mydocpath + @"\Json_Data.xml"))
            {
                outputFile.WriteLine(json);
            }
        }

        private static void XML_Serialize(List<data> ar, string mydocpath)
        {
            XmlSerializer serializer = new XmlSerializer(ar.GetType());
            using (StreamWriter writer = new StreamWriter(mydocpath + @"\XML_Data.xml"))
            {
                serializer.Serialize(writer, ar);
            }
        }
    }

    [DataContract]
    public class data
    {
        [DataMember, XmlAttribute]
        public string date { get; set; }
        [DataMember, XmlAttribute]
        public string ver { get; set; }
        [DataMember, XmlAttribute]
        public string hash { get; set; }
    }

    static class RegexUtil
    {
        static string patt = @"([a-z0-9\-]+)\ ";
        static string patt2 = @"(?<=/tags/)(.*?)(\)|\,)";
        static string patt3 = @"(.{20})(\-0.00)";
        static Regex _regex;
        static public string MatchKey(string input, int n)
        {
            switch (n)
            {
                case 0: _regex = new Regex(patt);  break;
                case 1: _regex = new Regex(patt2); break;
                case 2: _regex = new Regex(patt3); break;

                default:
                    throw new Exception("Use just 1 or 2 or 3!");
            }
            

            Match match = _regex.Match(input);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return null;
            }
        }
    }
}
