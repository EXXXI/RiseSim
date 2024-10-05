# RiseSim(気ままに作ったモンハンシミュレータ for MHRise)

## 概要

Windows上で動くモンハンライズスキルシミュレータです

## 更新・対応状況

本体の対応状況：サンブレイク Var16.0系統に対応

CSVの対応状況：サンブレイク Var16.0系統に対応

本体の最終更新：2024/10/05 内部処理を保守性の高いものに変更

CSVの最終更新：2024/10/04 装備の表形式表示用に仮番号を整備

今までの更新履歴は[ReleaseNote.md](./ReleaseNote.md)を参照

## 特徴

- 各種データをCSV形式で保持しています
  - イベクエ追加時など、装備等の追加を、シミュの更新を待たずに自身で行えます
- 「最近検索に利用したスキル」からスキルを選択できます
- 「よく使う検索条件」を保存して、必要な時に呼び出すことができます
- 「T6をここまで錬成出来たらどんなスキル組める？」「T6でこんな錬成引いたけど移植前提でどんなスキル組める？」といった検索(理想錬成検索機能)ができます
  - ただ、ちょっと設定が面倒

## 使い方

### 起動方法

- RiseSim.zipをダウンロード
- RiseSim.zipを解凍
- 中にあるRiseSim.exeをダブルクリック

### 機能説明

偉大な先人が作られたシミュに似せているので大体見ればわかるはず

詳しく知りたい人は[Reference.md](./Reference.md)を参照してください

## 注意点

- 64bitマシンのWindowsでしか動きません
- .Netのインストールが必要です(無ければ起動時に案内されるはず)
  - x64のデスクトップランタイムで動くはず
  - 持ってない場合は、Visual C++ 再頒布可能パッケージも必要
- ファイルにデータを書き出す(マイセットとかの保存用)ので、ウィルス対策ソフトが文句言ってくる場合があります
- シリーズ耐性値ボーナスや、スキルによる防御や耐性上昇は計算していません
- 装飾品の組み合わせは1通りしか検索しないため、他の組み合わせが可能な場合もあります
  - 例：火炎珠Ⅲ【３】1つと火炎珠【１】2つ→火炎珠Ⅲ【３】1つと火炎珠Ⅱ【２】1つ
  - 空きスロ欄に表示される空きスロは、表示されている装飾品を装備した場合の空きスロです
- 護石を削除するとその護石を使ってるマイセットも消えます
- 錬成防具や理想錬成も削除するとその錬成防具を使ってるマイセットも消えます
  - 編集した場合は消えませんが、マイセットの内容も変化します
    - 編集により、スロットが足りなくなった場合は、マイセットの詳細欄に警告が出ます
- 検索アルゴリズムの仕様上、1度に1000件や2000件検索するのはとても重くなります
  - 大量に検索するより、追加スキル検索や除外・固定機能をうまく使って絞り込む方がいいです
  - 防御・広域化などの、装飾品が複数あるやつは100件でもクッソ重くなります
- 「WPFって何だっけ？」って状態から1週間で大枠作ったので色々甘いのは許して
  - WPFの習得もコレを作った目的の一つなので、マズイところ等ご指摘いただければ狂喜乱舞します

## ライセンス

The MIT License

(ただし、20221128.0以前のバージョンを利用する場合はGNU General Public License v3.0です)

### ↑このライセンスって何？

こういう使い方までならOKだよ、ってのを定める取り決め

今回のは大体こんな感じ

- 基本は好きに使ってOK
- このシミュのせいで何か起きても開発者は責任取らんよ
- 改変や再配布するときはよく調べてルールに従ってね

## お問い合わせ

不具合発見時や欲しい機能がある際は、このリポジトリのIssueか、以下の質問箱へどうぞ

[質問箱](https://peing.net/ja/58b7c250e12e37)

## 使わせていただいたOSS(+必要であればライセンス)

### Google OR-Tools

プロジェクト：<https://github.com/google/or-tools>

ライセンス：<https://github.com/google/or-tools/blob/stable/LICENSE>

### CSV

プロジェクト：<https://github.com/stevehansen/csv/>

ライセンス：<https://raw.githubusercontent.com/stevehansen/csv/master/LICENSE>

### Prism.Wpf

プロジェクト：<https://github.com/PrismLibrary/Prism>

ライセンス：<https://www.nuget.org/packages/Prism.Wpf/8.1.97/license>

### ReactiveProperty

プロジェクト：<https://github.com/runceel/ReactiveProperty>

ライセンス：<https://github.com/runceel/ReactiveProperty/blob/main/LICENSE.txt>

### NLog

プロジェクト：<https://nlog-project.org/>

### DotNetKit.Wpf.AutoCompleteComboBox

プロジェクト：<https://github.com/vain0x/DotNetKit.Wpf.AutoCompleteComboBox/>

ライセンス：<https://www.nuget.org/packages/DotNetKit.Wpf.AutoCompleteComboBox/1.6.0/license>

## スペシャルサンクス

### 5chモンハン板シミュスレの方々

特にVer.13の>>480様の以下論文を大いに参考にしました

<https://github.com/13-480/lp-doc>

### 先人のシミュ作成者様

特に頑シミュ様のUIに大きく影響を受けています
