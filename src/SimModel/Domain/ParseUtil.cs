/*    RiseSim : MHRise skill simurator for Windows
 *    Copyright (C) 2022  EXXXI
 *
 *    This program is free software: you can redistribute it and/or modify
 *    it under the terms of the GNU General Public License as published by
 *    the Free Software Foundation, either version 3 of the License, or
 *    (at your option) any later version.
 *
 *    This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 *    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *    GNU General Public License for more details.
 *
 *    You should have received a copy of the GNU General Public License
 *    along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
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
