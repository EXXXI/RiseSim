using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RiseSim.ViewModels.Controls
{
    /// <summary>
    /// スキルとそのレベルの候補を表示し、選択したレベルをスキル選択画面に適用する部品
    /// </summary>
    internal class SkillAdderViewModel : ChildViewModelBase
    {
        /// <summary>
        /// スキル名
        /// </summary>
        public ReactivePropertySlim<string> Name { get; } = new();

        /// <summary>
        /// スキルレベル
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<int>> Range { get; } = new();

        /// <summary>
        /// 追加コマンド
        /// </summary>
        public ReactiveCommand AddCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="name">スキル名</param>
        /// <param name="range">スキルレベル</param>
        public SkillAdderViewModel(string name, IEnumerable<int> range)
        {
            Name.Value = name;
            Range.Value = new ObservableCollection<int>(range);

            // コマンドを設定
            AddCommand.Subscribe(level => AddSkill(level as int?));
        }

        /// <summary>
        /// 追加コマンド
        /// </summary>
        /// <param name="level"></param>
        private void AddSkill(int? level)
        {
            if (level == null)
            {
                return;
            }
            SkillSelectTabVM.AddSkill(Name.Value, (int)level);
        }
    }
}
