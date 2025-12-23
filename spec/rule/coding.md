# コーディングルール

## 基本要素
- ファイルおよびクラスはなるべく分割する事
- クラス間の関係性は疎結合である事
- テストを書く事で契約的な実装である事を保証する
- データ注入するさいはDIの概念に則る

## ライブラリの利用
- コルーチンはUniTaskを利用する事
- DOTweenをアニメーションに使用する
- Unity DOTS (Entities) を大量オブジェクト処理に使用する
- Unity.Mathematics を数学演算に使用する

## ECS (DOTS) 関連
- Baker<T>を使用する際は`Unity.Entities.Hybrid`アセンブリ参照が必要
- SystemはISystemを継承したpartial structで実装
- BurstCompile属性を可能な限り適用する

## 名前空間
- DominantK : ルート名前空間
- DominantK.Core : ゲーム制御
- DominantK.Systems : ゲームシステム
- DominantK.AI : AI関連
- DominantK.Data : データ定義
- DominantK.Entities : ゲームエンティティ
- DominantK.ECS : Unity DOTS関連
