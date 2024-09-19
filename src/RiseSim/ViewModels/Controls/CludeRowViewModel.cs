using Reactive.Bindings;
using SimModel.Model;
using System;

namespace RiseSim.ViewModels.Controls
{
    /// <summary>
    /// 除外固定装備表示部品
    /// </summary>
    internal class CludeRowViewModel : ChildViewModelBase
    {
        /// <summary>
        /// 表示用装備種類
        /// </summary>
        public ReactivePropertySlim<string> DispKind { get; } = new();

        /// <summary>
        /// 表示用装備名
        /// </summary>
        public ReactivePropertySlim<string> DispName { get; } = new();

        /// <summary>
        /// 除外・固定状況
        /// </summary>
        public ReactivePropertySlim<string> Status { get; } = new();

        /// <summary>
        /// 管理用装備種類
        /// </summary>
        public EquipKind TrueKind { get; set; }

        /// <summary>
        /// 管理用装備名
        /// </summary>
        public string TrueName { get; set; }

        /// <summary>
        /// 除外・固定解除コマンド
        /// </summary>
        public ReactiveCommand DeleteCludeCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="clude">除外固定情報</param>
        /// <exception cref="ArgumentException"></exception>
        public CludeRowViewModel(Clude clude)
        {
            Equipment? equip = Masters.GetEquipByName(clude.Name, false);
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
            DeleteCludeCommand.Subscribe(_ => DeleteClude());
        }

        /// <summary>
        /// 除外・固定の解除
        /// </summary>
        private void DeleteClude()
        {
            CludeTabVM.DeleteClude(TrueName, DispName.Value);
        }
    }
}
