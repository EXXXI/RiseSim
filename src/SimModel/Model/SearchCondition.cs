using SimModel.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimModel.Model
{
    // 検索条件
    public class SearchCondition
    {
        // スキルリスト
        public List<Skill> Skills { get; set; } = new();

        // 武器スロ1つ目
        public int WeaponSlot1 { get; set; }

        // 武器スロ2つ目
        public int WeaponSlot2 { get; set; }

        // 武器スロ3つ目
        public int WeaponSlot3 { get; set; }

        // 防御力
        public int? Def { get; set; }

        // 火耐性
        public int? Fire { get; set; }

        // 水耐性
        public int? Water { get; set; }

        // 雷耐性
        public int? Thunder { get; set; }

        // 氷耐性
        public int? Ice { get; set; }

        // 龍耐性
        public int? Dragon { get; set; }

        // 性別
        public Sex Sex { get; set; }

        // 理想錬成を利用するか否か
        public bool IncludeIdealAugmentation { get; set; }

        // 通常装備を優先するか否か
        public bool PrioritizeNoIdeal { get; set; }

        // 既存装備で組める場合を除外するか否か
        public bool ExcludeAbstract { get; set; }

        // マイ検索条件保存用ID
        public string ID { get; set; }

        // マイ検索条件保存用名前
        public string DispName { get; set; }

        // 錬成再計算用 部位固定情報
        public SortedDictionary<string, int>? AdditionalFixData { get; set; } = null;

        // CSV用スキル形式
        public string SkillCSV
        {
            get
            {
                StringBuilder sb = new();
                bool isFirst = true;
                foreach (var skill in Skills)
                {
                    if (!isFirst)
                    {
                        sb.Append(',');
                    }
                    sb.Append(skill.Name);
                    sb.Append(',');
                    sb.Append(skill.Level);
                    if (skill.IsFixed)
                    {
                        sb.Append("固定");
                    }
                    isFirst = false;
                }
                return sb.ToString();
            }
            set
            {
                Skills = new List<Skill>();
                string[] splitted = value.Split(',');
                for (int i = 0; i < splitted.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(splitted[i]))
                    {
                        return;
                    }
                    string name = splitted[i];
                    string levelStr = splitted[++i];
                    bool isFixed = levelStr.EndsWith("固定");
                    levelStr = levelStr.Replace("固定", string.Empty);
                    Skill skill = new(name, ParseUtil.Parse(levelStr));
                    skill.IsFixed = isFixed;
                    Skills.Add(skill);
                }
            }
        }

        // デフォルトコンストラクタ
        public SearchCondition()
        {
        }

        // コピーコンストラクタ
        public SearchCondition(SearchCondition condition)
        {
            Skills = new List<Skill>();
            foreach (var skill in condition.Skills)
            {
                Skill newSkill = new Skill(skill.Name, skill.Level, skill.IsAdditional, skill.IsFixed);
                Skills.Add(newSkill);
            }
            WeaponSlot1 = condition.WeaponSlot1;
            WeaponSlot2 = condition.WeaponSlot2;
            WeaponSlot3 = condition.WeaponSlot3;
            Def = condition.Def;
            Fire = condition.Fire;
            Water = condition.Water;
            Thunder = condition.Thunder;
            Ice = condition.Ice;
            Dragon = condition.Dragon;
            Sex = condition.Sex;
            IncludeIdealAugmentation = condition.IncludeIdealAugmentation;
            PrioritizeNoIdeal = condition.PrioritizeNoIdeal;
            ExcludeAbstract = condition.ExcludeAbstract;
        }

        // スキル追加(同名スキルはレベルが高い方のみを採用、固定がある場合は固定が優先)
        // 追加したスキルが有効かどうかを返す
        public bool AddSkill(Skill additionalSkill)
        {
            foreach (var skill in Skills)
            {
                if(skill.Name == additionalSkill.Name)
                {
                    // 固定フラグに差がある場合それを優先
                    if (!skill.IsFixed && additionalSkill.IsFixed)
                    {
                        skill.Level = additionalSkill.Level;
                        skill.IsFixed = additionalSkill.IsFixed; // true
                        return true;
                    }
                    if (skill.IsFixed && !additionalSkill.IsFixed)
                    {
                        return false;
                    }

                    // 固定フラグに差がない場合は高いほうを優先
                    if (skill.Level < additionalSkill.Level)
                    {
                        skill.Level = additionalSkill.Level;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            Skills.Add(additionalSkill);
            return true;
        }

    }
}
