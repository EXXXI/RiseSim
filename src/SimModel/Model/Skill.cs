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
using System;

namespace SimModel.Model
{
    // �X�L��
    public class Skill
    {

        // �X�L����
        public string Name { get; set; }

        // �X�L�����x��
        public int Level { get; set; } = 0;

        // �R���X�g���N�^
        public Skill(string name, int level)
        {
            Name = name;
            Level = level;
        }

        // �\���p������
        public string Description 
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Name))
                {
                    return string.Empty;
                }
                return Name + "Lv" + Level;
            }
        }
    }
}
