using Csv;
using System;

namespace SimModel.Domain
{
    /// <summary>
    /// Parse系のUtilクラス
    /// </summary>
    public static class ParseUtil
    {
        /// <summary>
        /// int.Parseを実行
        /// </summary>
        /// <param name="str">Parse対象</param>
        /// <returns>Parse結果、ただし、失敗した場合は0</returns>
        static public int Parse(string str)
        {
            return Parse(str, 0);
        }

        /// <summary>
        /// int.Parseを実行
        /// </summary>
        /// <param name="str">Parse対象</param>
        /// <param name="def">失敗時の値</param>
        /// <returns>Parse結果、ただし、失敗した場合は指定した失敗時の値</returns>
        static public int Parse(string str, int def)
        {
            if (int.TryParse(str, out int num))
            {
                return num;
            }
            else
            {
                return def;
            }
        }

        /// <summary>
        /// Configの項目を読み込み
        /// </summary>
        /// <param name="line">CsvLine</param>
        /// <param name="columnName">項目名</param>
        /// <param name="def">読み込み失敗時の値(int)</param>
        /// <returns>CSVから読み込んだ値、ただし、読み込み失敗時はdefの値を利用</returns>
        static public int LoadConfigItem(ICsvLine line, string columnName, int def)
        {
            try
            {
                return Parse(line[columnName], def);
            }
            catch (Exception)
            {
                return def;
            }
        }

        /// <summary>
        /// Configの項目を読み込み
        /// </summary>
        /// <param name="line">CsvLine</param>
        /// <param name="columnName">項目名</param>
        /// <param name="def">読み込み失敗時の値(string)</param>
        /// <returns>CSVから読み込んだ値、ただし、読み込み失敗時はdefの値を利用</returns>
        static public string LoadConfigItem(ICsvLine line, string columnName, string def)
        {
            try
            {
                return line[columnName];
            }
            catch (Exception)
            {
                return def;
            }
        }
    }
}
