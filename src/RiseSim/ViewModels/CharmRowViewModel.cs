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

namespace RiseSim.ViewModels
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

        public CharmRowViewModel(Equipment charm)
        {
            TrueName = charm.Name;
            DispName.Value = charm.DispName;
            SetCommand();
        }

        internal void DeleteCharm()
        {
            MainViewModel.Instance.DeleteCharm(TrueName, DispName.Value);
        }
    }
}
