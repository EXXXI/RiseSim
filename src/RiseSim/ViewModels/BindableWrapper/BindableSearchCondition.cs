using Reactive.Bindings;
using SimModel.Model;

namespace RiseSim.ViewModels.BindableWrapper
{
    /// <summary>
    /// バインド用マイ検索条件
    /// </summary>
    internal class BindableSearchCondition : ChildViewModelBase
    {
        /// <summary>
        /// 表示用詳細
        /// </summary>
        public ReactivePropertySlim<string> Description { get; set; } = new();

        /// <summary>
        /// 表示用名称
        /// </summary>
        public ReactivePropertySlim<string> DispName { get; set; } = new();

        /// <summary>
        /// オリジナル
        /// </summary>
        public SearchCondition Original { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="condition"></param>
        public BindableSearchCondition(SearchCondition condition)
        {
            Original = condition;
            Description.Value = condition.Description;
            DispName.Value = condition.DispName;
        }
    }
}
