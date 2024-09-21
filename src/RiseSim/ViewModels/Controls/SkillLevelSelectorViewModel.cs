using Reactive.Bindings;
using RiseSim.ViewModels.BindableWrapper;
using SimModel.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace RiseSim.ViewModels.Controls
{
    /// <summary>
    /// スキルレベル選択部品
    /// </summary>
    internal class SkillLevelSelectorViewModel : ChildViewModelBase
    {
        /// <summary>
        /// 「固定」文字列
        /// </summary>
        const string FixStr = "固定";

        /// <summary>
        /// 「以上」文字列
        /// </summary>
        const string NotFixStr = "以上";

        /// <summary>
        /// ComboBox用のデータソース
        /// </summary>
        public ObservableCollection<BindableSkill> Items { get; init; }

        /// <summary>
        /// 選択されているスキル
        /// </summary>
        public ReactivePropertySlim<BindableSkill> SelectedSkill { get; set; }

        /// <summary>
        /// スキル値固定有無の表示候補
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<string>> IsFixDisps { get; } = new();

        /// <summary>
        /// スキル値固定有無の表示内容
        /// </summary>
        public ReactivePropertySlim<string> IsFixDisp { get; } = new();

        /// <summary>
        /// スキル値固定有無状態
        /// </summary>
        public bool IsFix { get; set; } = false;

        /// <summary>
        /// 設定有無
        /// </summary>
        public ReactivePropertySlim<bool> Selected { get; } = new(false);

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="skill">スキル情報</param>
        public SkillLevelSelectorViewModel(Skill skill)
        {
            // スキル値固定関連準備
            IsFixDisps.Value = new();
            IsFixDisps.Value.Add(NotFixStr);
            IsFixDisps.Value.Add(FixStr);
            IsFixDisp.Value = NotFixStr;

            // レベル分のBindableSkillを作成
            Items = new ObservableCollection<BindableSkill>(Enumerable.Range(0, skill.Level + 1)
                .Select(level => new BindableSkill(skill) { Level = level }));

            // 初期値
            SelectedSkill = new ReactivePropertySlim<BindableSkill> { Value = Items.First() };

            // 設定有無フラグのロジック
            SelectedSkill.Subscribe(selected =>
            {
                Selected.Value = SelectedSkill.Value.Level != 0 || IsFix;
            });
            IsFixDisp.Subscribe(_ => 
            { 
                IsFix = IsFixDisp.Value == FixStr;
                Selected.Value = SelectedSkill.Value.Level != 0 || IsFix;
            });
        }

        /// <summary>
        /// 指定のレベルを選択する
        /// </summary>
        /// <param name="v">レベル</param>
        internal void SelectLevel(int v)
        {
            SelectedSkill.Value = Items.Where(skill => skill.Level == v).First();
        }

        /// <summary>
        /// 選択状態をリセット
        /// </summary>
        internal void Reset()
        {
            SelectLevel(0);
            IsFixDisp.Value = NotFixStr;
        }

        /// <summary>
        /// 固定有無を指定したものに変更
        /// </summary>
        /// <param name="isFix">指定</param>
        internal void SetIsFix(bool isFix)
        {
            IsFixDisp.Value = isFix ? FixStr : NotFixStr;
        }
    }
}
