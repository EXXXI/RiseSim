using Csv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimModel.Domain
{
    public static class ParseUtil
    {
        // int.Parseを実行
        // 失敗した場合は0として扱う
        static public int Parse(string str)
        {
            return Parse(str, 0);
        }

        // int.Parseを実行
        // 失敗した場合は指定したデフォルト値として扱う
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

        // Configの各項目を読み込み
        // 読み込み失敗時はdefの値を利用
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

        // Configの各項目を読み込み
        // 読み込み失敗時はdefの値を利用
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
