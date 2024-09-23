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
        public ReactivePropertySlim<ObservableCollection<SkillLevelSelectorItem>> Levels { get; } = new();

        /// <summary>
        /// 選択されているレベル
        /// </summary>
        public ReactivePropertySlim<SkillLevelSelectorItem> SelectedLevel { get; } = new();

        /// <summary>
        /// スキル値固定有無の表示候補
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<string>> IsFixDisps { get; } = new();

        /// <summary>
        /// スキル値固定有無の表示内容
        /// </summary>
        public ReactivePropertySlim<string> IsFixDisp { get; } = new();

        /// <summary>
        /// 設定有無
        /// </summary>
        public ReactivePropertySlim<bool> Selected { get; } = new(false);

        /// <summary>
        /// スキル名
        /// </summary>
        public string SkillName { get; set; }

        /// <summary>
        /// 選択されているスキルを返却
        /// </summary>
        internal Skill SelectedSkill
        {
            get
            {
                return new Skill(SkillName, SelectedLevel.Value.Level, isFixed: IsFixDisp.Value == FixStr);
            }
        }

        /// <summary>
        /// 固定有無
        /// </summary>
        private bool IsFixed { get => IsFixDisp.Value == FixStr; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="skill">スキル情報</param>
        public SkillLevelSelectorViewModel(string skillName)
        {
            // スキル名保存
            SkillName = skillName;

            // スキル値固定関連準備
            IsFixDisps.Value = new();
            IsFixDisps.Value.Add(NotFixStr);
            IsFixDisps.Value.Add(FixStr);
            IsFixDisp.Value = NotFixStr;

            // レベル分のBindableSkillを作成
            Levels.Value = SkillLevelSelectorItem.MakeSkillLevelSelectorItems(skillName); 

            // 初期値
            SelectedLevel.Value = Levels.Value.First();

            // 設定有無フラグのロジック
            SelectedLevel.Subscribe(selected =>
            {
                Selected.Value = SelectedLevel.Value.Level != 0 || IsFixed;
            });
            IsFixDisp.Subscribe(_ => 
            { 
                Selected.Value = SelectedLevel.Value.Level != 0 || IsFixed;
            });
        }

        /// <summary>
        /// 指定のレベルを選択する
        /// </summary>
        /// <param name="v">レベル</param>
        internal void SelectLevel(int v)
        {
            SelectedLevel.Value = Levels.Value.Where(skill => skill.Level == v).First();
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
