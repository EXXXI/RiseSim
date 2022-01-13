using Csv;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiseSim.Const
{
    internal class ViewConfig
    {
        // インスタンス
        static private ViewConfig instance;

        // 画面設定ファイル
        private const string ConfCsv = "conf/viewConfig.csv";

        // スキル選択部品の個数
        public int SkillSelectorCount { get; set; }

        // スロットの最大の大きさ
        public int MaxSlotSize { get; set; }

        // デフォルトの頑張り度
        public string DefaultLimit { get; set; }

        // スキル未選択時の表示
        public string NoSkillName { get; set; }

        // プライベートコンストラクタ
        private ViewConfig()
        {
            string csv = File.ReadAllText(ConfCsv);

            foreach (ICsvLine line in CsvReader.ReadFromText(csv))
            {
                SkillSelectorCount = Parse(line[@"スキル選択部品の個数"], 15);
                MaxSlotSize = Parse(line[@"スロットの最大の大きさ"], 4);
                DefaultLimit = Parse(line[@"デフォルトの頑張り度"], 100).ToString();
                NoSkillName = line[@"スキル未選択時の表示"];
            }
        }

        // インスタンス
        static public ViewConfig Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ViewConfig();
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
