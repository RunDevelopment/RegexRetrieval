# Queries

Queries are the basic unit to interact with the retrievers.

The following will describe the simple text representation of all query features plus their equivalent regex expression.

_Note: The Netspeak query syntax which is also supported by the CLI is not part of this document. Every Netspeak query can be translated in one of the queries described here._


## Syntax

The following context free grammar will describe the syntax of queries.

_Note: `,` is used for alternation, two adjacent terms are concatenated, terms followed by `*` are repeated n>=0 times, and JS regex-literal expressions are allowed._

```
ValidChar := /[^|?*(){}\[\]\\]/, "\\" AnyCharacter

Word      := ValidChar*
QMark     := "?"
Star      := "*"
CharSet   := "[" ValidChar* "]"
Optional  := "(" ValidChar* ")"

Token     := Word, QMark, Star, CharSet, Optional
Query     := Token Token*
```

_Note: Backslash `\` is used to escape characters._


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

_Note: The `+` operator refers to simple query concatenation._

1. _q_ + _EMPTY_ = _EMPTY_ + _q_ = _q_
2. `*` = `**`
3. `?*` = `*?`
4. `[c]` = `c`
5. `()` = _EMPTY_
6. `(w)*` = `*` = `*(w)`
