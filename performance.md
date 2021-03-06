# Performance

## Measurements

| Query           | Avg          | Std     | Samples | Words   | Min/Max | Combinations | Optimization         |
| :-------------- | -----------: | ------: | ------: | ------: | ------: | -----------: | -------------------: |
| `love`          |     0.000 ms |  68.59% |     500 |       1 |   4/  4 |            1 |            WordIndex |
| `the`           |     0.000 ms |  90.46% |     500 |       1 |   3/  3 |            1 |            WordIndex |
| `considerate`   |     0.000 ms |  67.33% |     500 |       1 |  11/ 11 |            1 |            WordIndex |
| `qq`            |     0.000 ms |  68.27% |     500 |       1 |   2/  2 |            1 |            WordIndex |
|                 |              |         |         |         |         |              |                      |
| `te?t`          |     5.774 ms |   4.49% |      44 |      29 |   4/  4 |          inf |                      |
| `f?r`           |     2.285 ms |   4.47% |     103 |      64 |   3/  3 |          inf |                      |
| `?o`            |     0.535 ms |   7.49% |     231 |      96 |   2/  2 |          inf |                      |
| `g?`            |     0.526 ms |   3.84% |     474 |      88 |   2/  2 |          inf |                      |
| `?et`           |     1.433 ms |   5.08% |     177 |      48 |   3/  3 |          inf |                      |
| `te?`           |     1.565 ms |   3.51% |     162 |      52 |   3/  3 |          inf |                      |
| `c?n?i?e?a?e`   |     9.203 ms |   5.07% |      27 |      12 |  11/ 11 |          inf |                      |
| `c?????????e`   |     8.287 ms |   7.11% |      27 |    8009 |  11/ 11 |          inf |                      |
|                 |              |         |         |         |         |              |                      |
| `??`            |     0.098 ms |  10.40% |     500 |    5718 |   2/  2 |          inf |     PlaceholdersOnly |
| `????????`      |     0.157 ms |   7.56% |     500 |   10000 |   8/  8 |          inf |     PlaceholdersOnly |
| `????*`         |     0.143 ms |   4.91% |     500 |   10000 |   4/inf |          inf |     PlaceholdersOnly |
| `[a]????*`      |     0.148 ms |  53.16% |     500 |   10000 |   4/inf |          inf |     PlaceholdersOnly |
| `*`             |     0.000 ms |  85.07% |     500 |   10000 |   0/inf |          inf |             MatchAny |
| `[a][b]*[c]`    |     0.000 ms |  85.53% |     500 |   10000 |   0/inf |          inf |             MatchAny |
|                 |              |         |         |         |         |              |                      |
| `te?t*`         |     8.963 ms |   6.47% |      26 |   10000 |   4/inf |          inf |                      |
| `??*abc*`       |     2.054 ms |  39.97% |      62 |     885 |   5/inf |          inf |                      |
| `*abc??*`       |     2.427 ms |  19.95% |      90 |    1906 |   5/inf |          inf |                      |
| `??*abc??*`     |     2.017 ms |  26.86% |      84 |     443 |   7/inf |          inf |                      |
| `??*abc??`      |     0.259 ms |   5.10% |     500 |     114 |   7/inf |          inf |                      |
| `??abc??*`      |     0.174 ms |  10.68% |     500 |     183 |   7/inf |          inf |                      |
|                 |              |         |         |         |         |              |                      |
| `cons*ate`      |     1.610 ms |   5.79% |     155 |      66 |   7/inf |          inf |                      |
| `*ably`         |     1.203 ms |   3.33% |     176 |    1883 |   4/inf |          inf |                      |
| `*ell*`         |     6.031 ms |   9.97% |      37 |   10000 |   3/inf |          inf |                      |
| `qq*`           |     0.014 ms |   7.17% |     500 |     375 |   2/inf |          inf |                      |
| `*qq`           |     0.011 ms |  76.35% |     500 |     296 |   2/inf |          inf |                      |
| `*qq*`          |     0.118 ms |   5.28% |     500 |    1589 |   2/inf |          inf |                      |
| `*appendchild*` |     2.151 ms |  36.58% |      82 |   10000 |  11/inf |          inf |                      |
|                 |              |         |         |         |         |              |                      |
| `a*b*c`         |    14.420 ms |   3.95% |      17 |    1556 |   3/inf |          inf |                      |
| `*a*b*c`        |    23.773 ms |   1.34% |      11 |    5853 |   3/inf |          inf |                      |
| `a*b*c*`        |    47.652 ms |   1.50% |       6 |   10000 |   3/inf |          inf |                      |
| `*a*b*c*`       |    54.907 ms |   4.05% |       5 |   10000 |   3/inf |          inf |                      |
| `*a*b*c*d*e*f*` |    95.712 ms |   0.54% |       3 |     172 |   6/inf |          inf |                      |
|                 |              |         |         |         |         |              |                      |
| `initiali[zs]e` |     0.001 ms |  65.75% |     500 |       2 |  10/ 10 |            2 |            WordIndex |
| `[bjp]et`       |     0.001 ms |  34.43% |     500 |       3 |   3/  3 |            3 |            WordIndex |
|                 |              |         |         |         |         |              |                      |
| `colo[u]r`      |     0.001 ms |  50.66% |     500 |       2 |   5/  6 |            2 |            WordIndex |
| `[a][b][c]`     |     0.001 ms |  23.52% |     500 |       7 |   0/  3 |            8 |            WordIndex |
|                 |              |         |         |         |         |              |                      |
| `f{orm}`        |     0.004 ms | 150.06% |     500 |      26 |   4/  4 |           27 |            WordIndex |
| `{abc}`         |     0.004 ms |  10.40% |     500 |      27 |   3/  3 |           27 |            WordIndex |
| `{abcd}`        |     0.039 ms |  29.81% |     500 |     256 |   4/  4 |          256 |            WordIndex |
| `{abcde}`       |   211.626 ms |   1.02% |       2 |    1378 |   5/  5 |         3125 |                      |
| `{abcdef}`      |   375.626 ms |   0.51% |       2 |     929 |   6/  6 |        46656 |                      |
| `{abcdefg}`     |   333.461 ms |   6.10% |       2 |     569 |   7/  7 |       823543 |                      |
| `{abcdefgh}`    |   299.655 ms |   0.01% |       2 |     424 |   8/  8 |     16777216 |                      |
|                 |              |         |         |         |         |              |                      |
| `*C*`           |     0.001 ms | 645.61% |     500 |       0 |   1/inf |          inf |       EmptySelection |

All measurements were taken on an Intel i7 8700K @ 3.7GHz with a maximum trie depth of 2.
