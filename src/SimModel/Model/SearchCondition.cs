using SimModel.Domain;
using System.Collections.Generic;
using System.Text;

namespace SimModel.Model
{
    /// <summary>
    /// 検索条件
    /// </summary>
    public class SearchCondition
    {
        /// <summary>
        /// スキルリスト
        /// </summary>
        public List<Skill> Skills { get; set; } = new();

        /// <summary>
        /// 武器スロ1つ目
        /// </summary>
        public int WeaponSlot1 { get; set; }

        /// <summary>
        /// 武器スロ2つ目
        /// </summary>
        public int WeaponSlot2 { get; set; }

        /// <summary>
        /// 武器スロ3つ目
        /// </summary>
        public int WeaponSlot3 { get; set; }

        /// <summary>
        /// 防御力
        /// </summary>
        public int? Def { get; set; }

        /// <summary>
        /// 火耐性
        /// </summary>
        public int? Fire { get; set; }

        /// <summary>
        /// 水耐性
        /// </summary>
        public int? Water { get; set; }

        /// <summary>
        /// 雷耐性
        /// </summary>
        public int? Thunder { get; set; }

        /// <summary>
        /// 氷耐性
        /// </summary>
        public int? Ice { get; set; }

        /// <summary>
        /// 龍耐性
        /// </summary>
        public int? Dragon { get; set; }

        /// <summary>
        /// 性別
        /// </summary>
        public Sex Sex { get; set; }

        /// <summary>
        /// 理想錬成を利用するか否か
        /// </summary>
        public bool IncludeIdealAugmentation { get; set; }

        /// <summary>
        /// 通常装備を優先するか否か
        /// </summary>
        public bool PrioritizeNoIdeal { get; set; }

        /// <summary>
        /// 既存装備で組める場合を除外するか否か
        /// </summary>
        public bool ExcludeAbstract { get; set; }

        /// <summary>
        /// マイ検索条件保存用ID
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// マイ検索条件保存用名前
        /// </summary>
        public string DispName { get; set; }

        /// <summary>
        /// 錬成再計算用 部位固定情報
        /// </summary>
        public SortedDictionary<string, int>? AdditionalFixData { get; set; } = null;

        /// <summary>
        /// CSV用スキル形式
        /// </summary>
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

        /// <summary>
        /// 表示用説明
        /// </summary>
        public string Description
        {
            get
            {
                string none = "なし";
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"武器スロ:{WeaponSlot1}-{WeaponSlot2}-{WeaponSlot3}");
                sb.AppendLine($"防御力:{Def?.ToString() ?? none}, 性別:{Sex.Str()}");
                sb.Append($"火:{Fire?.ToString() ?? none},");
                sb.Append($"水:{Water?.ToString() ?? none},");
                sb.Append($"雷:{Thunder?.ToString() ?? none},");
                sb.Append($"氷:{Ice?.ToString() ?? none},");
                sb.Append($"龍:{Dragon?.ToString() ?? none}");
                foreach (var skill in Skills)
                {
                    sb.AppendLine();
                    sb.Append($"{skill.Name}Lv{skill.Level}{(skill.IsFixed ? "(固定)" : string.Empty)}");
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public SearchCondition()
        {
        }

        /// <summary>
        /// コピーコンストラクタ
        /// </summary>
        /// <param name="condition"></param>
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

        /// <summary>
        /// スキル追加(同名スキルはレベルが高い方のみを採用、固定がある場合は固定が優先)
        /// </summary>
        /// <param name="additionalSkill">追加スキル</param>
        /// <returns>追加したスキルが有効だった場合true</returns>
        // 
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
