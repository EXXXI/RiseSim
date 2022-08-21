using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Prism.Mvvm;
using Reactive.Bindings;
using SimModel.Model;

namespace RiseSim.ViewModels.Controls
{
    internal class SkillPickerContainerViewModel : BindableBase
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
            {
                Value = new ObservableCollection<SkillPickerSelectorViewModel>(skills.Select(x =>
                    new SkillPickerSelectorViewModel(x)))
            };
        }
    }
}
