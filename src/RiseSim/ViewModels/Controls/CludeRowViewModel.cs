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
using SimModel.model;
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

        internal void DeleteClude()
        {
            MainViewModel.Instance.DeleteClude(TrueName, DispName.Value);
        }
    }
}
