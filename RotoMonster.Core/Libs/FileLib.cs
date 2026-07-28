using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RotoMonster.Core.Libs
{
    public class FileLib
    {
        public string GetDebugFilename(string filenameTemplate, string sport, string fileType, string userId, string leagueId, string extenstion)
        {
            string o = filenameTemplate;
            o = o.Replace("{sport}", sport);
            o = o.Replace("{filetype}", fileType);
            o = o.Replace("{userid}", userId);
            if (leagueId.Length > 0)
                o = o.Replace("{league}", leagueId);
            else
                o = o.Replace("_{league}", "");
            o = o.Replace("{extension}", extenstion);

            return o;
        }

        public bool WriteData(string filename, string data)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filename))
                {
                    writer.Write(data);
                    writer.Close();
                }

                return true;
            }
            catch
            {
            }

            return false;
        }

        public bool WriteData(IConfiguration config, string fileType, string userId, string leagueId, string extenstion, string data)
        {
            if (config == null)
                return false;

            string debugPathVariable = "DebugPath";
            string useDebugFilesVariable = "UseDebugFiles";

            string sport = (string)config["sport"].ToLower();

            if (data.Length > 0 && config != null && config[debugPathVariable] != null)
            {
                string fileTemplate = (string)config[debugPathVariable];
                if (fileTemplate.Length > 0 && config[useDebugFilesVariable] != null && (string)config[useDebugFilesVariable] == "1")
                {
                    return WriteData(GetDebugFilename(fileTemplate, sport, fileType, userId, leagueId, extenstion), data);
                }
            }

            return false;
        }

        public string GetData(string startText, string endText, ref int pos, string data)
        {
            if (data.IndexOf(startText, pos) == -1)
                return "";

            int startIndex = data.IndexOf(startText, pos) + startText.Length;
            int endIndex = data.IndexOf(endText, startIndex + 1);

            if (startIndex != 0 & endIndex > startIndex)
            {
                pos = endIndex;
                return data.Substring(startIndex, endIndex - startIndex).Trim();
            }
            else
            {
                return "";
            }
        }

        public string GetDateParameter(DateTime date)
        {
            return String.Format("{0}-{1}-{2}", date.Month, date.Day, date.Year);
        }


    }
}
