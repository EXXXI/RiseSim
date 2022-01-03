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
using Prism.Mvvm;
using SimModel.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiseSim.ViewModels
{
    class SkillSelectorViewModel : BindableBase
    {
        // TODO: 他のファイルでも利用しているので一元化したい
        // スキル未選択時の表示
        private const string NoSkillName = "スキル選択";

        // スキル名一覧
        private List<string> skills;
        public List<string> Skills
        {
            get { return this.skills; }
            set
            {
                this.SetProperty(ref this.skills, value);
            }
        }

        // 選択中スキルのレベル一覧
        private List<int> skillLevels;
        public List<int> SkillLevels
        {
            get { return this.skillLevels; }
            set
            {
                this.SetProperty(ref this.skillLevels, value);
            }
        }

        // 選択中スキル名
        private string skillName;
        public string SkillName
        {
            get { return this.skillName; }
            set
            {
                this.SetProperty(ref this.skillName, value);
            }
        }

        // 選択中スキルレベル
        private int skillLevel;
        public int SkillLevel
        {
            get { return this.skillLevel; }
            set
            {
                this.SetProperty(ref this.skillLevel, value);
            }
        }

        // 空行(「スキル選択」の行)を追加してスキルマスタを読み込み
        public SkillSelectorViewModel()
        {
            List<string> skillList = new List<string>();
            skillList.Add(NoSkillName);
            foreach (var skill in Masters.Skills)
            {
                skillList.Add(skill.Name);
            }
            Skills = skillList;
            SkillName = NoSkillName;
        }

        // 選択中スキル名にあわせてスキルレベルの選択肢を変更
        internal void SetLevels()
        {
            List<int> list = new List<int>();
            int maxLevel = 0;
            foreach (var skill in Masters.Skills)
            {
                if (skill.Name.Equals(SkillName))
                {
                    maxLevel = skill.Level;
                }
            }
            for (int i = maxLevel; i > 0; i--)
            {
                list.Add(i);
            }
            SkillLevels = list;

            if (list.Count > 0)
            {
                // 初期値は最大レベルとする
                SkillLevel = maxLevel;
            }
        }

        internal void SetDefault()
        {
            SkillName = NoSkillName;
        }
    }
}
