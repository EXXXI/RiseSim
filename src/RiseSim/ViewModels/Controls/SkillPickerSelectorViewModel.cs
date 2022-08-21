using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;
using Prism.Mvvm;
using Reactive.Bindings;
using SimModel.Model;

namespace RiseSim.ViewModels.Controls
{
    internal class SkillPickerSelectorViewModel : BindableBase
    {
        /// <summary>
        /// ComboBox用のデータソース
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<Skill>> Items { get; init; }

        /// <summary>
        /// 選択されているスキル
        /// </summary>
        public ReactivePropertySlim<Skill> SelectedSkill { get; set; }

        private bool skillSelected = false;

        public bool Selected
        {
            get => skillSelected;
            set => SetProperty(ref skillSelected, value);
        }

        public SkillPickerSelectorViewModel(Skill skill)
        {
            Items = new ReactivePropertySlim<ObservableCollection<Skill>>
            {
                Value = new ObservableCollection<Skill>(
                    Enumerable.Range(0, skill.Level + 1).Select(level => skill with { Level = level })
                )
            };

            SelectedSkill = new ReactivePropertySlim<Skill> { Value = Items.Value.First() };
            SelectedSkill.Subscribe(selected =>
            {
                Selected = selected.Level != 0;
            });
        }
    }
}
