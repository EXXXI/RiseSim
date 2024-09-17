using Prism.Mvvm;
using Reactive.Bindings;
using RiseSim.ViewModels.BindableWrapper;
using SimModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiseSim.ViewModels.Controls
{
    internal class MyConditionRowViewModel : BindableBase
    {
        public BindableSearchCondition Condition { get; }

        public ReactivePropertySlim<bool> IsRenaming { get; } = new(false);

        public ReactivePropertySlim<bool> IsNotRenaming { get; } = new(true);

        public ReactivePropertySlim<string> InputName { get; } = new();

        // 名称変更開始コマンド
        public ReactiveCommand BeginRenameCommand { get; private set; }

        // 名称変更完了コマンド
        public ReactiveCommand ApplyRenameCommand { get; private set; }

        // 名称変更キャンセルコマンド
        public ReactiveCommand CancelRenameCommand { get; private set; }

        // 検索条件適用コマンド
        public ReactiveCommand ApplyConditionCommand { get; } = new();

        // 検索条件削除コマンド
        public ReactiveCommand DeleteConditionCommand { get; } = new();

        public MyConditionRowViewModel(SearchCondition condition)
        {
            Condition = new BindableSearchCondition(condition);
            InputName.Value = Condition.DispName.Value;

            IsRenaming.Subscribe(_ => IsNotRenaming.Value = !IsRenaming.Value);
            BeginRenameCommand = IsNotRenaming.ToReactiveCommand().WithSubscribe(() => IsRenaming.Value = true);
            ApplyRenameCommand = IsRenaming.ToReactiveCommand().WithSubscribe(() => ApplyRename());
            CancelRenameCommand = IsRenaming.ToReactiveCommand().WithSubscribe(() => CancelRename());
            ApplyConditionCommand.Subscribe(() => ApplyMyCondition());
            DeleteConditionCommand.Subscribe(() => DeleteCondition());
        }

        private void DeleteCondition()
        {
            MainViewModel.Instance.Simulator.DeleteMyCondition(Condition.Original);
            MainViewModel.Instance.LoadMyCondition();
        }

        private void ApplyMyCondition()
        {
            MainViewModel.Instance.ApplyMyCondition(Condition.Original);
        }

        private void CancelRename()
        {
            InputName.Value = Condition.DispName.Value;
            IsRenaming.Value = false;
        }

        private void ApplyRename()
        {
            IsRenaming.Value = false;
            Condition.Original.DispName = InputName.Value;
            MainViewModel.Instance.Simulator.UpdateMyCondition(Condition.Original);
            MainViewModel.Instance.LoadMyCondition();

        }
    }
}
