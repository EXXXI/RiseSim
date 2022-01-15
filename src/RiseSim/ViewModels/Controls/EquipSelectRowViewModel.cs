/*    RiseSim : MHRise skill simurator for Windows
 *    Copyright (C) 2022  EXXXI
 *
 *    This program is free software: you can redistribute it and/or modify
 *    it under the terms of the GNU General Public License as published by
 *    the Free Software Foundation, either version 3 of the License, or
 *    (at your option) any later version.
 *
 *    This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 *    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *    GNU General Public License for more details.
 *
 *    You should have received a copy of the GNU General Public License
 *    along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
using Prism.Mvvm;
using Reactive.Bindings;
using RiseSim.ViewModels.BindableWrapper;
using SimModel.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiseSim.ViewModels.Controls
{
    internal class EquipSelectRowViewModel : BindableBase
    {
        // 表示用装備種類
        public ReactivePropertySlim<string> DispKind { get; } = new();

        // 装備一覧
        public ReactivePropertySlim<ObservableCollection<BindableEquipment>> Equips { get; } = new();

        // 選択中の装備
        public ReactivePropertySlim<BindableEquipment> SelectedEquip { get; } = new();

        // 固定可能フラグ
        public ReactivePropertySlim<bool> CanInclude { get; } = new(true);

        // 除外コマンド
        public ReactiveCommand ExcludeCommand { get; } = new ReactiveCommand();

        // 固定コマンド
        public ReactiveCommand IncludeCommand { get; private set; }

        // コマンドを設定
        private void SetCommand()
        {
            ExcludeCommand.Subscribe(_ => Exclude());
            IncludeCommand = CanInclude.ToReactiveCommand().WithSubscribe(() => Include());
        }

        // コンストラクタ
        public EquipSelectRowViewModel(string dispKind, List<Equipment> equips)
        {
            DispKind.Value = dispKind;
            if (EquipKind.deco.StrWithColon().Equals(dispKind))
            {
                // 装飾品は固定できない
                CanInclude.Value = false;
            }
            Equips.Value = BindableEquipment.BeBindableList(equips);

            SetCommand();
        }

        // 装備を除外
        internal void Exclude()
        {
            if (SelectedEquip.Value != null)
            {
                MainViewModel.Instance.AddExclude(SelectedEquip.Value.Name, SelectedEquip.Value.DispName);
            }
        }

        // 装備を固定
        internal void Include()
        {
            if (SelectedEquip.Value != null)
            {
                MainViewModel.Instance.AddInclude(SelectedEquip.Value.Name, SelectedEquip.Value.DispName);
            }
        }
    }
}
