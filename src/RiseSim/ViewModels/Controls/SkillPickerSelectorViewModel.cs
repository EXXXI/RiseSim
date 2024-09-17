using System;
using System.Collections.ObjectModel;
using System.Linq;
using Prism.Mvvm;
using Reactive.Bindings;
using RiseSim.ViewModels.BindableWrapper;
using SimModel.Model;

namespace RiseSim.ViewModels.Controls
{
    internal class SkillPickerSelectorViewModel : BindableBase, IDisposable
    {
        const string FixStr = "固定";
        const string NotFixStr = "以上";

        /// <summary>
        /// ComboBox用のデータソース
        /// </summary>
        public ObservableCollection<BindableSkill> Items { get; init; }

        /// <summary>
        /// 選択されているスキル
        /// </summary>
        public ReactivePropertySlim<BindableSkill> SelectedSkill { get; set; }

        // スキル値固定の表示候補
        public ReactivePropertySlim<ObservableCollection<string>> IsFixDisps { get; } = new();

        // スキル値固定の表示内容
        public ReactivePropertySlim<string> IsFixDisp { get; } = new();

        // スキル値固定状態
        public bool IsFix { get; set; } = false;

        private bool skillSelected;

        public bool Selected
        {
            get => skillSelected;
            set => SetProperty(ref skillSelected, value);
        }

        public SkillPickerSelectorViewModel(Skill skill)
        {
            // スキル値固定関連準備
            IsFixDisps.Value = new();
            IsFixDisps.Value.Add(NotFixStr);
            IsFixDisps.Value.Add(FixStr);
            IsFixDisp.Value = NotFixStr;

            Items = new ObservableCollection<BindableSkill>(Enumerable.Range(0, skill.Level + 1)
                .Select(level => new BindableSkill(skill) { Level = level }));

            SelectedSkill = new ReactivePropertySlim<BindableSkill> { Value = Items.First() };
            SelectedSkill.Subscribe(selected =>
            {
                Selected = SelectedSkill.Value.Level != 0 || IsFix;
            });

            IsFixDisp.Subscribe(_ => 
            { 
                IsFix = IsFixDisp.Value == FixStr;
                Selected = SelectedSkill.Value.Level != 0 || IsFix;
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

        internal void Reset()
        {
            SelectLevel(0);
            IsFixDisp.Value = NotFixStr;
        }

        internal void SetIsFix(bool isFix)
        {
            IsFixDisp.Value = isFix ? FixStr : NotFixStr;
        }

        ~SkillPickerSelectorViewModel() => Dispose(false);

    }
}
