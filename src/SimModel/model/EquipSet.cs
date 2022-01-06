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

namespace SimModel.model
{
    // 装備セット
    public class EquipSet
    {
        // 頭装備名
        public string HeadName { get; set; } = "";

        // 胴装備名
        public string BodyName { get; set; } = "";

        // 腕装備名
        public string ArmName { get; set; } = "";

        // 腰装備名
        public string WaistName { get; set; } = "";

        // 足装備名
        public string LegName { get; set; } = "";

        // 護石名
        public string CharmName { get; set; } = "";

        // 装飾品名(リスト)
        public List<string> DecoNames { get; set; } = new();

        // 武器スロ1つ目
        public int WeaponSlot1 { get; set; }

        // 武器スロ2つ目
        public int WeaponSlot2 { get; set; }

        // 武器スロ3つ目
        public int WeaponSlot3 { get; set; }


        // 以下、Calcメソッド対象


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

        // スキル(リスト)
        public List<Skill> Skills { get; set; } = new();


        // CSV表記(装飾品を除く)
        public string SimpleSetNameWithoutDecos { 
            get 
            {
                StringBuilder sb = new();
                sb.Append(HeadName);
                sb.Append(',');
                sb.Append(BodyName);
                sb.Append(',');
                sb.Append(ArmName);
                sb.Append(',');
                sb.Append(WaistName);
                sb.Append(',');
                sb.Append(LegName);
                sb.Append(',');
                sb.Append(CharmName);

                return sb.ToString();
            }
        }

        // CSV表記
        public string SimpleSetName
        {
            get
            {
                StringBuilder sb = new();
                sb.Append(SimpleSetNameWithoutDecos);

                foreach (string name in DecoNames)
                {
                    sb.Append(',');
                    sb.Append(name);
                }

                return sb.ToString();
            }
        }

        // 装備のIndex(頭、胴、腕、腰、足、護石の順に全装備に振った連番)リスト
        public List<int> EquipIndexsWithOutDecos
        {
            get
            {
                List<int> list = new();
                if (!string.IsNullOrWhiteSpace(HeadName))
                {
                    list.Add(Masters.GetEquipIndexByName(HeadName));
                }
                if (!string.IsNullOrWhiteSpace(BodyName))
                {
                    list.Add(Masters.GetEquipIndexByName(BodyName));
                }
                if (!string.IsNullOrWhiteSpace(ArmName))
                {
                    list.Add(Masters.GetEquipIndexByName(ArmName));
                }
                if (!string.IsNullOrWhiteSpace(WaistName))
                {
                    list.Add(Masters.GetEquipIndexByName(WaistName));
                }
                if (!string.IsNullOrWhiteSpace(LegName))
                {
                    list.Add(Masters.GetEquipIndexByName(LegName));
                }
                if (!string.IsNullOrWhiteSpace(CharmName))
                {
                    list.Add(Masters.GetEquipIndexByName(CharmName));
                }
                return list;
            }
        }

        // 装飾品のCSV表記
        public string DecoNameCSV
        {
            get
            {
                StringBuilder sb = new();
                bool isFirst = true;
                foreach (var decoName in DecoNames)
                {
                    if (!isFirst)
                    {
                        sb.Append(',');
                    }
                    sb.Append(decoName);
                    isFirst = false;
                }
                return sb.ToString();
            }
            set
            {
                DecoNames = new List<string>();
                string[] splitted = value.Split(',');
                foreach (var decoName in splitted)
                {
                    DecoNames.Add(decoName);
                }
            }
        }

        // 護石の表示用装備名
        public string CharmNameDisp
        {
            get
            {
                if (string.IsNullOrWhiteSpace(CharmName))
                {
                    return "";
                }

                Equipment? charm = Masters.GetEquipByName(CharmName);
                if (charm == null)
                {
                    return "";
                }
                return charm.DispName;
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
                    if (!first)
                    {
                        sb.Append(", ");
                    }
                    sb.Append(skill.Name);
                    sb.Append("Lv");
                    sb.Append(skill.Level);
                    first = false;
                }
                return sb.ToString();
            }
        }

        // 計算項目の計算
        internal void Calc()
        {
            Mindef = 0;
            Maxdef = 0;
            Fire = 0;
            Water = 0;
            Thunder = 0;
            Ice = 0;
            Dragon = 0;
            Skills = new List<Skill>();
            CalcOneEquip(HeadName);
            CalcOneEquip(BodyName);
            CalcOneEquip(ArmName);
            CalcOneEquip(WaistName);
            CalcOneEquip(LegName);
            CalcOneEquip(CharmName);
            foreach (var decoName in DecoNames)
            {
                CalcOneEquip(decoName);
            }
            Skills.Sort((a, b) => b.Level - a.Level);

        }

        // 1防具の性能を反映
        private void CalcOneEquip(string name)
        {
            Equipment? equip = Masters.GetEquipByName(name);
            if (equip != null)
            {
                Mindef += equip.Mindef;
                Maxdef += equip.Maxdef;
                Fire += equip.Fire;
                Water += equip.Water;
                Thunder += equip.Thunder;
                Ice += equip.Ice;
                Dragon += equip.Dragon;
                JoinSkill(Skills, equip.Skills);
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
                foreach(var skill in Masters.Skills)
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
    }
}
