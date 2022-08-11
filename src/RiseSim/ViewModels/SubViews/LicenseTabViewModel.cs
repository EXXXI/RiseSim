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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiseSim.ViewModels.SubViews
{
    class LicenseTabViewModel : BindableBase
    {
        // ライセンス画面の内容
        public ReactivePropertySlim<string> License { get; } = new();

        // ライセンス画面の雑な要約
        public ReactivePropertySlim<string> WhatIsLicense { get; } = new();

        // コンストラクタ
        public LicenseTabViewModel()
        {
            // TODO:バージョン毎回変えるのめんどくさいから何かいい方法を考える
            // バージョン・ライセンス表示
            StringBuilder sb = new();
            sb.Append("■バージョン\n");
            sb.Append("20220811.2\n");
            sb.Append('\n');
            sb.Append("■このシミュのライセンス\n");
            sb.Append("GNU General Public License v3.0\n");
            sb.Append('\n');
            sb.Append("■使わせていただいたOSS(+必要であればライセンス)\n");
            sb.Append("・Google OR-Tools\n");
            sb.Append("プロジェクト：https://github.com/google/or-tools\n");
            sb.Append("ライセンス：https://github.com/google/or-tools/blob/stable/LICENSE\n");
            sb.Append('\n');
            sb.Append("・GLPK for C#/CLI\n");
            sb.Append("プロジェクト：http://glpk-cli.sourceforge.net/\n");
            sb.Append('\n');
            sb.Append("・GlpkWrapperCS(一部改変)\n");
            sb.Append("プロジェクト：https://github.com/YSRKEN/GlpkWrapperCS\n");
            sb.Append('\n');
            sb.Append("・CSV\n");
            sb.Append("プロジェクト：https://github.com/stevehansen/csv/\n");
            sb.Append("ライセンス：https://raw.githubusercontent.com/stevehansen/csv/master/LICENSE\n");
            sb.Append('\n');
            sb.Append("・Prism.Wpf\n");
            sb.Append("プロジェクト：https://github.com/PrismLibrary/Prism\n");
            sb.Append("ライセンス：https://www.nuget.org/packages/Prism.Wpf/8.1.97/license\n");
            sb.Append('\n');
            sb.Append("・ReactiveProperty\n");
            sb.Append("プロジェクト：https://github.com/runceel/ReactiveProperty\n");
            sb.Append("ライセンス：https://github.com/runceel/ReactiveProperty/blob/main/LICENSE.txt\n");
            sb.Append('\n');
            sb.Append("■スペシャルサンクス\n");
            sb.Append("・5chモンハン板シミュスレの方々\n");
            sb.Append("特にVer.13の >> 480様の以下論文を大いに参考にしました\n");
            sb.Append("https://github.com/13-480/lp-doc\n");
            sb.Append('\n');
            sb.Append("・先人のシミュ作成者様\n");
            sb.Append("特に頑シミュ様のUIに大きく影響を受けています\n");
            License.Value = sb.ToString();

            // ライセンスの雑な説明を表示
            StringBuilder sbw = new();
            sbw.Append("■←ライセンスって何？\n");
            sbw.Append("普通にスキルシミュとして使う分には気にしないでOK\n");
            sbw.Append("        \n");
            sbw.Append("■いや、怖いからちゃんと説明して？\n");
            sbw.Append("こういう使い方までならOKだよ、ってのを定める取り決め\n");
            sbw.Append("今回のは大体こんな感じ\n");
            sbw.Append("・シミュとして使う分には好きに使ってOK\n");
            sbw.Append("・このシミュのせいで何か起きても開発者は責任取らんよ\n");
            sbw.Append("・改変や再配布するときはよく調べてルールに従ってね\n");
            WhatIsLicense.Value = sbw.ToString();
        }
    }
}
