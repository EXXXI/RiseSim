using Csv;
using SimModel.Domain;
using SimModel.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiseSim.Config
{
    /// <summary>
    /// View関連の設定
    /// </summary>
    internal class ViewConfig
    {
        /// <summary>
        /// インスタンス
        /// </summary>
        static private ViewConfig instance;

        /// <summary>
        /// 画面設定ファイル
        /// </summary>
        private const string ConfCsv = "conf/viewConfig.csv";

        /// <summary>
        /// スロットの最大の大きさ
        /// </summary>
        public int MaxSlotSize { get; set; }

        /// <summary>
        /// デフォルトの頑張り度
        /// </summary>
        public string DefaultLimit { get; set; }

        /// <summary>
        /// スキル未選択時の表示
        /// </summary>
        public string NoSkillName { get; set; }

        /// <summary>
        /// 性別の初期値
        /// </summary>
        public Sex DefaultSex { get; set; }

        /// <summary>
        /// グリッドの列順保存有無
        /// </summary>
        public bool UseSavedColumnIndexes { get; set; }


        /// <summary>
        /// プライベートコンストラクタ
        /// </summary>
        private ViewConfig()
        {
            string csv = File.ReadAllText(ConfCsv);

            foreach (ICsvLine line in CsvReader.ReadFromText(csv))
            {
                MaxSlotSize = ParseUtil.LoadConfigItem(line, @"スロットの最大の大きさ", 4);
                DefaultLimit = ParseUtil.LoadConfigItem(line, @"デフォルトの頑張り度", 100).ToString();
                NoSkillName = ParseUtil.LoadConfigItem(line, @"スキル未選択時の表示", @"スキル選択");
                Sex defSex = ParseUtil.LoadConfigItem(line, @"性別の初期値", @"男性").StrToSex();
                if (defSex == Sex.all)
                {
                    // 指定が不正な場合、ひとまず男性とする
                    defSex = Sex.male;
                }
                DefaultSex = defSex;
                UseSavedColumnIndexes = ParseUtil.LoadConfigItem(line, @"グリッドの列順保存有無", @"有").Equals(@"有");
            }
        }

        /// <summary>
        /// インスタンス
        /// </summary>
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
    }
}
