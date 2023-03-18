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
            sb.Append("20230319.0\n");
            sb.Append('\n');
            sb.Append("■このシミュのライセンス\n");
            sb.Append("MIT License\n");
            sb.Append('\n');
            sb.Append("■使わせていただいたOSS(+必要であればライセンス)\n");
            sb.Append("・Google OR-Tools\n");
            sb.Append("プロジェクト：https://github.com/google/or-tools\n");
            sb.Append("ライセンス：https://github.com/google/or-tools/blob/stable/LICENSE\n");
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
            sb.Append("・NLog\n");
            sb.Append("プロジェクト：https://nlog-project.org/\n");
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
            sbw.Append("■もっと詳しく\n");
            sbw.Append("こういう使い方までならOKだよ、ってのを定める取り決め\n");
            sbw.Append("今回のは大体こんな感じ\n");
            sbw.Append("・基本は好きに使ってOK\n");
            sbw.Append("・このシミュのせいで何か起きても開発者は責任取らんよ\n");
            sbw.Append("・改変や再配布するときだけはよく調べてルールに従ってね\n");
            WhatIsLicense.Value = sbw.ToString();
        }
    }
}
