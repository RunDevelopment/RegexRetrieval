# Performance

## Measurements

| Query           | Avg          | Std     | Samples | Words   | Min/Max | Combinations | Optimization         |
| :-------------- | -----------: | ------: | ------: | ------: | ------: | -----------: | -------------------: |
| `love`          |     0,001 ms | 250,13% |     500 |       1 |   4/  4 |            1 |            WordIndex |
| `the`           |     0,000 ms | 146,10% |     500 |       1 |   3/  3 |            1 |            WordIndex |
| `considerate`   |     0,000 ms | 140,26% |     500 |       1 |  11/ 11 |            1 |            WordIndex |
| `qq`            |     0,000 ms | 138,32% |     500 |       1 |   2/  2 |            1 |            WordIndex |
|                 |              |         |         |         |         |              |                      |
| `te?t`          |     3,760 ms |   2,14% |      64 |      29 |   4/  4 |          inf |                      |
| `f?r`           |     3,215 ms |   2,89% |      75 |      64 |   3/  3 |          inf |                      |
| `?o`            |     0,561 ms |  18,86% |     233 |      96 |   2/  2 |          inf |                      |
| `g?`            |     0,513 ms |  24,47% |     486 |      88 |   2/  2 |          inf |                      |
| `?et`           |     1,348 ms |   4,69% |     187 |      48 |   3/  3 |          inf |                      |
| `te?`           |     1,173 ms |   1,81% |     213 |      52 |   3/  3 |          inf |                      |
| `c?n?i?e?a?e`   |    12,197 ms |   1,61% |      21 |      12 |  11/ 11 |          inf |                      |
| `c?????????e`   |    27,744 ms |   0,60% |      10 |    8009 |  11/ 11 |          inf |                      |
|                 |              |         |         |         |         |              |                      |
| `??`            |     0,086 ms |   6,77% |     500 |    5718 |   2/  2 |          inf |     PlaceholdersOnly |
| `????????`      |     0,121 ms |  11,72% |     500 |   10000 |   8/  8 |          inf |     PlaceholdersOnly |
| `????*`         |     0,138 ms |   8,24% |     500 |   10000 |   4/inf |          inf |     PlaceholdersOnly |
| `*`             |     0,000 ms |  82,65% |     500 |   10000 |   0/inf |          inf |             MatchAny |
| `[a][b]*[c]`    |     0,000 ms |  60,41% |     500 |   10000 |   0/inf |          inf |             MatchAny |
|                 |              |         |         |         |         |              |                      |
| `te?t*`         |     8,562 ms |   1,58% |      29 |   10000 |   4/inf |          inf |                      |
| `cons*ate`      |     1,382 ms |   8,15% |     174 |      66 |   7/inf |          inf |                      |
| `*ably`         |     1,046 ms |   4,44% |     178 |    1883 |   4/inf |          inf |                      |
| `*ell*`         |     7,081 ms |   1,21% |      35 |   10000 |   3/inf |          inf |                      |
| `qq*`           |     0,014 ms |  18,35% |     500 |     375 |   2/inf |          inf |                      |
| `*qq`           |     0,011 ms |  16,72% |     500 |     296 |   2/inf |          inf |                      |
| `*qq*`          |     0,168 ms |   6,59% |     500 |    1589 |   2/inf |          inf |                      |
|                 |              |         |         |         |         |              |                      |
| `a*b*c`         |    15,961 ms |   0,69% |      16 |    1556 |   3/inf |          inf |                      |
| `*a*b*c`        |    27,127 ms |   1,22% |      10 |    5853 |   3/inf |          inf |                      |
| `a*b*c*`        |    66,552 ms |   0,50% |       4 |   10000 |   3/inf |          inf |                      |
| `*a*b*c*`       |    89,508 ms |   1,45% |       3 |   10000 |   3/inf |          inf |                      |
|                 |              |         |         |         |         |              |                      |
| `initiali[zs]e` |     0,001 ms |  53,80% |     500 |       2 |  10/ 10 |            2 |            WordIndex |
| `[bjp]et`       |     0,001 ms |  43,51% |     500 |       3 |   3/  3 |            3 |            WordIndex |
| `colo[u]r`      |     0,001 ms |  49,92% |     500 |       2 |   5/  6 |            2 |            WordIndex |
| `[a][b][c]`     |     0,001 ms |  29,49% |     500 |       7 |   0/  3 |            8 |            WordIndex |
|                 |              |         |         |         |         |              |                      |
| `f{orm}`        |     0,005 ms | 184,31% |     500 |      26 |   4/  4 |           27 |            WordIndex |
| `{abc}`         |     0,004 ms |  42,06% |     500 |      27 |   3/  3 |           27 |            WordIndex |
| `{abcdef}`      |   355,880 ms |   1,17% |       2 |     929 |   6/  6 |        46656 |                      |

All measurements were taken on a Intel i7 8700K @ 3.7GHz with a maximum trie depth of 2.
