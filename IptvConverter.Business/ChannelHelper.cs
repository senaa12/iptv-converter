using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IptvConverter.Business
{
    public static class ChannelHelper
    {
        public static string ExtractExtInf(string line)
        {
            string[] split = line.Split(',');
            return split[0].Replace("#EXTINF:", "").Split(' ')[0];
        }

        public static string ExtractEpgId(string line)
        {
            return getChannelProperty(line, "tvg-id");
        }

        public static string ExtractLogo(string line)
        {
            return getChannelProperty(line, "tvg-logo");
        }

        public static string ExtractGroup(string line)
        {
            return getChannelProperty(line, "group-title");
        }

        public static bool IsHdChannel(string name)
        {
            return name.ToLower().Split(' ').Contains("hd") || name.ToLower().Split(' ').Contains("fhd");
        }

        public static string ExtractProgramName(string line)
        {
            string[] split = line.Split(',');
            split = split[split.Length - 1].Split(':');
            split = split[split.Length - 1].Trim(' ').Split('|');
            return split[split.Length - 1].Trim(' ');
        }

        /// <summary>
        /// reads channel properties from playlist
        /// </summary>
        /// <param name="line"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private static string getChannelProperty(string line, string propertyName)
        {
            var fullPropertyName = $" {propertyName}=";
            var startIndex = line.IndexOf(fullPropertyName);
            if (startIndex < 0)
                return null;

            return readPropertyValue(line, startIndex + fullPropertyName.Length);
        }

        /// <summary>
        /// start index is last index before '"'
        /// function reads string until closing '"'
        /// </summary>
        /// <param name="line"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        private static string readPropertyValue(string line, int startIndex)
        {
            var chars = new List<char>();
            var startingQuotes = line[startIndex];
            if (startingQuotes != '"')
                throw new Exception("Error while parsing");

            startIndex++;
            while (line[startIndex] != '"')
            {
                chars.Add(line[startIndex]);
                startIndex++;
            }

            var sb = new StringBuilder();
            foreach (var c in chars)
            {
                sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
