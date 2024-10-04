using System;
using System.Collections.Generic;
using System.Linq;

namespace SimModel.Model
{
    /// <summary>
    /// 各種マスタ管理
    /// </summary>
    static public class Masters
    {
        /// <summary>
        /// スキルマスタ
        /// </summary>
        public static List<Skill> Skills { get; set; } = new();

        /// <summary>
        /// 理想錬成用スキルマスタ
        /// </summary>
        public static List<Equipment> GenericSkills { get; set; } = new();

        /// <summary>
        /// 頭装備マスタ
        /// </summary>
        public static List<Equipment> OriginalHeads { get; set; } = new();

        /// <summary>
        /// 胴装備マスタ
        /// </summary>
        public static List<Equipment> OriginalBodys { get; set; } = new();

        /// <summary>
        /// 腕装備マスタ
        /// </summary>
        public static List<Equipment> OriginalArms { get; set; } = new();

        /// <summary>
        /// 腰装備マスタ
        /// </summary>
        public static List<Equipment> OriginalWaists { get; set; } = new();

        /// <summary>
        /// 足装備マスタ
        /// </summary>
        public static List<Equipment> OriginalLegs { get; set; } = new();

        /// <summary>
        /// 錬成装備情報マスタ
        /// </summary>
        public static List<Augmentation> Augmentations { get; set; } = new();

        /// <summary>
        /// 理想錬成装備情報マスタ
        /// </summary>
        public static List<IdealAugmentation> Ideals { get; set; } = new();

        /// <summary>
        /// 頭装備マスタ(錬成防具入り)
        /// </summary>
        public static List<Equipment> Heads { get; set; } = new();

        /// <summary>
        /// 胴装備マスタ(錬成防具入り)
        /// </summary>
        public static List<Equipment> Bodys { get; set; } = new();

        /// <summary>
        /// 腕装備マスタ(錬成防具入り)
        /// </summary>
        public static List<Equipment> Arms { get; set; } = new();

        /// <summary>
        /// 腰装備マスタ(錬成防具入り)
        /// </summary>
        public static List<Equipment> Waists { get; set; } = new();

        /// <summary>
        /// 足装備マスタ(錬成防具入り)
        /// </summary>
        public static List<Equipment> Legs { get; set; } = new();

        /// <summary>
        /// 頭装備マスタ(理想錬成防具入り)
        /// </summary>
        public static List<Equipment> IdealHeads { get; set; } = new();

        /// <summary>
        /// 胴装備マスタ(理想錬成防具入り)
        /// </summary>
        public static List<Equipment> IdealBodys { get; set; } = new();

        /// <summary>
        /// 腕装備マスタ(理想錬成防具入り)
        /// </summary>
        public static List<Equipment> IdealArms { get; set; } = new();

        /// <summary>
        /// 腰装備マスタ(理想錬成防具入り)
        /// </summary>
        public static List<Equipment> IdealWaists { get; set; } = new();

        /// <summary>
        /// 足装備マスタ(理想錬成防具入り)
        /// </summary>
        public static List<Equipment> IdealLegs { get; set; } = new();

        /// <summary>
        /// 護石マスタ
        /// </summary>
        public static List<Equipment> Charms { get; set; } = new();

        /// <summary>
        /// 装飾品マスタ
        /// </summary>
        public static List<Equipment> Decos { get; set; } = new();

        /// <summary>
        /// 除外固定マスタ
        /// </summary>
        public static List<Clude> Cludes { get; set; } = new();

        /// <summary>
        /// マイセットマスタ
        /// </summary>
        public static List<EquipSet> MySets { get; set; } = new();

        /// <summary>
        /// 最近使ったスキルマスタ
        /// </summary>
        public static List<string> RecentSkillNames { get; set; } = new();

        /// <summary>
        /// マイ検索条件マスタ
        /// </summary>
        public static List<SearchCondition> MyConditions { get; set; } = new();

        /// <summary>
        /// 錬成装備情報を各装備マスタに反映
        /// </summary>
        internal static void RefreshEquipmentMasters()
        {
            Heads = MakeEquipmentMaster(OriginalHeads, EquipKind.head);
            Bodys = MakeEquipmentMaster(OriginalBodys, EquipKind.body);
            Arms = MakeEquipmentMaster(OriginalArms, EquipKind.arm);
            Waists = MakeEquipmentMaster(OriginalWaists, EquipKind.waist);
            Legs = MakeEquipmentMaster(OriginalLegs, EquipKind.leg);

            IdealHeads = MakeIdealEquipmentMaster(Heads);
            IdealBodys = MakeIdealEquipmentMaster(Bodys);
            IdealArms = MakeIdealEquipmentMaster(Arms);
            IdealWaists = MakeIdealEquipmentMaster(Waists);
            IdealLegs = MakeIdealEquipmentMaster(Legs);
        }

