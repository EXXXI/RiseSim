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
using System.Collections.ObjectModel;
using System.Linq;
using Prism.Mvvm;
using Reactive.Bindings;
using SimModel.Model;

namespace RiseSim.ViewModels.Controls
{
    internal class SkillPickerSelectorViewModel : BindableBase, IDisposable
    {
        /// <summary>
        /// ComboBox用のデータソース
        /// </summary>
        public ObservableCollection<Skill> Items { get; init; }

        /// <summary>
        /// 選択されているスキル
        /// </summary>
        public ReactivePropertySlim<Skill> SelectedSkill { get; set; }

        private bool skillSelected;

        public bool Selected
        {
            get => skillSelected;
            set => SetProperty(ref skillSelected, value);
        }

        public SkillPickerSelectorViewModel(Skill skill)
        {

            Items = new ObservableCollection<Skill>(Enumerable.Range(0, skill.Level + 1)
                .Select(level => skill with { Level = level }));

            SelectedSkill = new ReactivePropertySlim<Skill> { Value = Items.First() };
            SelectedSkill.Subscribe(selected =>
            {
                Selected = selected.Level != 0;
            });
        }

        private bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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

            SelectedSkill.Dispose();

            disposed = true;
        }

        ~SkillPickerSelectorViewModel() => Dispose(false);

    }
}
