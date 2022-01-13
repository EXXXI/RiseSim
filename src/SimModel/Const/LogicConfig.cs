using Csv;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimModel.Const
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
            try
            {
                return int.Parse(str);
            }
            catch (FormatException)
            {
                return def;
            }
        }
    }
}
