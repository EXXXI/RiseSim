using SimModel.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimModel.Model
{
    // 装備セット
    public class EquipSet
    {
        private const string InvalidSlot = "invalid";
        private const string InvalidGSkill = "invalid";

        // 頭装備
        public Equipment Head { get; set; } = new Equipment(EquipKind.head);

        // 胴装備
        public Equipment Body { get; set; } = new Equipment(EquipKind.body);

        // 腕装備
        public Equipment Arm { get; set; } = new Equipment(EquipKind.arm);

        // 腰装備
        public Equipment Waist { get; set; } = new Equipment(EquipKind.waist);

        // 足装備
        public Equipment Leg { get; set; } = new Equipment(EquipKind.leg);

        // 護石
        public Equipment Charm { get; set; } = new Equipment(EquipKind.charm);

        // 装飾品(リスト)
        public List<Equipment> Decos { get; set; } = new();

        // 理想錬成用のコストでつくスキル
        public List<Equipment> GenericSkills { get; set; } = new();

        // 武器スロ1つ目
        public int WeaponSlot1 { get; set; }

        // 武器スロ2つ目
        public int WeaponSlot2 { get; set; }

        // 武器スロ3つ目
        public int WeaponSlot3 { get; set; }

        // マイセット用名前
        public string Name { get; set; } = LogicConfig.Instance.DefaultMySetName;

        // 合計パラメータ計算用装備一覧
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

        // 初期防御力
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

        // 最大防御力
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

        // 火耐性
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

        // 水耐性
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

        // 雷耐性
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

        // 氷耐性
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

        // 龍耐性
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

        // スキル(リスト)
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

                ret.Sort((a, b) => b.Level - a.Level);
                return ret;
            }
        }


        // 制約式名称用の、装飾品を除いたCSV表記
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

        // 表示用CSV表記
        public string SimpleSetName
        {
            get
            {
                StringBuilder sb = new();
                sb.Append(Head.DispName);
                sb.Append(',');
                sb.Append(Body.DispName);
                sb.Append(',');
                sb.Append(Arm.DispName);
                sb.Append(',');
                sb.Append(Waist.DispName);
                sb.Append(',');
                sb.Append(Leg.DispName);
                sb.Append(',');
                sb.Append(Charm.DispName);

                foreach (Equipment deco in Decos)
                {
                    sb.Append(',');
                    sb.Append(deco.DispName);
                }

                foreach (Equipment gSkill in GenericSkills)
                {
                    sb.Append(',');
                    sb.Append(gSkill.DispName);
                }

                return sb.ToString();
            }
        }

        // 装備のIndex(頭、胴、腕、腰、足、護石の順に全装備に振った連番)リスト
        public List<int> EquipIndexsWithOutDecos(bool includeIdealAugmentation)
        {
            List<int> list = new();
            if (!string.IsNullOrWhiteSpace(Head.Name))
            {
                list.Add(Masters.GetEquipIndexByName(Head.Name, includeIdealAugmentation));
            }
            if (!string.IsNullOrWhiteSpace(Body.Name))
            {
                list.Add(Masters.GetEquipIndexByName(Body.Name, includeIdealAugmentation));
            }
            if (!string.IsNullOrWhiteSpace(Arm.Name))
            {
                list.Add(Masters.GetEquipIndexByName(Arm.Name, includeIdealAugmentation));
            }
            if (!string.IsNullOrWhiteSpace(Waist.Name))
            {
                list.Add(Masters.GetEquipIndexByName(Waist.Name, includeIdealAugmentation));
            }
            if (!string.IsNullOrWhiteSpace(Leg.Name))
            {
                list.Add(Masters.GetEquipIndexByName(Leg.Name, includeIdealAugmentation));
            }
            if (!string.IsNullOrWhiteSpace(Charm.Name))
            {
                list.Add(Masters.GetEquipIndexByName(Charm.Name, includeIdealAugmentation));
            }
            return list;
        }


        // 装飾品のCSV表記 Set可能
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

        // 装飾品のCSV表記 3行
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

        // 理想錬成の追加スキルのCSV表記
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

        // 武器スロの表示用形式(2-2-0など)
        public string WeaponSlotDisp
        {
            get
            {
                return string.Join('-', WeaponSlot1, WeaponSlot2, WeaponSlot3);
            }
        }

        // スキルのCSV形式
        public string SkillsDisp
        {
            get
            {
                StringBuilder sb = new();
                bool first = true;
                foreach (var skill in Skills)
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


        // 装飾品のCSV表記 3行
        public string SkillsDispMultiLine
        {
            get
            {
                StringBuilder sb = new();
                int secondLineIdx = Skills.Count / 3;
                int thirdLineIdx = Skills.Count * 2 / 3;
                for (int i = 0; i < Skills.Count; i++)
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
                    sb.Append(Skills[i].Description);
                }
                return sb.ToString();
            }
        }

        // 装備の説明
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

        // 装飾品をはめ込む例
        // 装飾品情報がソートされていることを前提としている
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
                    if (WeaponSlot1 == slotLv)
                    {
                        weaponSb.Append(Decos[decoIndex++].DispName);
                        weaponSb.Append("\n");
                    }
                    if (WeaponSlot2 == slotLv)
                    {
                        weaponSb.Append(Decos[decoIndex++].DispName);
                        weaponSb.Append("\n");
                    }
                    if (WeaponSlot3 == slotLv)
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

        // 防具に指定レベルのスロットがあったらそこにはめるべき装飾品情報を書き込む
        private int AppendDecoExample(int decoIndex, int slotLv, Equipment equip, StringBuilder sb)
        {
            if (equip.Slot1 == slotLv)
            {
                sb.Append(Decos[decoIndex++].DispName);
                sb.Append("\n");
            }

            if (equip.Slot2 == slotLv)
            {
                sb.Append(Decos[decoIndex++].DispName);
                sb.Append("\n");
            }

            if (equip.Slot3 == slotLv)
            {
                sb.Append(Decos[decoIndex++].DispName);
                sb.Append("\n");
            }

            return decoIndex;
        }


        // 防具の空きスロット合計
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

        // 理想錬成防具の錬成スキルの空き
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

        // 装飾品がはめられる状態かチェック
        private bool IsDecoValid
        {
            get
            {
                return EmptySlotNum != InvalidSlot;
            }
        }

        // 理想錬成の追加スキルが実現可能な状態かチェック
        public bool IsGSkillValid
        {
            get
            {
                return EmptyGSkillNum != InvalidGSkill;
            }
        }

        // 錬成を利用しているかどうかチェック(理想錬成も含む)
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

        // 理想錬成を利用しているかどうかチェック
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

        // 装飾品のソート
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

        // 理想錬成スキルのソート
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


        // 防具のスロット数計算
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

        // 防具のコスト数計算
        private static void CalcEquipHasGSkill(int[] hasGSkills, Equipment equip)
        {
            for (int i = 0; i < 5; i++)
            {
                hasGSkills[i] += equip.GenericSkills[i];
            }
        }

        // スキルの追加(同名スキルはスキルレベルを加算)
        private List<Skill> JoinSkill(List<Skill> baseSkills, List<Skill> newSkills)
        {
            foreach (var newSkill in newSkills)
            {
                if (string.IsNullOrWhiteSpace(newSkill.Name))
                {
                    continue;
                }

                int maxLevel = 0;
                foreach (var skill in Masters.Skills)
                {
                    if (newSkill.Name.Equals(skill.Name))
                    {
                        maxLevel = skill.Level;
                    }
                }

                bool exist = false;
                foreach (var baseSkill in baseSkills)
                {
                    if (baseSkill.Name.Equals(newSkill.Name))
                    {
                        exist = true;
                        int level = baseSkill.Level + newSkill.Level;
                        if (level > maxLevel)
                        {
                            level = maxLevel;
                        }
                        baseSkill.Level = level;
                    }
                }
                if (!exist)
                {
                    baseSkills.Add(new Skill(newSkill.Name, newSkill.Level));
                }
            }
            return baseSkills;
        }

        // スキルの追加(同名スキルはスキルレベルを加算、錬成の追加スキルは除外)
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
