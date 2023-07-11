using System;

namespace SimModel.Model
{
    // スキル
    public record Skill
    {

        // スキル名
        public string Name { get; }

        // スキルレベル
        public int Level { get; set; } = 0;

        // 追加スキルフラグ
        public bool IsAdditional { get; init; } = false;

        // 固定検索フラグ
        public bool IsFixed { get; set; } = false;

        // スキルのカテゴリ
        public string Category { get; init; }

        // コンストラクタ
        public Skill(string name, int level, bool isAdditional = false, bool isFixed = false) : this(name, level, "", isAdditional, isFixed) { }

        public Skill(string name, int level, string category, bool isAdditional = false, bool isFixed = false)
        {
            Name = name;
            Level = level;
            IsAdditional = isAdditional;
            IsFixed = isFixed;
            Category = string.IsNullOrEmpty(category) ? @"未分類" : category;
        }

        // 最大レベル
        // マスタに存在しないスキルの場合0
        public int MaxLevel {
            get 
            {
                return Masters.SkillMaxLevel(Name);
            }
        }

        // 表示用文字列
        public string Description
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Name) || Level == 0)
                {
                    return string.Empty;
                }

                return (IsAdditional ? "(追加)" : string.Empty) + Name + "Lv" + Level;
            }
        }

        /// <summary>
        /// SkillPickerSelectorViewでComboBoxの表示に使う文字列を返す
        /// </summary>
        public string PickerSelectorDisplayName => Level switch
        {
            0 => Name,
            _ => $"{Name}Lv{Level}"
        };
    }
}
