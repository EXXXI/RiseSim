using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Prism.Mvvm;
using Reactive.Bindings;
using SimModel.Model;

namespace RiseSim.ViewModels.Controls
{
    internal class SkillPickerContainerViewModel : BindableBase, IDisposable
    {
        /// <summary>
        /// Expanderに設定するタイトル。スキルカテゴリを設定する想定
        /// </summary>
        public string Header { get; init; }
        public ReactivePropertySlim<ObservableCollection<SkillPickerSelectorViewModel>> SkillPickerSelectors { get; init; }

        public SkillPickerContainerViewModel(string categoryName, IEnumerable<Skill> skills)
        {
            Header = categoryName;
            SkillPickerSelectors = new ReactivePropertySlim<ObservableCollection<SkillPickerSelectorViewModel>>
        /// <summary>
        /// 特定のスキルを担当するSkillPickerSelectorのスキルを設定する
        /// </summary>
        /// <param name="skill"></param>
        public void SetPickerSelected(Skill skill)
        {
            if(skill.Category != Header) return;
            // 特定のスキルを担当するSkillPickerSelectorは一つしかないのでFirstで良い
            var selectorViewModel = SkillPickerSelectors.FirstOrDefault(s => s.SelectedSkill.Value.Name == skill.Name);
            if(selectorViewModel is null) return;
            selectorViewModel.SelectedSkill.Value = skill;
        }
    private bool disposed;
        protected virtual void Dispose(bool disposing)
        {
            if(disposed) return;
            if (!disposing) return;

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
