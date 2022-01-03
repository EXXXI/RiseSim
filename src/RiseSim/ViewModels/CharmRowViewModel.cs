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
        private string dispName;
        public string DispName
        {
            get { return this.dispName; }
            set
            {
                this.SetProperty(ref this.dispName, value);
            }
        }

        // 管理用護石名(GUID)
        private string trueName;
        public string TrueName
        {
            get { return this.trueName; }
            set
            {
                this.SetProperty(ref this.trueName, value);
            }
        }

        public CharmRowViewModel(Equipment charm)
        {
            TrueName = charm.Name;
            DispName = charm.DispName;
        }

        internal void DeleteCharm()
        {
            MainViewModel.Instance.DeleteCharm(TrueName);
        }
    }
}
