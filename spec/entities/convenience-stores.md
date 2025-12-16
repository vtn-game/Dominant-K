# コンビニ定義

## 共通パラメータ

| パラメータ名 | 型 | 説明 |
|-------------|-----|------|
| `id` | string | 一意識別子 |
| `displayName` | string | 表示名 |
| `buildCost` | int | 建設コスト |
| `zocRadius` | float | ZOC（影響範囲）半径 |
| `dominantRadius` | float | ドミナントエリア半径 |
| `baseRevenue` | int | 基本収益/秒 |
| `baseFaithGain` | float | 基本信仰度上昇率 |
| `customerSpendMultiplier` | float | 客単価倍率 |

---

## セブンイレバン (SevenEleban)

低コストだが、拡大するほど腐敗する。

### 基本ステータス

| パラメータ | 値 | 備考 |
|-----------|-----|------|
| `buildCost` | 100 | 最安 |
| `zocRadius` | 3.0 | 標準 |
| `dominantRadius` | 4.0 | 標準 |
| `baseRevenue` | 10 | 標準 |
| `baseFaithGain` | 1.0 | 標準 |
| `customerSpendMultiplier` | 1.0 | 標準 |

### 特殊効果: 経営腐敗 (CorruptionEffect)

ドミナント数に応じてステータスが低下する。

```
腐敗係数 = 1.0 - (dominantCount * 0.05)
最小値 = 0.3
```

| 影響パラメータ | 計算式 |
|---------------|--------|
| `customerSpendMultiplier` | base × 腐敗係数 |
| `faithGain` | base × 腐敗係数 |

| ドミナント数 | 腐敗係数 | 客単価倍率 | 信仰度上昇率 |
|-------------|---------|-----------|-------------|
| 1-2 | 1.0 | 1.0 | 1.0 |
| 5 | 0.75 | 0.75 | 0.75 |
| 10 | 0.50 | 0.50 | 0.50 |
| 14+ | 0.30 | 0.30 | 0.30 |

### 特殊能力: イレバン

7社以上でドミナント形成時に発動可能（効果は別途定義）

---

## ローサン (Lawson)

3種類のバリエーションを展開可能。個々の能力は控えめ。

### バリエーション共通

| パラメータ | 値 | 備考 |
|-----------|-----|------|
| `zocRadius` | 2.5 | やや狭い |
| `dominantRadius` | 3.5 | やや狭い |
| `baseFaithGain` | 0.8 | やや低い |

### バリエーション別ステータス

#### ローサン100 (Lawson100) - 低価格店舗

| パラメータ | 値 |
|-----------|-----|
| `buildCost` | 80 |
| `baseRevenue` | 6 |
| `customerSpendMultiplier` | 0.7 |

#### ローサン (LawsonStandard) - 標準店舗

| パラメータ | 値 |
|-----------|-----|
| `buildCost` | 120 |
| `baseRevenue` | 9 |
| `customerSpendMultiplier` | 1.0 |

#### ナチュラルローサン (NaturalLawson) - 高価格店舗

| パラメータ | 値 |
|-----------|-----|
| `buildCost` | 180 |
| `baseRevenue` | 14 |
| `customerSpendMultiplier` | 1.4 |

---

## フォモマ (Famoma)

怪音波による広域支配。高コスト高性能。

### 基本ステータス

| パラメータ | 値 | 備考 |
|-----------|-----|------|
| `buildCost` | 200 | 最高 |
| `zocRadius` | 5.0 | 最大 |
| `dominantRadius` | 6.0 | 最大 |
| `baseRevenue` | 12 | やや高い |
| `baseFaithGain` | 1.2 | やや高い |
| `customerSpendMultiplier` | 1.1 | やや高い |

### 特殊効果: 怪音波 (StrangeSoundWave)

ZOC内の住民に追加効果を与える。

| 効果 | 値 |
|------|-----|
| 洗脳速度ボーナス | +30% |
| 他社コンビニへの移動抑制 | 20% |

---

## チェーン比較サマリー

| チェーン | コスト | ZOC | ドミナント | 特徴 |
|---------|-------|-----|-----------|------|
| セブンイレバン | 低 | 中 | 中 | 腐敗デメリット |
| ローサン | 可変 | 狭 | 狭 | バリエーション |
| フォモマ | 高 | 広 | 広 | 怪音波バフ |
