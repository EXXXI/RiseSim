using Reactive.Bindings;
using SimModel.Model;
using System;

namespace RiseSim.ViewModels.Controls
{
    /// <summary>
    /// 護石表示部品
    /// </summary>
    internal class CharmRowViewModel : ChildViewModelBase
    {
        /// <summary>
        /// 表示用護石名
        /// </summary>
        public ReactivePropertySlim<string> DispName { get; } = new();

        /// <summary>
        /// 管理用護石名(GUID)
        /// </summary>
        public string TrueName { get; set; }

        /// <summary>
        /// 護石削除コマンド
        /// </summary>
        public ReactiveCommand DeleteCharmCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="charm">護石情報</param>
        public CharmRowViewModel(Equipment charm)
        {
            TrueName = charm.Name;
            DispName.Value = charm.DispName;

            // コマンドを設定
            DeleteCharmCommand.Subscribe(_ => DeleteCharm());
        }

        /// <summary>
        /// 護石削除
        /// </summary>
        internal void DeleteCharm()
        {
            CharmTabVM.DeleteCharm(TrueName, DispName.Value);
        }
    }
}
