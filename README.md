# RiseSim

## 概要

Windows上で動くモンハンライズスキルシミュレータです

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
- ファイルにデータを書き出す(マイセットとかの保存用)ので、ウィルス対策ソフトが文句言ってくる場合があります
- メモリの挙動が怪しい・・・(調査中)
  - たまにアプリ再起動したほうがいいかも
- 男女の区別なく検索します
  - そのうち対応予定ですが今は除外設定を使ってください
- 風雷合一のスキルレベル増加効果は計算していません
- 護石を削除するとその護石を使ってるマイセットも消えます
- 検索アルゴリズムの仕様上、1度に1000件や2000件検索するのはとても重くなります
  - 追加スキル検索や除外・固定機能をうまく使って絞り込む方がいいです
- 「WPFって何だっけ？」って状態から1週間で作ったので色々甘いのは許して
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
