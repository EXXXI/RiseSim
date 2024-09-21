using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RiseSim.ViewModels.BindableWrapper;
using SimModel.Model;
using System;

namespace RiseSim.ViewModels.Controls
{
    /// <summary>
    /// マイ検索条件表示部品
    /// </summary>
    internal class MyConditionRowViewModel : ChildViewModelBase
    {
        /// <summary>
        /// マイ検索条件
        /// </summary>
        public BindableSearchCondition Condition { get; }

        /// <summary>
        /// 名前変更中
        /// </summary>
        public ReactivePropertySlim<bool> IsRenaming { get; } = new(false);

        /// <summary>
        /// 名前変更中でない
        /// </summary>
        public ReactivePropertySlim<bool> IsNotRenaming { get; } = new(true);

        /// <summary>
        /// 名前変更時の入力した名前
        /// </summary>
        public ReactivePropertySlim<string> InputName { get; } = new();

        /// <summary>
        /// 名称変更開始コマンド
        /// </summary>
        public ReactiveCommand BeginRenameCommand { get; private set; }

        /// <summary>
        /// 名称変更完了コマンド
        /// </summary>
        public ReactiveCommand ApplyRenameCommand { get; private set; }

        /// <summary>
        /// 名称変更キャンセルコマンド
        /// </summary>
        public ReactiveCommand CancelRenameCommand { get; private set; }

        /// <summary>
        /// 検索条件適用コマンド
        /// </summary>
        public ReactiveCommand ApplyConditionCommand { get; } = new();

        /// <summary>
        /// 検索条件削除コマンド
        /// </summary>
        public ReactiveCommand DeleteConditionCommand { get; } = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="condition">マイ検索条件</param>
        public MyConditionRowViewModel(SearchCondition condition)
        {
            Condition = new BindableSearchCondition(condition);
            InputName.Value = Condition.DispName.Value;

            IsRenaming.Subscribe(_ => IsNotRenaming.Value = !IsRenaming.Value);
            BeginRenameCommand = IsNotRenaming.ToReactiveCommand().WithSubscribe(() => IsRenaming.Value = true).AddTo(Disposable);
            ApplyRenameCommand = IsRenaming.ToReactiveCommand().WithSubscribe(() => ApplyRename()).AddTo(Disposable);
            CancelRenameCommand = IsRenaming.ToReactiveCommand().WithSubscribe(() => CancelRename()).AddTo(Disposable);
            ApplyConditionCommand.Subscribe(() => ApplyMyCondition());
            DeleteConditionCommand.Subscribe(() => DeleteCondition());
        }

        /// <summary>
        /// マイ検索条件適用
        /// </summary>
        private void ApplyMyCondition()
        {
            SkillSelectTabVM.ApplyMyCondition(Condition.Original);
        }

        /// <summary>
        /// マイ検索条件削除
        /// </summary>
        private void DeleteCondition()
        {
            Simulator.DeleteMyCondition(Condition.Original);
            SkillSelectTabVM.LoadMyCondition();
        }

        /// <summary>
        /// 名前変更キャンセル
        /// </summary>
        private void CancelRename()
        {
            InputName.Value = Condition.DispName.Value;
            IsRenaming.Value = false;
        }

        /// <summary>
        /// 名前変更確定
        /// </summary>
        private void ApplyRename()
        {
            IsRenaming.Value = false;
            Condition.Original.DispName = InputName.Value;
            Simulator.UpdateMyCondition(Condition.Original);
            SkillSelectTabVM.LoadMyCondition();
        }
    }
}
