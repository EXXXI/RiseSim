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
using System.Collections.ObjectModel;
using System.Linq;
using Prism.Mvvm;
using SimModel.Model;

namespace RiseSim.ViewModels.Controls
{
    internal class SkillPickerContainerViewModel : BindableBase, IDisposable
    {
        /// <summary>
        /// Expanderに設定するタイトル。スキルカテゴリを設定する想定
        /// </summary>
        public string Header { get; init; }

        public ObservableCollection<SkillPickerSelectorViewModel> SkillPickerSelectors { get; init; }

        public SkillPickerContainerViewModel(string categoryName, IEnumerable<Skill> skills)
        {
            Header = categoryName;
            SkillPickerSelectors = new ObservableCollection<SkillPickerSelectorViewModel>(skills.Select(x =>
                new SkillPickerSelectorViewModel(x)));
        }

        /// <summary>
        /// 特定のスキルを担当するSkillPickerSelectorのスキルを設定する
        /// </summary>
        /// <param name="skill"></param>
        public void SetPickerSelected(Skill skill)
        {
            if (skill.Category != Header) 
            {
                return;
            }
            // 特定のスキルを担当するSkillPickerSelectorは一つしかないのでFirstで良い
            var selectorViewModel = SkillPickerSelectors.FirstOrDefault(s => s.SelectedSkill.Value.Name == skill.Name);
            if (selectorViewModel is null)
            {
                return;
            }
            selectorViewModel.SelectedSkill.Value = skill;
        }

        /// <summary>
        /// このコンテナにあるSelectorのうち選択されているスキルの配列を返す
        /// </summary>
        /// <returns></returns>
        public IReadOnlyList<Skill> SelectedSkills() => 
            SkillPickerSelectors
                .Where(s => s.Selected)
                .Select(s => s.SelectedSkill.Value)
                .ToList();

    private bool disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }
            if (!disposing)
            {
                return;
            }

            foreach (var skillPickerSelectorViewModel in SkillPickerSelectors)
            {
                skillPickerSelectorViewModel.Dispose();
            }

            disposed = true;
        }

        ~SkillPickerContainerViewModel() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
