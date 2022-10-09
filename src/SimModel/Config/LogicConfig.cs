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
using SimModel.Domain;
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

        // 傀異錬成防具のスキル数の設定下限(泣シミュとの連携用)
        private const int MinAugmentationSkillCount = 5;

        // 最近使ったスキルの記憶容量
        public int MaxRecentSkillCount { get; set; }

        // 防具のスキル最大個数
        public int MaxEquipSkillCount { get; set; }

        // 装飾品のスキル最大個数
        public int MaxDecoSkillCount { get; set; }

        // 護石のスキル最大個数
        public int MaxCharmSkillCount { get; set; }

        // 傀異錬成防具のスキル最大個数(設定値)
        public int MaxAugmentationSkillCount { get; set; }

        // 傀異錬成防具のスキル最大個数(実際の値)
        // 設定値を超える数のスキル情報がCSVにあった場合にそれに合わせて変更される
        public int MaxAugmentationSkillCountActual { get; set; }

        // 最大並列処理数
        public int MaxDegreeOfParallelism { get; set; }

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
                MaxRecentSkillCount = ParseUtil.LoadConfigItem(line, @"最近使ったスキルの記憶容量", 20);
                MaxEquipSkillCount = ParseUtil.LoadConfigItem(line, @"防具のスキル最大個数", 5);
                MaxDecoSkillCount = ParseUtil.LoadConfigItem(line, @"装飾品のスキル最大個数", 2);
                MaxCharmSkillCount = ParseUtil.LoadConfigItem(line, @"護石のスキル最大個数", 2);
                MaxAugmentationSkillCount = ParseUtil.LoadConfigItem(line, @"傀異錬成防具のスキル最大個数", 7);
                if (MaxAugmentationSkillCount < MinAugmentationSkillCount)
                {
                    MaxAugmentationSkillCount = MinAugmentationSkillCount;
                }
                MaxAugmentationSkillCountActual = MaxAugmentationSkillCount;
                MaxDegreeOfParallelism = ParseUtil.LoadConfigItem(line, @"最大並列処理数", 4);
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
    }
}
