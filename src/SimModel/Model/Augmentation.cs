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
    public class Augmentation
    {
        // 管理用装備名(GUID)
        public string Name { get; set; } = string.Empty;

        // 表示用装備名
        public string DispName { get; set; } = string.Empty;

        // 装備種類
        public EquipKind Kind { get; set; }

        // 装備種類(文字列)
        public string KindStr {
            get
            { 
                return Kind.Str();
            }
        }

        // ベース装備名
        public string BaseName { get; set; } = string.Empty;

        // スロット1つ目
        public int Slot1 { get; set; }

        // スロット2つ目
        public int Slot2 { get; set; }

        // スロット3つ目
        public int Slot3 { get; set; }

        // スロット表示
        public string SlotDisp { 
            get
            { 
                return Slot1 + "-" + Slot2 + "-" + Slot3;
            }
        }

        // 防御力増減
        public int Def { get; set; }

        // 火耐性増減
        public int Fire { get; set; }

        // 水耐性増減
        public int Water { get; set; }

        // 雷耐性増減
        public int Thunder { get; set; }

        // 氷耐性増減
        public int Ice { get; set; }

        // 龍耐性増減
        public int Dragon { get; set; }

        // 追加スキル
        public List<Skill> Skills { get; set; } = new();

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
    }
}
