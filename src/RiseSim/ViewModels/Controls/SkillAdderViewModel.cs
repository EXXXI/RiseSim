using Prism.Mvvm;
using Reactive.Bindings;
using RiseSim.ViewModels.BindableWrapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiseSim.ViewModels.Controls
{
    internal class SkillAdderViewModel : BindableBase
    {
        public ReactivePropertySlim<string> Name { get; } = new();

        public ReactivePropertySlim<ObservableCollection<int>> Range { get; } = new();

        // コマンド
        public ReactiveCommand AddCommand { get; } = new ReactiveCommand();

        // コマンドを設定
        private void SetCommand()
        {
            AddCommand.Subscribe(level => AddSkill(level as int?));
        }

        private void AddSkill(int? level)
        {
            if (level == null)
            {
                return;
            }
            MainViewModel.Instance.AddSkill(Name.Value, (int)level);
        }

        public SkillAdderViewModel(string name, IEnumerable<int> range) 
        {
            Name.Value = name;
            Range.Value = new ObservableCollection<int>(range);

            SetCommand();
        }
    }
}
