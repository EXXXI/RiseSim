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

        internal void SelectLevel(int v)
        {
            SelectedSkill.Value = Items.Where(skill => skill.Level == v).First();
        }

        ~SkillPickerSelectorViewModel() => Dispose(false);

    }
}
