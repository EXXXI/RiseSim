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
    // 検索条件
    internal class SearchCondition
    {
        // スキルリスト
        public List<Skill> Skills { get; set; } = new();

        // 武器スロ1つ目
        public int WeaponSlot1 { get; set; }

        // 武器スロ2つ目
        public int WeaponSlot2 { get; set; }

        // 武器スロ3つ目
        public int WeaponSlot3 { get; set; }

        /* TODO: 今後追加するかも？
        public int Def { get; set; }
        public int? Fire { get; set; }
        public int? Water { get; set; }
        public int? Thunder { get; set; }
        public int? Ice { get; set; }
        public int? Dragon { get; set; }
        public Sex Sex { get; set; }
        */

        public SearchCondition()
        {
        }

        // コピーコンストラクタ
        public SearchCondition(SearchCondition condition)
        {
            Skills = new List<Skill>();
            foreach (var skill in condition.Skills)
            {
                Skill newSkill = new Skill(skill.Name, skill.Level);
                Skills.Add(newSkill);
            }
            WeaponSlot1 = condition.WeaponSlot1;
            WeaponSlot2 = condition.WeaponSlot2;
            WeaponSlot3 = condition.WeaponSlot3;
        }

        // スキル追加(同名スキルはレベルが高い方のみを採用)
        // 追加したスキルが有効かどうかを返す
        public bool AddSkill(Skill additionalSkill)
        {
            foreach (var skill in Skills)
            {
                if(skill.Name == additionalSkill.Name)
                {
                    if(skill.Level < additionalSkill.Level)
                    {
                        skill.Level = additionalSkill.Level;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            Skills.Add(additionalSkill);
            return true;
        }

    }

    
}
