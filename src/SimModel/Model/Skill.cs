using System.Collections.Generic;
using System.Linq;

namespace SimModel.Model
{
    /// <summary>
    /// スキル
    /// </summary>
    public record Skill
    {

        /// <summary>
        /// スキル名
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// スキルレベル
        /// </summary>
        public int Level { get; set; } = 0;

        /// <summary>
        /// 追加スキルフラグ
        /// </summary>
        public bool IsAdditional { get; init; } = false;

        /// <summary>
        /// 固定検索フラグ
        /// </summary>
        public bool IsFixed { get; set; } = false;

        /// <summary>
        /// スキルのカテゴリ
        /// </summary>
        public string Category { get; init; }

        /// <summary>
        /// シリーズスキル等、レベルに特殊な名称がある場合ここに格納
        /// </summary>
        public Dictionary<int, string> SpecificNames { get; } = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="name">スキル名</param>
        /// <param name="level">レベル</param>
        /// <param name="isAdditional">追加スキルかどうかのフラグ</param>
        /// <param name="isFixed">固定検索フラグ</param>
        public Skill(string name, int level, bool isAdditional = false, bool isFixed = false) 
            : this(name, level, Masters.Skills.Where(s => s.Name == name).FirstOrDefault()?.Category, isAdditional, isFixed) { }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="name">スキル名</param>
        /// <param name="level">レベル</param>
        /// <param name="category">カテゴリ</param>
        /// <param name="isAdditional">追加スキルかどうかのフラグ</param>
        /// <param name="isFixed">固定検索フラグ</param>
        public Skill(string name, int level, string? category, bool isAdditional = false, bool isFixed = false)
        {
            Name = name;
            Level = level;
            IsAdditional = isAdditional;
            IsFixed = isFixed;
            Category = string.IsNullOrEmpty(category) ? @"未分類" : category;
        }

        /// <summary>
        /// 最大レベル
        /// マスタに存在しないスキルの場合0
        /// </summary>
        public int MaxLevel {
            get 
            {
                return Masters.SkillMaxLevel(Name);
            }
        }

        /// <summary>
        /// 表示用文字列
        /// </summary>
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
    }
}
