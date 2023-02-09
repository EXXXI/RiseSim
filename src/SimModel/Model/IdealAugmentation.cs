using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimModel.Model
{
    // 理想錬成装備
    public class IdealAugmentation
    {
        // 管理用装備名(GUID)
        public string Name { get; set; } = string.Empty;

        // 表示用装備名
        public string DispName { get; set; } = string.Empty;

        // テーブル
        public int Table { get; set; }

        // 下位テーブル含フラグ
        public bool IsIncludeLower { get; set; }

        // スロット追加数
        public int SlotIncrement { get; set; }

        // TODO: 防御・耐性は需要があれば
        /* 
        // 防御力増減
        public int Def { get; set; }

        // 火耐性増減
        public int Fire { get; set; }

        // 水耐性増減
        public int Water { get; set; }

        // 雷耐性増減
        public int Thunder { get; set; }

        // 氷耐性増減
        public int Ice { get; set; }

        // 龍耐性増減
        public int Dragon { get; set; }
        */

        // 追加スキル
        public List<Skill> Skills { get; set; } = new();

        // 各コストごとの追加可能スキル数
        public int[] GenericSkills { get; set; } = new int[5];

        // スキルマイナス位置(2Lvマイナスしたい場合は複数登録する)
        // 0:どこかがマイナス
        // n(n>0):n番目のスキルがマイナス
        public List<int> SkillMinuses { get; set; } = new();

        // 1部位だけか否か
        // true: 1部位のみ、false: 全部位可能
        public bool IsOne { get; set; }

        // 検索時に有効か無効か
        public bool IsEnabled { get; set; } = true;

        // 表示用スキル一覧
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

        // 表示用スキルマイナス一覧
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
