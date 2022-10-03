/*    RiseSim : MHRise skill simurator for Windows
 *    Copyright (C) 2022  EXXXI
 *
 *    This program is free software: you can redistribute it and/or modify
 *    it under the terms of the GNU General Public License as published by
 *    the Free Software Foundation, either version 3 of the License, or
 *    (at your option) any later version.
 *
 *    This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 *    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *    GNU General Public License for more details.
 *
 *    You should have received a copy of the GNU General Public License
 *    along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
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

        internal static void RefreshEquipmentMasters()
        {
            Heads = MakeEquipmentMaster(OriginalHeads, EquipKind.head);
            Bodys = MakeEquipmentMaster(OriginalBodys, EquipKind.body);
            Arms = MakeEquipmentMaster(OriginalArms, EquipKind.arm);
            Waists = MakeEquipmentMaster(OriginalWaists, EquipKind.waist);
            Legs = MakeEquipmentMaster(OriginalLegs, EquipKind.leg);
        }

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
                    Equipment baseEquip = GetEquipByName(aug.BaseName);
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
            string? name = equipName?.Trim();

            foreach (var equip in Heads)
            {
                if (equip.Name == name)
                {
                    return equip;
                }
            }
            foreach (var equip in Bodys)
            {
                if (equip.Name == name)
                {
                    return equip;
                }
            }
            foreach (var equip in Arms)
            {
                if (equip.Name == name)
                {
                    return equip;
                }
            }
            foreach (var equip in Waists)
            {
                if (equip.Name == name)
                {
                    return equip;
                }
            }
            foreach (var equip in Legs)
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
            
            return new Equipment();
        }

        // 装備名から装備のIndex(頭、胴、腕、腰、足、護石の順に全装備に振った連番)を取得
        public static int GetEquipIndexByName(string name)
        {
            int index = 0;
            foreach (var equip in Heads)
            {
                if (equip.Name == name)
                {
                    return index;
                }
                index++;
            }
            foreach (var equip in Bodys)
            {
                if (equip.Name == name)
                {
                    return index;
                }
                index++;
            }
            foreach (var equip in Arms)
            {
                if (equip.Name == name)
                {
                    return index;
                }
                index++;
            }
            foreach (var equip in Waists)
            {
                if (equip.Name == name)
                {
                    return index;
                }
                index++;
            }
            foreach (var equip in Legs)
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
            throw new ArgumentException();
        }
    }
}
