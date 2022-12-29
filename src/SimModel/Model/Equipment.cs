using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimModel.Model
{
    // 装備
    public class Equipment
    {
        // 管理用装備名
        public string Name { get; set; } = string.Empty;

        // 性別制限
        public Sex Sex { get; set; }

        // レア度
        public int Rare { get; set; }

        // スロット1つ目
        public int Slot1 { get; set; }

        // スロット2つ目
        public int Slot2 { get; set; }

        // スロット3つ目
        public int Slot3 { get; set; }

        // 初期防御力
        public int Mindef { get; set; }

        // 最大防御力
        public int Maxdef { get; set; }

        // 火耐性
        public int Fire { get; set; }

        // 水耐性
        public int Water { get; set; }

        // 雷耐性
        public int Thunder { get; set; }

        // 氷耐性
        public int Ice { get; set; }

        // 龍耐性
        public int Dragon { get; set; }

        // スキル(錬成スキルを別個に扱う)
        public List<Skill> Skills { get; set; } = new();

        // スキル(錬成スキルを合算して扱う)
        public List<Skill> MargedSkills {
            get
            {
                List<Skill> margedSkills = new();
                foreach (Skill skill in Skills)
                {
                    bool isExist = false;
                    foreach (Skill margedSkill in margedSkills)
                    {
                        if (skill.Name == margedSkill.Name)
                        {
                            margedSkill.Level += skill.Level;
                            isExist = true;
                        }
                    }
                    if (!isExist)
                    {
                        margedSkills.Add(new Skill(skill.Name, skill.Level));
                    }
                }
                return margedSkills;
            }
        }

        // 装備種類
        public EquipKind Kind { get; set; }

        // ベース防具(非錬成防具の場合null)
        public Equipment? BaseEquipment { get; set; } = null;

        // 錬成テーブル
        public int AugmentationTable { get; set; }

        // 各コストごとの追加可能スキル数(理想錬成用)
        public int[] GenericSkills { get; set; } = new int[5];

        // 理想錬成データ
        public IdealAugmentation Ideal { get; set; }

        // デフォルトコンストラクタ
        public Equipment()
        {

        }

        // 装備種類指定コンストラクタ
        public Equipment(EquipKind kind)
        {
            Kind = kind;
        }

        // コピーコンストラクタ
        public Equipment(Equipment equip)
        {
            Name = equip.Name;
            Sex = equip.Sex;
            Rare = equip.Rare;
            Slot1 = equip.Slot1;
            Slot2 = equip.Slot2;
            Slot3 = equip.Slot3;
            Mindef = equip.Mindef;
            Maxdef = equip.Maxdef;
            Fire = equip.Fire;
            Water = equip.Water;
            Thunder = equip.Thunder;
            Ice = equip.Ice;
            Dragon = equip.Dragon;
            Kind = equip.Kind;
            foreach (var skill in equip.Skills)
            {
                Skills.Add(skill);
            }
        }

        // 表示用装備名(護石は特殊処理、錬成で名前付けした場合はそれを優先)
        private string? dispName = null;
        public string DispName { 
            get
            {
                if (!string.IsNullOrWhiteSpace(dispName))
                {
                    return dispName;
                }
                if (!Kind.Equals(EquipKind.charm) || string.IsNullOrWhiteSpace(Name))
                {
                    return Name;
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    bool isFirst = true;
                    foreach (var skill in Skills)
                    {
                        if (!isFirst)
                        {
                            sb.Append(',');
                        }
                        sb.Append(skill.Name);
                        sb.Append(skill.Level);
                        isFirst = false;
                    }
                    if (!isFirst)
                    {
                        sb.Append(',');
                    }
                    sb.Append(Slot1);
                    sb.Append('-');
                    sb.Append(Slot2);
                    sb.Append('-');
                    sb.Append(Slot3);

                    return sb.ToString();
                }
            }
            set
            {
                dispName = value;
            }
        }


        // 装備の説明
        public string Description
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Name))
                {
                    return string.Empty;
                }

                StringBuilder sb = new StringBuilder();
                sb.Append(DispName);
                if (!Kind.Equals(EquipKind.deco) && !Kind.Equals(EquipKind.charm))
                {
                    sb.Append(',');
                    sb.Append(Slot1);
                    sb.Append('-');
                    sb.Append(Slot2);
                    sb.Append('-');
                    sb.Append(Slot3);
                    sb.Append('\n');
                    sb.Append("防御:");
                    sb.Append(Mindef);
                    sb.Append('→');
                    sb.Append(Maxdef);
                    sb.Append(',');
                    sb.Append("火:");
                    sb.Append(Fire);
                    sb.Append(',');
                    sb.Append("水:");
                    sb.Append(Water);
                    sb.Append(',');
                    sb.Append("雷:");
                    sb.Append(Thunder);
                    sb.Append(',');
                    sb.Append("氷:");
                    sb.Append(Ice);
                    sb.Append(',');
                    sb.Append("龍:");
                    sb.Append(Dragon);
                }
                foreach (var skill in Skills)
                {
                    sb.Append('\n');
                    sb.Append(skill.Description);
                }
                if (GenericSkills[0] > 0)
                {
                    sb.Append('\n');
                    sb.Append("c3スキル +" + GenericSkills[0]);
                }
                if (GenericSkills[1] > 0)
                {
                    sb.Append('\n');
                    sb.Append("c6スキル +" + GenericSkills[1]);
                }
                if (GenericSkills[2] > 0)
                {
                    sb.Append('\n');
                    sb.Append("c9スキル +" + GenericSkills[2]);
                }
                if (GenericSkills[3] > 0)
                {
                    sb.Append('\n');
                    sb.Append("c12スキル +" + GenericSkills[3]);
                }
                if (GenericSkills[4] > 0)
                {
                    sb.Append('\n');
                    sb.Append("c15スキル +" + GenericSkills[4]);
                }

                // ベース防具情報
                if (BaseEquipment != null)
                {
                    sb.Append('\n');
                    sb.Append("-----------");
                    sb.Append('\n');
                    sb.Append("(ベース防具)");
                    sb.Append('\n');
                    sb.Append(BaseEquipment.Description);
                }

                return sb.ToString();
            }
        }

        // 装備の簡易説明(名前とスロットのみ)
        public string SimpleDescription
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(Kind.StrWithColon());
                if (!string.IsNullOrWhiteSpace(Name))
                {
                    sb.Append(DispName);
                    if (!Kind.Equals(EquipKind.deco) && !Kind.Equals(EquipKind.charm))
                    {
                        sb.Append(',');
                        sb.Append(Slot1);
                        sb.Append('-');
                        sb.Append(Slot2);
                        sb.Append('-');
                        sb.Append(Slot3);
                    }
                    for (int i = 0; i < 5; i++)
                    {
                        for (int j = 0; j < GenericSkills[i]; j++)
                        {
                            sb.Append(',');
                            sb.Append("c" + ((i * 3) + 3));
                        }
                    }
                }

                return sb.ToString();
            }
        }
    }
}
