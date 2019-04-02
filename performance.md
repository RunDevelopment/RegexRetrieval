# Performance

## Measurements

| Query           | Avg          | Std     | Samples | Words   | Min/Max | Combinations | Optimization         |
| :-------------- | -----------: | ------: | ------: | ------: | ------: | -----------: | -------------------: |
| `love`          |     0,001 ms | 144,31% |     500 |       1 |   4/  4 |            1 |            WordIndex |
| `the`           |     0,000 ms |  97,98% |     500 |       1 |   3/  3 |            1 |            WordIndex |
| `considerate`   |     0,000 ms |  72,41% |     500 |       1 |  11/ 11 |            1 |            WordIndex |
| `qq`            |     0,000 ms |  79,61% |     500 |       1 |   2/  2 |            1 |            WordIndex |
|                 |              |         |         |         |         |              |                      |
| `te?t`          |     5,831 ms |   9,17% |      44 |      29 |   4/  4 |          inf |                      |
| `f?r`           |     2,364 ms |   8,95% |      92 |      64 |   3/  3 |          inf |                      |
| `?o`            |     0,553 ms |   9,67% |     236 |      96 |   2/  2 |          inf |                      |
| `g?`            |     0,534 ms |   7,12% |     439 |      88 |   2/  2 |          inf |                      |
| `?et`           |     1,432 ms |   5,08% |     179 |      48 |   3/  3 |          inf |                      |
| `te?`           |     1,620 ms |   5,80% |     163 |      52 |   3/  3 |          inf |                      |
| `c?n?i?e?a?e`   |     8,560 ms |  10,83% |      27 |      12 |  11/ 11 |          inf |                      |
| `c?????????e`   |     8,217 ms |   9,80% |      27 |    8009 |  11/ 11 |          inf |                      |
|                 |              |         |         |         |         |              |                      |
| `??`            |     0,098 ms |  13,04% |     500 |    5718 |   2/  2 |          inf |     PlaceholdersOnly |
| `????????`      |     0,159 ms |  12,84% |     500 |   10000 |   8/  8 |          inf |     PlaceholdersOnly |
| `????*`         |     0,146 ms |  12,00% |     500 |   10000 |   4/inf |          inf |     PlaceholdersOnly |
| `[a]????*`      |     0,143 ms |   8,78% |     500 |   10000 |   4/inf |          inf |     PlaceholdersOnly |
| `*`             |     0,000 ms | 348,62% |     500 |   10000 |   0/inf |          inf |             MatchAny |
| `[a][b]*[c]`    |     0,000 ms | 463,83% |     500 |   10000 |   0/inf |          inf |             MatchAny |
|                 |              |         |         |         |         |              |                      |
| `te?t*`         |     9,011 ms |   6,95% |      26 |   10000 |   4/inf |          inf |                      |
|                 |              |         |         |         |         |              |                      |
| `cons*ate`      |     1,820 ms | 121,32% |     155 |      66 |   7/inf |          inf |                      |
| `*ably`         |     1,233 ms |   6,68% |     179 |    1883 |   4/inf |          inf |                      |
| `*ell*`         |     5,931 ms |   8,80% |      38 |   10000 |   3/inf |          inf |                      |
| `qq*`           |     0,013 ms |  21,88% |     500 |     375 |   2/inf |          inf |                      |
| `*qq`           |     0,010 ms |   6,12% |     500 |     296 |   2/inf |          inf |                      |
| `*qq*`          |     0,119 ms |  12,50% |     500 |    1589 |   2/inf |          inf |                      |
| `*appendchild*` |     1,917 ms |  12,11% |      83 |   10000 |  11/inf |          inf |                      |
|                 |              |         |         |         |         |              |                      |
| `a*b*c`         |    17,132 ms |   3,56% |      15 |    1556 |   3/inf |          inf |                      |
| `*a*b*c`        |    30,882 ms |   6,39% |       9 |    5853 |   3/inf |          inf |                      |
| `a*b*c*`        |    71,566 ms |   1,37% |       4 |   10000 |   3/inf |          inf |                      |
| `*a*b*c*`       |    93,146 ms |   1,25% |       3 |   10000 |   3/inf |          inf |                      |
|                 |              |         |         |         |         |              |                      |
| `initiali[zs]e` |     0,001 ms | 754,55% |     500 |       2 |  10/ 10 |            2 |            WordIndex |
| `[bjp]et`       |     0,001 ms |  52,99% |     500 |       3 |   3/  3 |            3 |            WordIndex |
|                 |              |         |         |         |         |              |                      |
| `colo[u]r`      |     0,001 ms |  76,89% |     500 |       2 |   5/  6 |            2 |            WordIndex |
| `[a][b][c]`     |     0,001 ms |  26,65% |     500 |       7 |   0/  3 |            8 |            WordIndex |
|                 |              |         |         |         |         |              |                      |
| `f{orm}`        |     0,004 ms |   9,74% |     500 |      26 |   4/  4 |           27 |            WordIndex |
| `{abc}`         |     0,004 ms | 136,20% |     500 |      27 |   3/  3 |           27 |            WordIndex |
| `{abcd}`        |     0,040 ms |  34,05% |     500 |     256 |   4/  4 |          256 |            WordIndex |
| `{abcde}`       |   215,680 ms |   1,22% |       2 |    1378 |   5/  5 |         3125 |                      |
| `{abcdef}`      |   397,175 ms |   4,86% |       2 |     929 |   6/  6 |        46656 |                      |
| `{abcdefg}`     |   324,331 ms |   0,56% |       2 |     569 |   7/  7 |       823543 |                      |
| `{abcdefgh}`    |   299,953 ms |   1,93% |       2 |     424 |   8/  8 |     16777216 |                      |
|                 |              |         |         |         |         |              |                      |
| `*C*`           |     0,001 ms | 146,20% |     500 |       0 |   1/inf |          inf |       EmptySelection |

All measurements were taken on a Intel i7 8700K @ 3.7GHz with a maximum trie depth of 2.
