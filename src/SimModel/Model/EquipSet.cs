using SimModel.Config;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimModel.Model
{
    /// <summary>
    /// 装備セット
    /// </summary>
    public class EquipSet
    {
        /// <summary>
        /// スロット不正時文字列
        /// </summary>
        private const string InvalidSlot = "invalid";

        /// <summary>
        /// 錬成コスト不正時文字列
        /// </summary>
        private const string InvalidGSkill = "invalid";

        /// <summary>
        /// 頭装備
        /// </summary>
        public Equipment Head { get; set; } = new Equipment(EquipKind.head);

        /// <summary>
        /// 胴装備
        /// </summary>
        public Equipment Body { get; set; } = new Equipment(EquipKind.body);

        /// <summary>
        /// 腕装備
        /// </summary>
        public Equipment Arm { get; set; } = new Equipment(EquipKind.arm);

        /// <summary>
        /// 腰装備
        /// </summary>
        public Equipment Waist { get; set; } = new Equipment(EquipKind.waist);

        /// <summary>
        /// 足装備
        /// </summary>
        public Equipment Leg { get; set; } = new Equipment(EquipKind.leg);

        /// <summary>
        /// 護石
        /// </summary>
        public Equipment Charm { get; set; } = new Equipment(EquipKind.charm);

        /// <summary>
        /// 装飾品(リスト)
        /// </summary>
        public List<Equipment> Decos { get; set; } = new();

        /// <summary>
        /// 理想錬成用のコストでつくスキル
        /// </summary>
        public List<Equipment> GenericSkills { get; set; } = new();

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
        /// マイセット用名前
        /// </summary>
        public string Name { get; set; } = LogicConfig.Instance.DefaultMySetName;

        /// <summary>
        /// 合計パラメータ計算用装備一覧
        /// </summary>
        private List<Equipment> Equipments
        {
            get
            {
                List<Equipment> ret = new List<Equipment>();
                ret.Add(Head);
                ret.Add(Body);
                ret.Add(Arm);
                ret.Add(Waist);
                ret.Add(Leg);
                ret.Add(Charm);
                foreach (var deco in Decos)
                {
                    ret.Add(deco);
                }
                foreach (var gSkill in GenericSkills)
                {
                    ret.Add(gSkill);
                }
                return ret;
            }
        }

        /// <summary>
        /// 初期防御力
        /// </summary>
        public int Mindef
        {
            get
            {
                int ret = 0;
                foreach (var equip in Equipments)
                {
                    ret += equip.Mindef;
                }
                return ret;
            }
        }

        /// <summary>
        /// 最大防御力
        /// </summary>
        public int Maxdef
        {
            get
            {
                int ret = 0;
                foreach (var equip in Equipments)
                {
                    ret += equip.Maxdef;
                }
                return ret;
            }
        }

        /// <summary>
        /// 火耐性
        /// </summary>
        public int Fire
        {
            get
            {
                int ret = 0;
                foreach (var equip in Equipments)
                {
                    ret += equip.Fire;
                }
                return ret;
            }
        }

        /// <summary>
        /// 水耐性
        /// </summary>
        public int Water
        {
            get
            {
                int ret = 0;
                foreach (var equip in Equipments)
                {
                    ret += equip.Water;
                }
                return ret;
            }
        }

        /// <summary>
        /// 雷耐性
        /// </summary>
        public int Thunder
        {
            get
            {
                int ret = 0;
                foreach (var equip in Equipments)
                {
                    ret += equip.Thunder;
                }
                return ret;
            }
        }

        /// <summary>
        /// 氷耐性
        /// </summary>
        public int Ice
        {
            get
            {
                int ret = 0;
                foreach (var equip in Equipments)
                {
                    ret += equip.Ice;
                }
                return ret;
            }
        }

        /// <summary>
        /// 龍耐性
        /// </summary>
        public int Dragon
        {
            get
            {
                int ret = 0;
                foreach (var equip in Equipments)
                {
                    ret += equip.Dragon;
                }
                return ret;
            }
        }

        /// <summary>
        /// スキル(リスト)
        /// </summary>
        public List<Skill> Skills
        {
            get
            {
                List<Skill> ret = new List<Skill>();
                foreach (var equip in Equipments)
                {
                    JoinSkill(ret, equip.Skills);
                }

                // 風雷合一：風雷合一判定
                int furaiPlus = 0;
                foreach (var skill in ret)
                {
                    if (LogicConfig.Instance.FuraiName.Equals(skill.Name))
                    {
                        switch (skill.Level)
                        {
                            case 4:
                                furaiPlus = 1;
                                break;
                            case 5:
                                furaiPlus = 2;
                                break;
                            default:
                                break;
                        }
                    }
                }

                // 風雷合一：スキル追加
                if (furaiPlus > 0)
                {
                    // 対象洗い出し
                    List<Skill> furaiTarget = new List<Skill>();
                    JoinSkill(furaiTarget, Head.Skills, true);
                    JoinSkill(furaiTarget, Body.Skills, true);
                    JoinSkill(furaiTarget, Arm.Skills, true);
                    JoinSkill(furaiTarget, Waist.Skills, true);
                    JoinSkill(furaiTarget, Leg.Skills, true);

                    // スキルレベル設定
                    foreach (var skill in furaiTarget)
                    {
                        skill.Level = furaiPlus;
                    }

                    // スキルレベル追加
                    JoinSkill(ret, furaiTarget);
                }

                // スキルレベル最大値調整
                foreach (var skill in ret)
                {
                    if (skill.Level > skill.MaxLevel)
                    {
                        skill.Level = skill.MaxLevel;
                    }
                }

                ret.Sort((a, b) => b.Level - a.Level);
                return ret;
            }
        }


        /// <summary>
        /// 制約式名称用の、装飾品を除いたCSV表記
        /// </summary>
        public string GlpkRowName
        {
            get
            {
                StringBuilder sb = new();
                sb.Append(Head.Name);
                sb.Append(',');
                sb.Append(Body.Name);
                sb.Append(',');
                sb.Append(Arm.Name);
                sb.Append(',');
                sb.Append(Waist.Name);
                sb.Append(',');
                sb.Append(Leg.Name);
                sb.Append(',');
                sb.Append(Charm.Name);

                return sb.ToString();
            }
        }

        /// <summary>
        /// 存在している装備の名前を返す(GLPK用)
        /// </summary>
        /// <returns>リスト</returns>
        public List<Equipment> ExistingEquipsWithOutDecos()
        {
            List<Equipment> list = new();
            if (!string.IsNullOrWhiteSpace(Head.Name))
            {
                list.Add(Head);
            }
            if (!string.IsNullOrWhiteSpace(Body.Name))
            {
                list.Add(Body);
            }
            if (!string.IsNullOrWhiteSpace(Arm.Name))
            {
                list.Add(Arm);
            }
            if (!string.IsNullOrWhiteSpace(Waist.Name))
            {
                list.Add(Waist);
            }
            if (!string.IsNullOrWhiteSpace(Leg.Name))
            {
                list.Add(Leg);
            }
            if (!string.IsNullOrWhiteSpace(Charm.Name))
            {
                list.Add(Charm);
            }
            return list;
        }


        /// <summary>
        /// 装飾品のCSV表記 Set可能
        /// </summary>
        public string DecoNameCSV
        {
            get
            {
                StringBuilder sb = new();
                bool isFirst = true;
                foreach (var deco in Decos)
                {
                    if (!isFirst)
                    {
                        sb.Append(',');
                    }
                    sb.Append(deco.Name);
                    isFirst = false;
                }
                return sb.ToString();
            }
            set
            {
                Decos = new List<Equipment>();
                string[] splitted = value.Split(',');
                foreach (var decoName in splitted)
                {
                    if (string.IsNullOrWhiteSpace(decoName))
                    {
                        continue;
                    }
                    Equipment? deco = Masters.GetEquipByName(decoName, false);
                    if (deco != null)
                    {
                        Decos.Add(deco);
                    }
                }
                SortDecos();
            }
        }

        /// <summary>
        /// 装飾品のCSV表記 3行
        /// </summary>
        public string DecoNameCSVMultiLine
        {
            get
            {
                StringBuilder sb = new();
                int secondLineIdx = Decos.Count / 3;
                int thirdLineIdx = Decos.Count * 2 / 3;
                for (int i = 0; i < Decos.Count; i++)
                {
                    if (i == 0)
                    {
                        // 処理なし
                    }
                    else if (i == secondLineIdx)
                    {
                        sb.Append(',');
                        sb.Append('\n');
                    }
                    else if (i == thirdLineIdx)
                    {
                        sb.Append(',');
                        sb.Append('\n');
                    }
                    else
                    {
                        sb.Append(',');
                    }
                    sb.Append(Decos[i].Name);
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// 理想錬成の追加スキルのCSV表記
        /// </summary>
        public string GSkillNameCSV
        {
            get
            {
                StringBuilder sb = new();
                bool isFirst = true;
                foreach (var gskill in GenericSkills)
                {
                    if (!isFirst)
                    {
                        sb.Append(',');
                    }
                    sb.Append(gskill.DispName);
                    isFirst = false;
                }
                return sb.ToString();
            }
            set
            {
                GenericSkills = new List<Equipment>();
                string[] splitted = value.Split(',');
                foreach (var label in splitted)
                {
                    if (string.IsNullOrWhiteSpace(label))
                    {
                        continue;
                    }
                    Equipment? gskill = Masters.GetEquipByName(label, false);
                    if (gskill != null)
                    {
                        GenericSkills.Add(gskill);
                    }
                }
                SortGSkills();
            }
        }

        /// <summary>
        /// 武器スロの表示用形式(2-2-0など)
        /// </summary>
        public string WeaponSlotDisp
        {
            get
            {
                return string.Join('-', WeaponSlot1, WeaponSlot2, WeaponSlot3);
            }
        }

        /// <summary>
        /// スキルのCSV形式
        /// </summary>
        public string SkillsDisp
        {
            get
            {
                List<Skill> existSkill = Skills.Where(s => s.Level > 0).ToList();

                StringBuilder sb = new();
                bool first = true;
                foreach (var skill in existSkill)
                {
                    if (skill.Level > 0)
                    {
                        if (!first)
                        {
                            sb.Append(", ");
                        }
                        sb.Append(skill.Description);
                        first = false;
                    }
                }
                return sb.ToString();
            }
        }


        /// <summary>
        /// 装飾品のCSV表記 3行
        /// </summary>
        public string SkillsDispMultiLine
        {
            get
            {
                List<Skill> existSkill = Skills.Where(s => s.Level > 0).ToList();

                StringBuilder sb = new();
                int secondLineIdx = existSkill.Count / 3;
                int thirdLineIdx = existSkill.Count * 2 / 3;
                for (int i = 0; i < existSkill.Count; i++)
                {
                    if (i == 0)
                    {
                        // 処理なし
                    }
                    else if (i == secondLineIdx)
                    {
                        sb.Append(',');
                        sb.Append('\n');
                    }
                    else if (i == thirdLineIdx)
                    {
                        sb.Append(',');
                        sb.Append('\n');
                    }
                    else
                    {
                        sb.Append(',');
                    }
                    sb.Append(existSkill[i].Description);
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
                StringBuilder sb = new();
                if (!IsDecoValid)
                {
                    sb.Append("※傀異錬成防具のスロットを減らしたため、\n");
                    sb.Append("※このマイセットの装飾品は装備しきれません\n");
                }
                if (!IsGSkillValid)
                {
                    sb.Append("※理想錬成防具の追加スキル数を減らしたため、\n");
                    sb.Append("※このマイセットの追加スキルは実現できません\n");
                }
                sb.Append("武器スロ：");
                sb.Append(WeaponSlotDisp);
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
                sb.Append('\n');
                sb.Append(Head.SimpleDescription);
                sb.Append('\n');
                sb.Append(Body.SimpleDescription);
                sb.Append('\n');
                sb.Append(Arm.SimpleDescription);
                sb.Append('\n');
                sb.Append(Waist.SimpleDescription);
                sb.Append('\n');
                sb.Append(Leg.SimpleDescription);
                sb.Append('\n');
                sb.Append(Charm.SimpleDescription);
                sb.Append('\n');
                sb.Append(EquipKind.deco.StrWithColon());
                sb.Append(DecoNameCSV);
                if (GenericSkills.Count > 0)
                {
                    sb.Append('\n');
                    sb.Append(EquipKind.gskill.StrWithColon());
                    sb.Append(GSkillNameCSV);
                }
                sb.Append('\n');
                sb.Append("空きスロ：");
                sb.Append(EmptySlotNum);
                sb.Append('\n'); 
                sb.Append("-----------");
                if (HasAugmentation)
                {
                    sb.Append('\n');
                    sb.Append("(錬成詳細)");
                    if (Head.BaseEquipment != null)
                    {
                        sb.Append('\n');
                        sb.Append("■" + Head.Kind.Str());
                        sb.Append('\n');
                        sb.Append(Head.DetailDispName);
                    }
                    if (Body.BaseEquipment != null)
                    {
                        sb.Append('\n');
                        sb.Append("■" + Body.Kind.Str());
                        sb.Append('\n');
                        sb.Append(Body.DetailDispName);
                    }
                    if (Arm.BaseEquipment != null)
                    {
                        sb.Append('\n');
                        sb.Append("■" + Arm.Kind.Str());
                        sb.Append('\n');
                        sb.Append(Arm.DetailDispName);
                    }
                    if (Waist.BaseEquipment != null)
                    {
                        sb.Append('\n');
                        sb.Append("■" + Waist.Kind.Str());
                        sb.Append('\n');
                        sb.Append(Waist.DetailDispName);
                    }
                    if (Leg.BaseEquipment != null)
                    {
                        sb.Append('\n');
                        sb.Append("■" + Leg.Kind.Str());
                        sb.Append('\n');
                        sb.Append(Leg.DetailDispName);
                    }
                    sb.Append('\n');
                    sb.Append("-----------");
                }
                if (IsDecoValid)
                {
                    sb.Append('\n');
                    sb.Append("装飾品装備例");
                    sb.Append('\n');
                    sb.Append(DecoExampleSetting);
                    sb.Append("-----------");
                }
                foreach (var skill in Skills)
                {
                    if (skill.Level > 0)
                    {
                        sb.Append('\n');
                        sb.Append(skill.Description);
                    }
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// 装飾品をはめ込む例
        /// 装飾品情報がソートされていることを前提としている
        /// </summary>
        private string DecoExampleSetting
        {
            get
            {
                StringBuilder weaponSb = new StringBuilder("・武器スロ\n");
                StringBuilder headSb = new StringBuilder("・頭\n");
                StringBuilder bodySb = new StringBuilder("・胴\n");
                StringBuilder armSb = new StringBuilder("・腕\n");
                StringBuilder waistSb = new StringBuilder("・腰\n");
                StringBuilder legSb = new StringBuilder("・脚\n");
                StringBuilder charmSb = new StringBuilder("・護石\n");


                int decoIndex = 0;
                for (int slotLv = 4; slotLv > 0; slotLv--)
                {
                    if (WeaponSlot1 == slotLv && decoIndex < Decos.Count)
                    {
                        weaponSb.Append(Decos[decoIndex++].DispName);
                        weaponSb.Append("\n");
                    }
                    if (WeaponSlot2 == slotLv && decoIndex < Decos.Count)
                    {
                        weaponSb.Append(Decos[decoIndex++].DispName);
                        weaponSb.Append("\n");
                    }
                    if (WeaponSlot3 == slotLv && decoIndex < Decos.Count)
                    {
                        weaponSb.Append(Decos[decoIndex++].DispName);
                        weaponSb.Append("\n");
                    }
                    decoIndex = AppendDecoExample(decoIndex, slotLv, Head, headSb);
                    decoIndex = AppendDecoExample(decoIndex, slotLv, Body, bodySb);
                    decoIndex = AppendDecoExample(decoIndex, slotLv, Arm, armSb);
                    decoIndex = AppendDecoExample(decoIndex, slotLv, Waist, waistSb);
                    decoIndex = AppendDecoExample(decoIndex, slotLv, Leg, legSb);
                    decoIndex = AppendDecoExample(decoIndex, slotLv, Charm, charmSb);
                }

                return weaponSb.ToString() + headSb.ToString() + bodySb.ToString() + armSb.ToString() + waistSb.ToString() + legSb.ToString() + charmSb.ToString();
            }
        }

        /// <summary>
        /// 防具に指定レベルのスロットがあったらそこにはめるべき装飾品情報を書き込む
        /// </summary>
        /// <param name="decoIndex">装飾品の連番</param>
        /// <param name="slotLv">指定スロットレベル</param>
        /// <param name="equip">装備</param>
        /// <param name="sb">StringBuilder</param>
        /// <returns>書き込み後の装飾品の連番</returns>
        private int AppendDecoExample(int decoIndex, int slotLv, Equipment equip, StringBuilder sb)
        {
            if (equip.Slot1 == slotLv && decoIndex < Decos.Count)
            {
                sb.Append(Decos[decoIndex++].DispName);
                sb.Append("\n");
            }

            if (equip.Slot2 == slotLv && decoIndex < Decos.Count)
            {
                sb.Append(Decos[decoIndex++].DispName);
                sb.Append("\n");
            }

            if (equip.Slot3 == slotLv && decoIndex < Decos.Count)
            {
                sb.Append(Decos[decoIndex++].DispName);
                sb.Append("\n");
            }

            return decoIndex;
        }


        /// <summary>
        /// 防具の空きスロット合計
        /// </summary>
        public string EmptySlotNum
        {
            get
            {
                int[] reqSlots = { 0, 0, 0, 0 }; // 要求スロット
                int[] hasSlots = { 0, 0, 0, 0 }; // 所持スロット
                int[] restSlots = { 0, 0, 0, 0 }; // 空きスロット

                foreach (var deco in Decos)
                {
                    reqSlots[deco.Slot1 - 1]++;
                }
                if (WeaponSlot1 > 0)
                {
                    hasSlots[WeaponSlot1 - 1]++;
                }
                if (WeaponSlot2 > 0)
                {
                    hasSlots[WeaponSlot2 - 1]++;
                }
                if (WeaponSlot3 > 0)
                {
                    hasSlots[WeaponSlot3 - 1]++;
                }
                CalcEquipHasSlot(hasSlots, Head);
                CalcEquipHasSlot(hasSlots, Body);
                CalcEquipHasSlot(hasSlots, Arm);
                CalcEquipHasSlot(hasSlots, Waist);
                CalcEquipHasSlot(hasSlots, Leg);
                CalcEquipHasSlot(hasSlots, Charm);

                // 空きスロット算出
                for (int i = 0; i < 4; i++)
                {
                    restSlots[i] = hasSlots[i] - reqSlots[i];
                }

                // 足りない分は1Lv上を消費する
                for (int i = 0; i < 3; i++)
                {
                    if (restSlots[i] < 0)
                    {
                        restSlots[i + 1] += restSlots[i];
                        restSlots[i] = 0;
                    }
                }

                if (restSlots[3] < 0)
                {
                    // スロット不足
                    return InvalidSlot;
                }

                return $"Lv1:{restSlots[0]}, Lv2:{restSlots[1]}, Lv3:{restSlots[2]}, Lv4:{restSlots[3]}";
            }
        }

        /// <summary>
        /// 理想錬成防具の錬成スキルの空き
        /// </summary>
        public string EmptyGSkillNum
        {
            get
            {
                int[] reqGSkills = { 0, 0, 0, 0, 0 }; // 要求
                int[] hasGSkills = { 0, 0, 0, 0, 0 }; // 所持
                int[] restGSkills = { 0, 0, 0, 0, 0 }; // 空き


                
                foreach (var gskill in GenericSkills)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        reqGSkills[i] += gskill.GenericSkills[i];
                    }
                }
                CalcEquipHasGSkill(hasGSkills, Head);
                CalcEquipHasGSkill(hasGSkills, Body);
                CalcEquipHasGSkill(hasGSkills, Arm);
                CalcEquipHasGSkill(hasGSkills, Waist);
                CalcEquipHasGSkill(hasGSkills, Leg);

                // 空き算出
                for (int i = 0; i < 5; i++)
                {
                    restGSkills[i] = hasGSkills[i] - reqGSkills[i];
                }

                // 足りない分は1Lv上を消費する
                for (int i = 0; i < 4; i++)
                {
                    if (restGSkills[i] < 0)
                    {
                        restGSkills[i + 1] += restGSkills[i];
                        restGSkills[i] = 0;
                    }
                }

                if (restGSkills[4] < 0)
                {
                    // スロット不足
                    return InvalidGSkill;
                }

                return $"c3:{restGSkills[0]}, c6:{restGSkills[1]}, c9:{restGSkills[2]}, c12:{restGSkills[3]}, c15:{restGSkills[4]}";
            }
        }

        /// <summary>
        /// 装飾品がはめられる状態かチェック
        /// </summary>
        private bool IsDecoValid
        {
            get
            {
                return EmptySlotNum != InvalidSlot;
            }
        }

        /// <summary>
        /// 理想錬成の追加スキルが実現可能な状態かチェック
        /// </summary>
        public bool IsGSkillValid
        {
            get
            {
                return EmptyGSkillNum != InvalidGSkill;
            }
        }

        /// <summary>
        /// 錬成を利用しているかどうかチェック(理想錬成も含む)
        /// </summary>
        public bool HasAugmentation
        {
            get
            {
                foreach (var equip in Equipments)
                {
                    if (equip.BaseEquipment != null)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// 理想錬成を利用しているかどうかチェック
        /// </summary>
        public bool HasIdeal
        {
            get
            {
                foreach (var equip in Equipments)
                {
                    if (equip.Ideal != null)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// 装飾品のソート
        /// </summary>
        public void SortDecos()
        {
            List<Equipment> newDecos = new List<Equipment>();
            for (int i = 4; i > 0; i--)
            {
                foreach (var deco in Decos)
                {
                    if (deco.Slot1 == i)
                    {
                        newDecos.Add(deco);
                    }
                }
            }
            Decos = newDecos;
        }

        /// <summary>
        /// 理想錬成スキルのソート
        /// </summary>
        public void SortGSkills()
        {
            List<Equipment> newGSkills = new List<Equipment>();
            for (int i = 4; i >= 0; i--)
            {
                foreach (var gskill in GenericSkills)
                {
                    if (gskill.GenericSkills[i] == 1)
                    {
                        newGSkills.Add(gskill);
                    }
                }
            }
            GenericSkills = newGSkills;
        }


        /// <summary>
        /// 防具のスロット数計算
        /// </summary>
        /// <param name="hasSlots">スロット数格納用配列</param>
        /// <param name="equip">装備</param>
        private static void CalcEquipHasSlot(int[] hasSlots, Equipment equip)
        {
            if (equip.Slot1 > 0)
            {
                hasSlots[equip.Slot1 - 1]++;
            }
            if (equip.Slot2 > 0)
            {
                hasSlots[equip.Slot2 - 1]++;
            }
            if (equip.Slot3 > 0)
            {
                hasSlots[equip.Slot3 - 1]++;
            }
        }

        /// <summary>
        /// 防具のコスト数計算
        /// </summary>
        /// <param name="hasGSkills">コスト数格納用配列</param>
        /// <param name="equip">装備</param>
        private static void CalcEquipHasGSkill(int[] hasGSkills, Equipment equip)
        {
            for (int i = 0; i < 5; i++)
            {
                hasGSkills[i] += equip.GenericSkills[i];
            }
        }

        /// <summary>
        /// スキルの追加(同名スキルはスキルレベルを加算)
        /// 最大値のチェックはここではしていない
        /// </summary>
        /// <param name="baseSkills">スキル一覧</param>
        /// <param name="newSkills">追加するスキル</param>
        /// <returns>合わせたスキル一覧</returns>
        private List<Skill> JoinSkill(List<Skill> baseSkills, List<Skill> newSkills)
        {
            foreach (var newSkill in newSkills)
            {
                if (string.IsNullOrWhiteSpace(newSkill.Name))
                {
                    continue;
                }

                bool exist = false;
                foreach (var baseSkill in baseSkills)
                {
                    if (baseSkill.Name.Equals(newSkill.Name))
                    {
                        exist = true;
                        baseSkill.Level += newSkill.Level;
                    }
                }
                if (!exist)
                {
                    baseSkills.Add(new Skill(newSkill.Name, newSkill.Level));
                }
            }
            return baseSkills;
        }

        /// <summary>
        /// スキルの追加(同名スキルはスキルレベルを加算)
        ///  最大値のチェックはしていない
        /// </summary>
        /// <param name="baseSkills">スキル一覧</param>
        /// <param name="newSkills">追加するスキル</param>
        /// <param name="excludeAdditional">錬成の追加スキルは除外する場合true</param>
        /// <returns></returns>
        //
        private List<Skill> JoinSkill(List<Skill> baseSkills, List<Skill> newSkills, bool excludeAdditional)
        {
            List<Skill> skills = new();
            if (excludeAdditional)
            {
                foreach (var skill in newSkills)
                {
                    if (!skill.IsAdditional)
                    {
                        skills.Add(skill);
                    }
                }
                return JoinSkill(baseSkills, skills);
            }
            else
            {
                return JoinSkill(baseSkills, newSkills);
            }
        }
    }
}
