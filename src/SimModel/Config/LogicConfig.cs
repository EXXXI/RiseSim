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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimModel.Config
{
    public class LogicConfig
    {
        // インスタンス
        static private LogicConfig instance;

        // ロジック設定ファイル
        private const string ConfCsv = "conf/logicConfig.csv";

        // 最近使ったスキルの記憶容量
        public int MaxRecentSkillCount { get; set; }

        // 防具のスキル最大個数
        public int MaxEquipSkillCount { get; set; }

        // 装飾品のスキル最大個数
        public int MaxDecoSkillCount { get; set; }

        // 護石のスキル最大個数
        public int MaxCharmSkillCount { get; set; }

        // TODO: 変更可能にするならCSVあたりもいじる必要あり
        // 傀異錬成防具のスキル最大個数
        public int MaxAugmentationSkillCount { get; set; } = 4;

        // 風雷合一
        public string FuraiName { get; } = "風雷合一";

        // マイセットのデフォルト名
        public string DefaultMySetName { get; } = "マイセット";

        // プライベートコンストラクタ
        private LogicConfig()
        {
            string csv = File.ReadAllText(ConfCsv);

            foreach (ICsvLine line in CsvReader.ReadFromText(csv))
            {
                MaxRecentSkillCount = Parse(line[@"最近使ったスキルの記憶容量"], 20);
                MaxEquipSkillCount = Parse(line[@"防具のスキル最大個数"], 5);
                MaxDecoSkillCount = Parse(line[@"装飾品のスキル最大個数"], 2);
                MaxCharmSkillCount = Parse(line[@"護石のスキル最大個数"], 2);
            }
        }

        // インスタンス
        static public LogicConfig Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new LogicConfig();
                }
                return instance;
            }
        }

        // int.Parseを実行
        // 失敗した場合は指定したデフォルト値として扱う
        static private int Parse(string str, int def)
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
    }
}
