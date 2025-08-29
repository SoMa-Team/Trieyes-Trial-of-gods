using System.Collections.Generic;

namespace Utils
{

    public static class CsvUtils
    {
        /// <summary>
        /// 한 줄의 CSV를 컬럼별로 안전하게 분리합니다.
        /// 따옴표 안의 쉼표, 이중 따옴표 모두 처리합니다.
        /// </summary>
        public static List<string> SplitCsvLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            int i = 0;
            string cur = "";

            while (i < line.Length)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        cur += '"';
                        i++; // skip one
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(cur);
                    cur = "";
                }
                else
                {
                    cur += c;
                }

                i++;
            }

            result.Add(cur);
            return result;
        }
    }
}