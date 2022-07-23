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
using SimModel.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiseSim.ViewModels.BindableWrapper
{
    internal class BindableSkill : BindableBase
    {   
        // スキル名
        public string Name { get; }

        // スキルレベル
        public int Level { get; }

        // 追加スキルフラグ
        public bool IsAdditional { get; set; }

        // 説明
        public string Description { get; }

        // オリジナル
        public Skill Original { get; set; }

        // コンストラクタ
        public BindableSkill(Skill skill)
        {
            Name = skill.Name;
            Level = skill.Level;
            IsAdditional = skill.IsAdditional;
            Description = skill.Description;
            Original = skill;
        }

        // リストをまとめてバインド用クラスに変換
        static public ObservableCollection<BindableSkill> BeBindableList(List<Skill> list)
        {
            ObservableCollection<BindableSkill> bindableList = new ObservableCollection<BindableSkill>();
            foreach (var skill in list)
            {
                bindableList.Add(new BindableSkill(skill));
            }
            return bindableList;
        }
    }
}
