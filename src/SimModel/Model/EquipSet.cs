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

                return sb.ToString();
            }
        }

        // 装備のIndex(頭、胴、腕、腰、足、護石の順に全装備に振った連番)リスト
        public List<int> EquipIndexsWithOutDecos
        {
            get
            {
                List<int> list = new();
                if (!string.IsNullOrWhiteSpace(Head.Name))
                {
                    list.Add(Masters.GetEquipIndexByName(Head.Name));
                }
                if (!string.IsNullOrWhiteSpace(Body.Name))
                {
                    list.Add(Masters.GetEquipIndexByName(Body.Name));
                }
                if (!string.IsNullOrWhiteSpace(Arm.Name))
                {
                    list.Add(Masters.GetEquipIndexByName(Arm.Name));
                }
                if (!string.IsNullOrWhiteSpace(Waist.Name))
                {
                    list.Add(Masters.GetEquipIndexByName(Waist.Name));
                }
                if (!string.IsNullOrWhiteSpace(Leg.Name))
                {
                    list.Add(Masters.GetEquipIndexByName(Leg.Name));
                }
                if (!string.IsNullOrWhiteSpace(Charm.Name))
                {
                    list.Add(Masters.GetEquipIndexByName(Charm.Name));
                }
                return list;
            }
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
                    Equipment? deco = Masters.GetEquipByName(decoName);
                    if (deco != null)
                    {
                        Decos.Add(deco);
                    }
                }
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
                    sb.Append(skill.Description);
                    first = false;
                }
                return sb.ToString();
            }
        }

        public string Description
        {
            get
            {
                StringBuilder sb = new();
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
                sb.Append('\n');
                sb.Append("-----------");
                foreach (var skill in Skills)
                {
                    sb.Append('\n');
                    sb.Append(skill.Description);
                }
                return sb.ToString();
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
