using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Prism.Mvvm;
using RiseSim.ViewModels.BindableWrapper;
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
        //public void SetPickerSelected(Skill skill)
        //{
        //    if (skill.Category != Header) 
        //    {
        //        return;
        //    }
        //    // 特定のスキルを担当するSkillPickerSelectorは一つしかないのでFirstで良い
        //    var selectorViewModel = SkillPickerSelectors.FirstOrDefault(s => s.SelectedSkill.Value.Name == skill.Name);
        //    if (selectorViewModel is null)
        //    {
        //        return;
        //    }
        //    selectorViewModel.SelectedSkill.Value = skill;
        //}

        /// <summary>
        /// このコンテナにあるSelectorのうち選択されているスキルの配列を返す
        /// </summary>
        /// <returns></returns>
        public IReadOnlyList<Skill> SelectedSkills() => 
            SkillPickerSelectors
                .Where(s => s.Selected)
                .Select(s => new Skill(s.SelectedSkill.Value.Name, s.SelectedSkill.Value.Level, isFixed:s.IsFix))
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

        internal void ClearAll()
        {
            foreach (var vm in SkillPickerSelectors)
            {
                vm.Reset();
            }
        }

        internal bool TryAddSkill(string name, int level)
        {
            foreach (var vm in SkillPickerSelectors)
            {
                if (vm.SelectedSkill.Value.Name == name)
                {
                    vm.SelectLevel(level);
                    return true;
                }
            }
            return false;
        }

        internal bool TryAddSkill(List<Skill> skills)
        {
            bool addedAny = false;
            foreach (var skill in skills)
            {
                bool added = TryAddSkill(skill.Name, skill.Level);
                addedAny = addedAny || added;
            }
            return addedAny;
        }
    }
}
