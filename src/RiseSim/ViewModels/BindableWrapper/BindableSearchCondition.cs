using Reactive.Bindings;
using SimModel.Model;
using System.Text;

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
            Description.Value = MakeDescription(condition);
            DispName.Value = condition.DispName;
        }

        /// <summary>
        /// 表示用説明
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns>表示用説明文字列</returns>
        private string MakeDescription(SearchCondition condition)
        {
            string none = "なし";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"武器スロ:{condition.WeaponSlot1}-{condition.WeaponSlot2}-{condition.WeaponSlot3}");
            sb.AppendLine($"防御力:{condition.Def?.ToString() ?? none}, 性別:{condition.Sex.ToString()}");
            sb.Append($"火:{condition.Fire?.ToString() ?? none},");
            sb.Append($"水:{condition.Water?.ToString() ?? none},");
            sb.Append($"雷:{condition.Thunder?.ToString() ?? none},");
            sb.Append($"氷:{condition.Ice?.ToString() ?? none},");
            sb.Append($"龍:{condition.Dragon?.ToString() ?? none}");
            foreach (var skill in condition.Skills)
            {
                sb.AppendLine();
                sb.Append($"{skill.Name}Lv{skill.Level}{(skill.IsFixed ? "(固定)" : string.Empty)}");
            }

            return sb.ToString();
        }
    }
}
