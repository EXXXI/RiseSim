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

        // スキルのカテゴリ
        public string Category { get; init; }

        // コンストラクタ
        public Skill(string name, int level, bool isAdditional = false) : this(name, level, "", isAdditional) { }

        public Skill(string name, int level, string category, bool isAdditional = false)
        {
            Name = name;
            Level = level;
            IsAdditional = isAdditional;
            Category = string.IsNullOrEmpty(category) ? @"未分類" : category;
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
