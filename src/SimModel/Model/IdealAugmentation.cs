using System.Collections.Generic;
using System.Text;

namespace SimModel.Model
{
    /// <summary>
    /// 理想錬成装備
    /// </summary>
    public class IdealAugmentation
    {
        /// <summary>
        /// 管理用装備名(GUID)
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 表示用装備名
        /// </summary>
        public string DispName { get; set; } = string.Empty;

        /// <summary>
        /// テーブル
        /// </summary>
        public int Table { get; set; }

        /// <summary>
        /// 下位テーブル含フラグ
        /// </summary>
        public bool IsIncludeLower { get; set; }

        /// <summary>
        /// スロット追加数
        /// </summary>
        public int SlotIncrement { get; set; }

        /// <summary>
        /// 追加スキル
        /// </summary>
        public List<Skill> Skills { get; set; } = new();

        /// <summary>
        /// 各コストごとの追加可能スキル数
        /// </summary>
        public int[] GenericSkills { get; set; } = new int[5];

        /// <summary>
        /// スキルマイナス位置(2Lvマイナスしたい場合は複数登録する)
        /// 0:どこかがマイナス
        /// n(n>0):n番目のスキルがマイナス
        /// </summary>
        public List<int> SkillMinuses { get; set; } = new();

        /// <summary>
        /// 1部位だけか否か
        /// true: 1部位のみ、false: 全部位可能
        /// </summary>
        public bool IsOne { get; set; }

        /// <summary>
        /// 検索時に有効か無効か
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 検索時に必須か
        /// </summary>
        public bool IsRequired { get; set; } = false;

        /// <summary>
        /// 表示用スキル一覧
        /// </summary>
        public string SimpleSkillDiscription
        {
            get
            {
                bool isFirst = true;
                StringBuilder sb = new StringBuilder();
                foreach (var skill in Skills)
                {
                    if (!isFirst)
                    {
                        sb.Append(", ");
                    }
                    isFirst = false;
                    sb.Append(skill.Description);
                }
                for (int i = 0; i < 5; i++)
                {
                    if (GenericSkills[i] > 0)
                    {
                        if (!isFirst)
                        {
                            sb.Append(", ");
                        }
                        isFirst = false;
                        sb.Append("c" + ((i * 3) + 3) + "スキル +" + GenericSkills[i]);
                    }

                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// 表示用スキルマイナス一覧
        /// </summary>
        public string SimpleSkillMinusDiscription
        {
            get
            {
                bool isFirst = true;
                StringBuilder sb = new StringBuilder();
                foreach (var index in SkillMinuses)
                {
                    if (!isFirst)
                    {
                        sb.Append(", ");
                    }
                    isFirst = false;

                    if (index == 0)
                    {
                        sb.Append("どれか1つ");
                    }
                    else
                    {
                        sb.Append(index + "番目");
                    }
                }
                return sb.ToString();
            }
        }
    }
}
