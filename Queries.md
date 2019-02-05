# Queries


## Syntax

```
ValidChar := [^|?*(){}\[\]\\], "\\" AnyCharacter

Word      := ValidChar*
QMark     := "?"
Star      := "*"
CharSet   := "[" ValidChar* "]"
Optional  := "(" ValidChar* ")"

Token     := Word, QMark, Star, CharSet, Optional
Query     := Token Token*
```

Note: Backslash `\` is used to escape characters.


## Token semantics

| Name     | Example   | Example Regex | Description                                     |
| :------- | :-------- | :------------ | :---------------------------------------------- |
| Word     | `foo+Bar` | `foo\+Bar`    | Matches the string character by character.      |
| QMark    | `?`       | `[\s\S]`      | Matches any one character.                      |
| Star     | `*`       | `[\s\S]*`     | Matches any character any number of times.      |
| CharSet  | `[abc-]`  | `[abc\-]`     | Matches any character in the set.               |
| Optional | `(a)`     | `(?:a)?`      | Matches the contained string one or zero times. |


## Equivalence

In this context, *equal* means that the two queries match the same words.

Define _EMPTY_ as the query that only matches the empty word.
Let _q_ be a query, _c_ be a single valid character, and _w_ be a string containing only valid characters.

1. _q_ + _EMPTY_ = _EMPTY_ + _q_ = _q_
2. `*` = `**`
3. `?*` = `*?`
4. `[c]` = `c`
5. `()` = _EMPTY_
6. `(w)*` = `*` = `*(w)`