        /// <summary>
        /// 理想錬成防具を理想錬成検索用の装備マスタに登録
        /// </summary>
        /// <param name="equips">通常防具のリスト</param>
        /// <returns>錬成装備込みのリスト</returns>
        private static List<Equipment> MakeIdealEquipmentMaster(List<Equipment> equips)
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

        /// <summary>
        /// 理想錬成のスキルマイナスパターン計算
        /// </summary>
        /// <param name="skillMinuses">欠け位置(不明は0)</param>
        /// <param name="baseSkillCount">元防具のスキル数</param>
        /// <param name="minBaseSkillIdx">適用済みのIndex</param>
        /// <returns>スキルマイナスパターン</returns>
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

        /// <summary>
        /// 理想錬成のスロット計算
        /// </summary>
        /// <param name="slot">元のスロット</param>
        /// <param name="slotIncrement">追加量</param>
        /// <returns>追加後のスロット</returns>
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

        /// <summary>
        /// 錬成防具を装備マスタに反映
        /// </summary>
        /// <param name="originalEquips">通常防具</param>
        /// <param name="kind">防具部位</param>
        /// <returns>錬成防具込みのリスト</returns>
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

        /// <summary>
        /// 装備名から装備を取得
        /// </summary>
        /// <param name="equipName">装備名</param>
        /// <returns>装備</returns>
        public static Equipment GetEquipByName(string equipName)
        {
            return GetEquipByName(equipName, true);
        }

        /// <summary>
        /// 装備名から装備を取得
        /// </summary>
        /// <param name="equipName">装備名</param>
        /// <param name="useIdeal">理想錬成を検索対象にする場合true</param>
        /// <returns>装備</returns>
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

        /// <summary>
        /// 装備名から、装備そのものとその理想錬成の装備名を取得
        /// 理想錬成の除外固定検索用
        /// </summary>
        /// <param name="name">装備名</param>
        /// <param name="includeIdealAugmentation">理想錬成を検索対象にする場合true</param>
        /// <returns>取得結果</returns>

        public static List<string> GetCludeNamesByName(string name, bool includeIdealAugmentation)
        {
            List<string> result = new();
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

            // 全装備を検索
            var equips = heads.Union(bodys).Union(arms).Union(waists).Union(legs)
                .Union(Charms).Union(Decos).Union(GenericSkills);
            foreach (var equip in equips)
            {
                if (equip.Name == name ||
                    (equip.Ideal != null && equip.BaseEquipment?.Name == name))
                {
                    result.Add(equip.Name);
                }
            }

            return result;
        }

        /// <summary>
        /// 錬成防具のデフォルト名作成
        /// </summary>
        /// <param name="baseName">ベース防具名</param>
        /// <returns>デフォルト名</returns>
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

        /// <summary>
        /// 理想錬成防具のデフォルト名作成
        /// </summary>
        /// <param name="table">テーブル番号</param>
        /// <returns>デフォルト名</returns>
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

        /// <summary>
        /// スキル名から最大レベルを算出
        /// マスタに存在しないスキルの場合0
        /// </summary>
        /// <param name="name">スキル名</param>
        /// <returns>最大レベル</returns>
        public static int SkillMaxLevel(string name)
        {
            foreach (var skill in Skills)
            {
                if (skill.Name == name)
                {
                    return skill.Level;
                }
            }
            return 0;
        }

        /// <summary>
        /// スキル名からコストを算出
        /// </summary>
        /// <param name="name">スキル名</param>
        /// <returns>コスト</returns>
        public static int[]? SkillCost(string name)
        {
            foreach (var skill in GenericSkills)
            {
                if (skill.Skills[0].Name == name)
                {
                    return skill.GenericSkills;
                }
            }
            return null;
        }

        /// <summary>
        /// スキル名(cxx)からコストを算出
        /// </summary>
        /// <param name="name">スキル名(cxx)</param>
        /// <returns>コスト</returns>
        public static int[]? SkillCostByGSkillEquipName(string name)
        {
            foreach (var skill in GenericSkills)
            {
                if (skill.Name == name)
                {
                    return skill.GenericSkills;
                }
            }
            return null;
        }

        /// <summary>
        /// 正しいスキル名かどうかを判定
        /// </summary>
        /// <param name="name">スキル名</param>
        /// <returns>マスタに存在する場合true</returns>
        public static bool IsSkillName(string name)
        {
            foreach (var skill in Skills)
            {
                if (skill.Name == name)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
