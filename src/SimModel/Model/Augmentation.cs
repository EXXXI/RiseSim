using System.Collections.Generic;
using System.Text;

namespace SimModel.Model
{
    /// <summary>
    /// 錬成装備
    /// </summary>
    public class Augmentation
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
        /// 装備種類
        /// </summary>
        public EquipKind Kind { get; set; }

        /// <summary>
        /// 装備種類(文字列)
        /// </summary>
        public string KindStr {
            get
            { 
                return Kind.Str();
            }
        }

        /// <summary>
        /// ベース装備名
        /// </summary>
        public string BaseName { get; set; } = string.Empty;

        /// <summary>
        /// スロット1つ目
        /// </summary>
        public int Slot1 { get; set; }

        /// <summary>
        /// スロット2つ目
        /// </summary>
        public int Slot2 { get; set; }

        /// <summary>
        /// スロット3つ目
        /// </summary>
        public int Slot3 { get; set; }

        /// <summary>
        /// スロット表示
        /// </summary>
        public string SlotDisp { 
            get
            { 
                return Slot1 + "-" + Slot2 + "-" + Slot3;
            }
        }

        /// <summary>
        /// 防御力増減
        /// </summary>
        public int Def { get; set; }

        /// <summary>
        /// 火耐性増減
        /// </summary>
        public int Fire { get; set; }

        /// <summary>
        /// 水耐性増減
        /// </summary>
        public int Water { get; set; }

        /// <summary>
        /// 雷耐性増減
        /// </summary>
        public int Thunder { get; set; }

        /// <summary>
        /// 氷耐性増減
        /// </summary>
        public int Ice { get; set; }

        /// <summary>
        /// 龍耐性増減
        /// </summary>
        public int Dragon { get; set; }

        /// <summary>
        /// 追加スキル
        /// </summary>
        public List<Skill> Skills { get; set; } = new();

        /// <summary>
        /// スキルのCSV形式
        /// </summary>
        public string SkillsDisp
        {
            get
            {
                StringBuilder sb = new();
                bool first = true;
                foreach (var skill in Skills)
                {
                    if (!first)
                    {
                        sb.Append(", ");
                    }
                    sb.Append(skill.Description);
                    first = false;
                }
                return sb.ToString();
            }
        }
    }
}
