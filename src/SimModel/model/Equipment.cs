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
    // 装備
    public class Equipment
    {
        // 管理用装備名
        public string Name { get; set; } = "";

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

        // スキル
        public List<Skill> Skills { get; set; } = new();

        // 装備種類
        public EquipKind Kind { get; set; }

        // 表示用装備名(護石のみ特殊処理)
        public string DispName { 
            get
            {
                if (!Kind.Equals(EquipKind.charm))
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
                        sb.Append(' ');
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
        }
    }
}
