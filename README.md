# RiseSim

## 概要

Windows上で動くモンハンライズスキルシミュレータです

## お知らせ(20220810.alpha)

傀異錬成防具の登録機能を仮実装しました。

ただし、細かい仕様が判明次第、手直しを入れます。

場合によっては登録した防具のデータを引き継げない変更もあり得るため、ご承知おきください。

## 更新・対応状況

本体の対応状況：サンブレイク Var11.0系統に対応中(傀異錬成防具仮対応)

CSVの対応状況：サンブレイク Var11.0系統に対応中(追加防具4シリーズとそのスキルに対応。装飾品はまだ)

本体の最終更新：2022/08/09 傀異錬成防具に仮対応

CSVの最終更新：2022/08/10 追加防具4シリーズとそのスキルに対応。装飾品はまだ

今までの更新履歴は[ReleaseNote.md](./ReleaseNote.md)を参照

## 特徴

- 各種データをCSV形式で保持しています
  - イベクエ追加時など、装備等の追加を、シミュの更新を待たずに自身で行えます
- スキル選択を右クリックで、「最近検索に利用したスキル」から選択できます

## 使い方

### 起動方法

- RiseSim.zipをダウンロード
- RiseSim.zipを解凍
- 中にあるRiseSim.exeをダブルクリック

### 機能説明

偉大な先人が作られたシミュに似せているので大体見ればわかるはず

詳しく知りたい人は[Reference.md](./Reference.md)を参照してください

## 注意点

- Windowsでしか動きません(たぶんWin7以上ならOK)
- .Netのインストールが必要です(無ければ起動時に案内されるはず)
  - デスクトップランタイムで動くはず
  - 64bitマシンではx64を利用してください
  - 32bitマシンではx86を利用すれば動くはずですが、メモリリークが残っているため動作が不安定です(調査中)
- ファイルにデータを書き出す(マイセットとかの保存用)ので、ウィルス対策ソフトが文句言ってくる場合があります
- シリーズ耐性値ボーナスや、スキルによる防御や耐性上昇は計算していません
- 護石を削除するとその護石を使ってるマイセットも消えます
- 検索アルゴリズムの仕様上、1度に1000件や2000件検索するのはとても重くなります
  - 大量に検索するより、追加スキル検索や除外・固定機能をうまく使って絞り込む方がいいです
  - 防御・広域化などの、装飾品が複数あるやつは100件でもクッソ重くなります
- 「WPFって何だっけ？」って状態から1週間で大枠作ったので色々甘いのは許して
  - WPFの習得もコレを作った目的の一つなので、マズイところ等ご指摘いただければ狂喜乱舞します

## ライセンス

GNU General Public License v3.0

### ↑このライセンスって何？

こういう使い方までならOKだよ、ってのを定める取り決め

今回のは大体こんな感じ

- シミュとして使う分には好きに使ってOK
- このシミュのせいで何か起きても開発者は責任取らんよ
- 改変や再配布するときはよく調べてルールに従ってね

## 使わせていただいたOSS(+必要であればライセンス)

### Google OR-Tools

プロジェクト：<https://github.com/google/or-tools>

ライセンス：<https://github.com/google/or-tools/blob/stable/LICENSE>

### GLPK for C#/CLI

プロジェクト：<http://glpk-cli.sourceforge.net/>

### GlpkWrapperCS(一部改変させていただきました)

プロジェクト：<https://github.com/YSRKEN/GlpkWrapperCS>

### CSV

プロジェクト：<https://github.com/stevehansen/csv/>

ライセンス：<https://raw.githubusercontent.com/stevehansen/csv/master/LICENSE>

### Prism.Wpf

プロジェクト：<https://github.com/PrismLibrary/Prism>

ライセンス：<https://www.nuget.org/packages/Prism.Wpf/8.1.97/license>

### ReactiveProperty

プロジェクト：<https://github.com/runceel/ReactiveProperty>

ライセンス：<https://github.com/runceel/ReactiveProperty/blob/main/LICENSE.txt>

## スペシャルサンクス

### 5chモンハン板シミュスレの方々

特にVer.13の>>480様の以下論文を大いに参考にしました

<https://github.com/13-480/lp-doc>

### 先人のシミュ作成者様

特に頑シミュ様のUIに大きく影響を受けています
