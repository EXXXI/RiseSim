using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        /// <summary>
        /// ComboBoxの背景色
        /// </summary>
        public Color BackgroundColor { get; set; } = Color.White;

        public SkillPickerSelectorViewModel(Skill skill)
        {
            Items = new ReactivePropertySlim<ObservableCollection<Skill>>
            {
                Value = new ObservableCollection<Skill>(
                    Enumerable.Range(0, skill.Level).Select(level => skill with { Level = level })
                )
            };

            SelectedSkill = new ReactivePropertySlim<Skill> { Value = Items.Value.First() };
            SelectedSkill.Subscribe(selected =>
            {
                // Lvが0以外(=選択したとき)は背景色を黄色にする
                BackgroundColor = selected.Level == 0 ? Color.White : Color.Yellow;
            });
        }
    }
}
