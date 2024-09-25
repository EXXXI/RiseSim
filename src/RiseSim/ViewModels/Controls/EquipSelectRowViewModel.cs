using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RiseSim.Util;
using RiseSim.ViewModels.BindableWrapper;
using SimModel.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RiseSim.ViewModels.Controls
{
    /// <summary>
    /// 装備選択部品
    /// </summary>
    internal class EquipSelectRowViewModel : ChildViewModelBase
    {
        /// <summary>
        /// 表示用装備種類
        /// </summary>
        public ReactivePropertySlim<string> DispKind { get; } = new();

        /// <summary>
        /// 装備一覧
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<BindableEquipment>> Equips { get; } = new();

        /// <summary>
        /// 選択中の装備
        /// </summary>
        public ReactivePropertySlim<BindableEquipment> SelectedEquip { get; } = new();

        /// <summary>
        /// 入力中の装備名
        /// </summary>
        public ReactivePropertySlim<string> InputEquipName { get; } = new();

        /// <summary>
        /// 固定可能フラグ
        /// </summary>
        public ReactivePropertySlim<bool> CanInclude { get; } = new(true);

        /// <summary>
        /// 除外コマンド
        /// </summary>
        public ReactiveCommand ExcludeCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// 固定コマンド
        /// </summary>
        public ReactiveCommand IncludeCommand { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dispKind">種類</param>
        /// <param name="equips">装備リスト</param>
        public EquipSelectRowViewModel(string dispKind, List<Equipment> equips)
        {
            DispKind.Value = dispKind;
            if (EquipKind.deco.StrWithColon().Equals(dispKind))
            {
                // 装飾品は固定できない
                CanInclude.Value = false;
            }
            Equips.ChangeCollection(BindableEquipment.BeBindableList(equips));

            // コマンドを設定
            ExcludeCommand.Subscribe(_ => Exclude());
            IncludeCommand = CanInclude.ToReactiveCommand().WithSubscribe(() => Include()).AddTo(Disposable);
        }

        /// <summary>
        /// 装備を除外
        /// </summary>
        private void Exclude()
        {
            if (SelectedEquip.Value == null)
            {
                foreach (var equip in Equips.Value)
                {
                    if (equip.DispName.Value == InputEquipName.Value)
                    {
                        SelectedEquip.Value = equip;
                        break;
                    }
                }
            }

            if (SelectedEquip.Value != null)
            {
                CludeTabVM.AddExclude(SelectedEquip.Value.Original);
            }
        }

        /// <summary>
        /// 装備を固定
        /// </summary>
        private void Include()
        {
            if (SelectedEquip.Value == null)
            {
                foreach (var equip in Equips.Value)
                {
                    if (equip.DispName.Value == InputEquipName.Value)
                    {
                        SelectedEquip.Value = equip;
                        break;
                    }
                }
            }
            if (SelectedEquip.Value != null)
            {
                CludeTabVM.AddInclude(SelectedEquip.Value.Original);
            }
        }
    }
}
