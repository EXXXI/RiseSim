using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimModel.Model
{
    // 各種マスタ管理
    static public class Masters
    {
        // スキルマスタ
        public static List<Skill> Skills { get; set; } = new();

        // 理想錬成用スキルマスタ
        public static List<Equipment> GenericSkills { get; set; } = new();

        // 頭装備マスタ
        public static List<Equipment> OriginalHeads { get; set; } = new();

        // 胴装備マスタ
        public static List<Equipment> OriginalBodys { get; set; } = new();

        // 腕装備マスタ
        public static List<Equipment> OriginalArms { get; set; } = new();

        // 腰装備マスタ
        public static List<Equipment> OriginalWaists { get; set; } = new();

        // 足装備マスタ
        public static List<Equipment> OriginalLegs { get; set; } = new();

        // 錬成装備情報マスタ
        public static List<Augmentation> Augmentations { get; set; } = new();

        // 理想錬成装備情報マスタ
        public static List<IdealAugmentation> Ideals { get; set; } = new();

        // 頭装備マスタ(錬成防具入り)
        public static List<Equipment> Heads { get; set; } = new();

        // 胴装備マスタ(錬成防具入り)
        public static List<Equipment> Bodys { get; set; } = new();

        // 腕装備マスタ(錬成防具入り)
        public static List<Equipment> Arms { get; set; } = new();

        // 腰装備マスタ(錬成防具入り)
        public static List<Equipment> Waists { get; set; } = new();

        // 足装備マスタ(錬成防具入り)
        public static List<Equipment> Legs { get; set; } = new();

        // 頭装備マスタ(理想錬成防具入り)
        public static List<Equipment> IdealHeads { get; set; } = new();

        // 胴装備マスタ(理想錬成防具入り)
        public static List<Equipment> IdealBodys { get; set; } = new();

        // 腕装備マスタ(理想錬成防具入り)
        public static List<Equipment> IdealArms { get; set; } = new();

        // 腰装備マスタ(理想錬成防具入り)
        public static List<Equipment> IdealWaists { get; set; } = new();

        // 足装備マスタ(理想錬成防具入り)
        public static List<Equipment> IdealLegs { get; set; } = new();

        // 護石マスタ
        public static List<Equipment> Charms { get; set; } = new();

        // 装飾品マスタ
        public static List<Equipment> Decos { get; set; } = new();

        // 除外固定マスタ
        public static List<Clude> Cludes { get; set; } = new();

        // マイセットマスタ
        public static List<EquipSet> MySets { get; set; } = new();

        // 最近使ったスキルマスタ
        public static List<string> RecentSkillNames { get; set; } = new();

        // マイ検索条件マスタ
        public static List<SearchCondition> MyConditions { get; set; } = new();

        // 錬成装備情報を各装備マスタに反映
        internal static void RefreshEquipmentMasters()
        {
            Heads = MakeEquipmentMaster(OriginalHeads, EquipKind.head);
            Bodys = MakeEquipmentMaster(OriginalBodys, EquipKind.body);
            Arms = MakeEquipmentMaster(OriginalArms, EquipKind.arm);
            Waists = MakeEquipmentMaster(OriginalWaists, EquipKind.waist);
            Legs = MakeEquipmentMaster(OriginalLegs, EquipKind.leg);

            IdealHeads = MakeIdealEquipmentMaster(Heads, EquipKind.head);
            IdealBodys = MakeIdealEquipmentMaster(Bodys, EquipKind.body);
            IdealArms = MakeIdealEquipmentMaster(Arms, EquipKind.arm);
            IdealWaists = MakeIdealEquipmentMaster(Waists, EquipKind.waist);
            IdealLegs = MakeIdealEquipmentMaster(Legs, EquipKind.leg);
        }

        // 理想錬成防具を理想錬成検索用の装備マスタに登録
        private static List<Equipment> MakeIdealEquipmentMaster(List<Equipment> equips, EquipKind head)
        {
            List<Equipment> idealEquips = new();
            foreach (Equipment equip in equips)
            {
                idealEquips.Add(equip);
                // 錬成済み(ベース防具がある)の防具の場合追加処理なし
                if (equip.BaseEquipment != null)
                {
                    continue;
                }
                foreach (IdealAugmentation ideal in Ideals)
                {
                    // テーブルが条件に合致した場合に理想錬成の内容を反映した防具を登録する
                    if (ideal.Table == equip.AugmentationTable ||
                        (ideal.IsIncludeLower && equip.AugmentationTable < ideal.Table))
                    {
                        // スキルマイナスのパターン生成
                        int baseSkillCount = equip.Skills.Count;
                        List<List<int>> minusPatterns = MakeMinusPatterns(ideal.SkillMinuses, baseSkillCount, 0);

                        int patternIndex = 0;
                        foreach (List<int> minusPattern in minusPatterns)
                        {
                            Equipment newEquip = new Equipment(equip);
                            newEquip.Name = equip.Name + "_" + patternIndex + "_" + ideal.Name;
                            newEquip.DispName = equip.DispName + "_" + ideal.DispName + "(" + patternIndex++ + ")";
                            /*
                            newEquip.Mindef += ideal.Def;
                            newEquip.Maxdef += ideal.Def;
                            newEquip.Fire += ideal.Fire;
                            newEquip.Water += ideal.Water;
                            newEquip.Thunder += ideal.Thunder;
                            newEquip.Ice += ideal.Ice;
                            newEquip.Dragon += ideal.Dragon;
                            */
                            int[] slot = new int[3];
                            slot[0] = equip.Slot1;
                            slot[1] = equip.Slot2;
                            slot[2] = equip.Slot3;
                            slot = CalcIdealSlot(slot, ideal.SlotIncrement);
                            newEquip.Slot1 = slot[0];
                            newEquip.Slot2 = slot[1];
                            newEquip.Slot3 = slot[2];
                            foreach (var skill in ideal.Skills)
                            {
                                newEquip.Skills.Add(skill);
                            }
                            foreach (var index in minusPattern)
                            {
                                int minusSkillIndex = (index - 1) % baseSkillCount;
                                newEquip.Skills.Add(new Skill(equip.Skills[minusSkillIndex].Name, -1, true));
                            }
                            newEquip.GenericSkills[0] = ideal.GenericSkills[0];
                            newEquip.GenericSkills[1] = ideal.GenericSkills[1];
                            newEquip.GenericSkills[2] = ideal.GenericSkills[2];
                            newEquip.GenericSkills[3] = ideal.GenericSkills[3];
                            newEquip.GenericSkills[4] = ideal.GenericSkills[4];
                            newEquip.BaseEquipment = equip;
                            newEquip.Ideal = ideal;

                            // スキルのバリデーション
                            // スキル値がマイナスのスキルがある場合、そのパターンは却下
                            List<Skill> margedSkills = newEquip.MargedSkills;
                            bool isValid = true;
                            foreach (var skill in margedSkills)
                            {
                                if (skill.Level < 0)
                                {
                                    isValid = false;
                                }
                            }
                            if (!isValid)
                            {
                                continue;
                            }

                            // スキル数が6以上の場合、そのパターンは却下
                            List<string> skillList = new();
                            // ベース防具スキルは全てカウント
                            foreach (var skill in equip.Skills)
                            {
                                if (skill.Level > 0 && !skillList.Contains(skill.Name))
                                {
                                    skillList.Add(skill.Name);
                                }
                            }
                            // 適用後のスキルをカウント
                            foreach (var skill in margedSkills)
                            {
                                if (skill.Level > 0 && !skillList.Contains(skill.Name))
                                {
                                    skillList.Add(skill.Name);
                                }
                            }
                            if (skillList.Count > 5)
                            {
                                continue;
                            }

                            idealEquips.Add(newEquip);
                        }
                    }
                }
            }
            return idealEquips;
        }

        // 理想錬成のスキルマイナスパターン計算
        private static List<List<int>> MakeMinusPatterns(List<int> skillMinuses, int baseSkillCount, int minBaseSkillIdx)
        {
            List<List<int>> result = new();
            int firstZero = -1;
            for (int i = 0; i < skillMinuses.Count; i++)
            {
                if (skillMinuses[i] == 0)
                {
                    firstZero = i;
                }
            }

            if (firstZero < 0)
            {
                // 位置指定なしのスキルマイナスがないためコピーをそのまま返す
                result.Add(new List<int>(skillMinuses));
            }
            else
            {
                // 位置指定を全パターン試して結合して返す
                for (int i = minBaseSkillIdx; i < baseSkillCount; i++)
                {
                    List<int> subList = new List<int>(skillMinuses);
                    subList[firstZero] = i + 1;
                    List<List<int>> subResult = MakeMinusPatterns(subList, baseSkillCount, i);
                    result.AddRange(subResult);
                }
            }
            return result;
        }

        // 理想錬成のスロット計算
        private static int[] CalcIdealSlot(int[] slot, int slotIncrement)
        {
            // slotIncrementが0の場合もう増やせないのでそのまま返す
            if (slotIncrement < 1)
            {
                return slot;
            }

            // 左から、0のスロットがある場合1にして次へ
            for (int i = 0; i < slot.Length; i++)
            {
                if (slot[i] < 1)
                {
                    slot[i] = 1;
                    return CalcIdealSlot(slot, slotIncrement - 1);
                }
            }

            // 全て1以上
            // 左から、4未満のスロットがある場合+1して次へ
            for (int i = 0; i < slot.Length; i++)
            {
                if (slot[i] < 4)
                {
                    slot[i]++;
                    return CalcIdealSlot(slot, slotIncrement - 1);
                }
            }

            // 全て4
            // そのまま返す
            return slot;
        }

        // 錬成防具を装備マスタに反映
        private static List<Equipment> MakeEquipmentMaster(List<Equipment> originalEquips, EquipKind kind)
        {
            List<Equipment> equips = new();
            foreach (Equipment equip in originalEquips)
            {
                equips.Add(equip);
            }
            foreach (Augmentation aug in Augmentations)
            {
                if (aug.Kind == kind)
                {
                    Equipment baseEquip = GetEquipByName(aug.BaseName, false);
                    Equipment newEquip = new Equipment(baseEquip);
                    newEquip.Name = aug.Name;
                    newEquip.DispName = aug.DispName;
                    newEquip.Mindef += aug.Def;
                    newEquip.Maxdef += aug.Def;
                    newEquip.Fire += aug.Fire;
                    newEquip.Water += aug.Water;
                    newEquip.Thunder += aug.Thunder;
                    newEquip.Ice += aug.Ice;
                    newEquip.Dragon += aug.Dragon;
                    newEquip.Slot1 = aug.Slot1;
                    newEquip.Slot2 = aug.Slot2;
                    newEquip.Slot3 = aug.Slot3;
                    foreach (var skill in aug.Skills)
                    {
                        newEquip.Skills.Add(skill);
                    }
                    newEquip.BaseEquipment = baseEquip;
                    equips.Add(newEquip);
                }
            }
            return equips;
        }

        // 装備名から装備を取得
        public static Equipment GetEquipByName(string equipName)
        {
            return GetEquipByName(equipName, true);
        }

        // 装備名から装備を取得
        public static Equipment GetEquipByName(string equipName, bool useIdeal)
        {
            string? name = equipName?.Trim();
            List<Equipment> heads, bodys, arms, waists, legs;
            if (useIdeal)
            {
                heads = IdealHeads;
                bodys = IdealBodys;
                arms = IdealArms;
                waists = IdealWaists;
                legs = IdealLegs;
            }
            else
            {
                heads = Heads;
                bodys = Bodys;
                arms = Arms;
                waists = Waists;
                legs = Legs;
            }

            foreach (var equip in heads)
            {
                if (equip.Name == name)
                {
                    return equip;
                }
            }
            foreach (var equip in bodys)
            {
                if (equip.Name == name)
                {
                    return equip;
                }
            }
            foreach (var equip in arms)
            {
                if (equip.Name == name)
                {
                    return equip;
                }
            }
            foreach (var equip in waists)
            {
                if (equip.Name == name)
                {
                    return equip;
                }
            }
            foreach (var equip in legs)
            {
                if (equip.Name == name)
                {
                    return equip;
                }
            }
            foreach (var equip in Charms)
            {
                if (equip.Name == name)
                {
                    return equip;
                }
            }
            foreach (var equip in Decos)
            {
                if (equip.Name == name)
                {
                    return equip;
                }
            }
            foreach (var equip in GenericSkills)
            {
                if (equip.Name == name)
                {
                    return equip;
                }
            }

            return new Equipment();
        }

        // 装備名から装備のIndex(頭、胴、腕、腰、足、護石の順に全装備に振った連番)を取得
        // 理想錬成の除外固定検索用
        // 装備そのものとその理想錬成を全て返す
        public static List<int> GetCludeIndexsByName(string name, bool includeIdealAugmentation)
        {
            List<int> result = new();
            List<Equipment> heads, bodys, arms, waists, legs;
            if (includeIdealAugmentation)
            {
                heads = IdealHeads;
                bodys = IdealBodys;
                arms = IdealArms;
                waists = IdealWaists;
                legs = IdealLegs;
            }
            else
            {
                heads = Heads;
                bodys = Bodys;
                arms = Arms;
                waists = Waists;
                legs = Legs;
            }

            int index = 0;
            foreach (var equip in heads)
            {
                if (equip.Name == name ||
                    (equip.Ideal != null && equip.BaseEquipment?.Name == name))
                {
                    result.Add(index);
                }
                index++;
            }
            foreach (var equip in bodys)
            {
                if (equip.Name == name ||
                    (equip.Ideal != null && equip.BaseEquipment?.Name == name))
                {
                    result.Add(index);
                }
                index++;
            }
            foreach (var equip in arms)
            {
                if (equip.Name == name ||
                    (equip.Ideal != null && equip.BaseEquipment?.Name == name))
                {
                    result.Add(index);
                }
                index++;
            }
            foreach (var equip in waists)
            {
                if (equip.Name == name ||
                    (equip.Ideal != null && equip.BaseEquipment?.Name == name))
                {
                    result.Add(index);
                }
                index++;
            }
            foreach (var equip in legs)
            {
                if (equip.Name == name ||
                    (equip.Ideal != null && equip.BaseEquipment?.Name == name))
                {
                    result.Add(index);
                }
                index++;
            }
            foreach (var equip in Charms)
            {
                if (equip.Name == name ||
                    (equip.Ideal != null && equip.BaseEquipment?.Name == name))
                {
                    result.Add(index);
                }
                index++;
            }
            foreach (var equip in Decos)
            {
                if (equip.Name == name ||
                    (equip.Ideal != null && equip.BaseEquipment?.Name == name))
                {
                    result.Add(index);
                }
                index++;
            }
            foreach (var equip in GenericSkills)
            {
                if (equip.Name == name ||
                    (equip.Ideal != null && equip.BaseEquipment?.Name == name))
                {
                    result.Add(index);
                }
                index++;
            }
            return result;
        }

        // 装備名から装備のIndex(頭、胴、腕、腰、足、護石の順に全装備に振った連番)を取得
        public static int GetEquipIndexByName(string name, bool includeIdealAugmentation)
        {
            List<Equipment> heads, bodys, arms, waists, legs;
            if (includeIdealAugmentation)
            {
                heads = IdealHeads;
                bodys = IdealBodys;
                arms = IdealArms;
                waists = IdealWaists;
                legs = IdealLegs;
            }
            else
            {
                heads = Heads;
                bodys = Bodys;
                arms = Arms;
                waists = Waists;
                legs = Legs;
            }

            int index = 0;
            foreach (var equip in heads)
            {
                if (equip.Name == name)
                {
                    return index;
                }
                index++;
            }
            foreach (var equip in bodys)
            {
                if (equip.Name == name)
                {
                    return index;
                }
                index++;
            }
            foreach (var equip in arms)
            {
                if (equip.Name == name)
                {
                    return index;
                }
                index++;
            }
            foreach (var equip in waists)
            {
                if (equip.Name == name)
                {
                    return index;
                }
                index++;
            }
            foreach (var equip in legs)
            {
                if (equip.Name == name)
                {
                    return index;
                }
                index++;
            }
            foreach (var equip in Charms)
            {
                if (equip.Name == name)
                {
                    return index;
                }
                index++;
            }
            foreach (var equip in Decos)
            {
                if (equip.Name == name)
                {
                    return index;
                }
                index++;
            }
            foreach (var equip in GenericSkills)
            {
                if (equip.Name == name)
                {
                    return index;
                }
                index++;
            }
            throw new ArgumentException();
        }

        // 錬成防具のデフォルト名作成
        public static string MakeAugmentaionDefaultDispName(string baseName)
        {
            bool isExist = true;
            string name = baseName + "_" + 0;
            for (int i = 1; isExist; i++)
            {
                isExist = false;
                name = baseName + "_" + i;
                foreach (var aug in Augmentations)
                {
                    if (aug.DispName == name)
                    {
                        isExist = true;
                        break;
                    }
                }
            }
            return name;
        }

        // 理想錬成防具のデフォルト名作成
        public static string MakeIdealAugmentaionDefaultDispName(int table)
        {
            bool isExist = true;
            string name = "T" + table + "_" + 0;
            for (int i = 1; isExist; i++)
            {
                isExist = false;
                name = "T" + table + "_" + i;
                foreach (var ideal in Ideals)
                {
                    if (ideal.DispName == name)
                    {
                        isExist = true;
                        break;
                    }
                }
            }
            return name;
        }
    }
}
