using Prism.Mvvm;
using Reactive.Bindings;
using SimModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiseSim.ViewModels.Controls
{
    internal class CharmRowViewModel : BindableBase
    {
        // 表示用護石名
        public ReactivePropertySlim<string> DispName { get; } = new();

        // 管理用護石名(GUID)
        public string TrueName { get; set; }

        // 護石削除コマンド
        public ReactiveCommand DeleteCharmCommand { get; } = new ReactiveCommand();

        // コマンドを設定
        private void SetCommand()
        {
            DeleteCharmCommand.Subscribe(_ => DeleteCharm());
        }

        // コンストラクタ
        public CharmRowViewModel(Equipment charm)
        {
            TrueName = charm.Name;
            DispName.Value = charm.DispName;

            // コマンドを設定
            SetCommand();
        }

        // 護石削除
        internal void DeleteCharm()
        {
            MainViewModel.Instance.DeleteCharm(TrueName, DispName.Value);
        }
    }
}
