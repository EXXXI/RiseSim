using System.Collections.Generic;
using System.Text;

namespace SimModel.Model
{
    /// <summary>
    /// 装備
    /// </summary>
    public class Equipment
    {
        /// <summary>
        /// 管理用装備名
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 性別制限
        /// </summary>
        public Sex Sex { get; set; }

        /// <summary>
        /// レア度
        /// </summary>
        public int Rare { get; set; }

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
        /// 初期防御力
        /// </summary>
        public int Mindef { get; set; }

        /// <summary>
        /// 最大防御力
        /// </summary>
        public int Maxdef { get; set; }

        /// <summary>
        /// 火耐性
        /// </summary>
        public int Fire { get; set; }

        /// <summary>
        /// 水耐性
        /// </summary>
        public int Water { get; set; }

        /// <summary>
        /// 雷耐性
        /// </summary>
        public int Thunder { get; set; }

        /// <summary>
        /// 氷耐性
        /// </summary>
        public int Ice { get; set; }

        /// <summary>
        /// 龍耐性
        /// </summary>
        public int Dragon { get; set; }

        /// <summary>
        /// 仮番号(除外固定画面用)
        /// </summary>
        public int RowNo { get; set; } = int.MaxValue;

        /// <summary>
        /// スキル(錬成スキルを別個に扱う)
        /// </summary>
        public List<Skill> Skills { get; set; } = new();

        /// <summary>
        /// スキル(錬成スキルを合算して扱う)
        /// </summary>
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

        /// <summary>
        /// 装備種類
        /// </summary>
        public EquipKind Kind { get; set; }

        /// <summary>
        /// ベース防具(非錬成防具の場合null)
        /// </summary>
        public Equipment? BaseEquipment { get; set; } = null;

        /// <summary>
        /// 錬成テーブル
        /// </summary>
        public int AugmentationTable { get; set; }

        /// <summary>
        /// 各コストごとの追加可能スキル数(理想錬成用)
        /// </summary>
        public int[] GenericSkills { get; set; } = new int[5];

        /// <summary>
        /// 理想錬成データ
        /// </summary>
        public IdealAugmentation Ideal { get; set; }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public Equipment()
        {

        }

        /// <summary>
        /// 装備種類指定コンストラクタ
        /// </summary>
        /// <param name="kind"></param>
        public Equipment(EquipKind kind)
        {
            Kind = kind;
        }

        /// <summary>
        /// コピーコンストラクタ
        /// </summary>
        /// <param name="equip"></param>
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

        /// <summary>
        /// 表示用装備名の本体
        /// </summary>
        private string? dispName = null;
        /// <summary>
        /// 表示用装備名(護石は特殊処理、錬成で名前付けした場合はそれを優先)
        /// </summary>
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

        /// <summary>
        /// 一覧での詳細表示用
        /// </summary>
        public string DetailDispName
        {
            get
            {
                if (BaseEquipment == null)
                {
                    return DispName;
                }

                StringBuilder sb = new();
                sb.AppendLine(DispName);

                // スロット計算
                int addSlotSum = 0;
                addSlotSum += Slot1 + Slot2 + Slot3;
                addSlotSum -= BaseEquipment.Slot1 + BaseEquipment.Slot2 + BaseEquipment.Slot3;
                sb.AppendLine("スロット追加：" + addSlotSum);

                // スキル一覧
                bool isFirst = true;
                foreach (var skill in Skills)
                {
                    if (skill.IsAdditional)
                    {
                        if (isFirst)
                        {
                            isFirst = false;
                        }
                        else
                        {
                            sb.Append(", ");
                        }
                        sb.Append($"{skill.Name}{skill.Level:+#;-#;}");
                    }
                }
                for (int i = 0; i < 5; i++)
                {
                    int level = GenericSkills[i];
                    if (level != 0)
                    {
                        if (isFirst)
                        {
                            isFirst = false;
                        }
                        else
                        {
                            sb.Append(", ");
                        }
                        sb.Append($"c{i * 3 + 3}{level:+#;-#;}");
                    }
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// 装備の説明
        /// </summary>
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

                // 理想錬成情報
                if (Ideal != null)
                {
                    sb.Append('\n');
                    sb.Append("-----------");
                    sb.Append('\n');
                    sb.Append("(理想錬成の内容)");
                    sb.Append('\n');
                    sb.Append("スロット追加：");
                    sb.Append(Ideal.SlotIncrement);
                    foreach (var skill in Skills)
                    {
                        if (skill.IsAdditional)
                        {
                            sb.Append('\n');
                            sb.Append(skill.Description);
                        }
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

        /// <summary>
        /// 装備の簡易説明(名前とスロットのみ)
        /// </summary>
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
