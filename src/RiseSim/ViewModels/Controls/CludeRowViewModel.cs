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
    internal class CludeRowViewModel : BindableBase
    {
        // 表示用装備種類
        public ReactivePropertySlim<string> DispKind { get; } = new();

        // 表示用装備名
        public ReactivePropertySlim<string> DispName { get; } = new();

        // 除外・固定状況
        public ReactivePropertySlim<string> Status { get; } = new();

        // 管理用装備種類
        public EquipKind TrueKind { get; set; }

        // 管理用装備名
        public string TrueName { get; set; }

        // 除外・固定解除コマンド
        public ReactiveCommand DeleteCludeCommand { get; } = new ReactiveCommand();

        // コマンドを設定
        private void SetCommand()
        {
            DeleteCludeCommand.Subscribe(_ => DeleteClude());
        }

        // コンストラクタ
        public CludeRowViewModel(Clude clude)
        {
            Equipment? equip = Masters.GetEquipByName(clude.Name);
            if (equip == null)
            {
                throw new ArgumentException(clude.Name + "is not found.");
            }
            DispName.Value = equip.DispName;
            TrueName = equip.Name;
            TrueKind = equip.Kind;
            DispKind.Value = TrueKind.StrWithColon();
            switch (clude.Kind)
            {
                case CludeKind.exclude:
                    Status.Value = "除外中";
                    break;
                case CludeKind.include:
                    Status.Value = "固定中";
                    break;
                default:
                    break;
            }

            // コマンド設定
            SetCommand();
        }

        // 除外・固定の解除
        internal void DeleteClude()
        {
            MainViewModel.Instance.DeleteClude(TrueName, DispName.Value);
        }
    }
}
