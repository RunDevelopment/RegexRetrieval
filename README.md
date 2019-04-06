# RegexRetrieval

This is a fun little C# project providing a CLI and library for fast searches on fixed word list using a regex-like query syntax. The resulting list of words is guaranteed to be in the original order.

The algorithms performing the search are called _Retrievers_. There are two retrievers currently implemented:

1. __Array retriever:__ <br>
    The most simple retriever possible. Given a query, it will generate a regular expression and match it against every word.
2. __Regex retriever:__ <br>
    This one is a bit smarter. It will try to narrow down the search range by analyzing the query and using _Matchers_ to make a pre-selection, filtering out words which can't match the query.


## Regex retriever

This section will describe the inner workings of the Regex Retriever, so get ready for some details!

The Regex retriever employs a number of _Matchers_ to narrow the search range, and a _word index_ to optimize queries which can only match a small number of words.

A _Matcher_ is an algorithm which given some criteria will return a list of words which match the criteria. The returned list is guaranteed to...

1. ...be a subset of the fixed word list on which to perform search operations.
2. ...be in the order of the fixed word list.
3. ...contain all elements matching the criteria.

Such a list is called a _selection_.

_Note: A selection might still contain some elements of the fixed word list which do not match the criteria. If all elements in the selection match the criteria, the selection is called **minimal**._

_Note: Selections are not usually collections of strings but collections of integers where each integer is the position of a word in the fixed word list. This integer representation has a number of advantages but the main reason is the memory-efficiency and the simplicity of implementing set operations on such collections._


### Word index

The word index is not a matcher as it does not return a selection. Instead, it is a simple hash map mapping each word of the fixed word list to its position. It is used to quickly check all possible words of queries which can only match a small finite number of words.

Given such a query, all possible words are generated and individually checked using the words index.

__Example:__ The query `[gs]et` can only match `get` and `set`.


### Length matcher

This simple matcher will return a minimal selection based on word length.

All length intervals of the form `[a, b]` with _0 &lt;= a &lt;= b_ are supported.

If a query only contains placeholders, the result of this matcher will be the final result.

__Example:__ The query `?e?` can only match words of length 3 while `??*` matches all words with length >= 2.


### Substring matcher (SSM)

Given strings which must be substrings of a word to match the query, it will return a selection.

If a given string contains a character which no word contains, the empty selection will be returned.

__Example:__ All words which match the query `*ll*` must have `ll` as a substring.


### Positional substring matcher (PSSM)

Given a list of positional substring (PSS) which must be PSSs of a word to match the query, it will return a selection.

The basic idea is that we know the position or a range of positions for each substring, i.e. the substring `abc` of the query `abc*` has to be at the start of every word matching the query. This drastically reduces the number of words which have to be tested compared to the SSM.

A PSS is a substring which also has a range in which the substring has to occur associated with it. A PSS is a tuple _(S, L, R)_ where _S_ is the substring, and _L_ and _R_ are intervals defining the number of characters which have to be left and right to _S_. <br>
_S_ is PSS of a word _w_ iff there exists a position _i_ in _w_ for which `w.SubString(i, S.Length) == S` and _i∈L_ and _(w.Length - S.Length - i)∈R_. <br>
__Example:__ _("abc", \[0,2\], \[1,1\])_ is PSS of _"abcd"_, _"xabcx"_, and _"xxabcx"_ but not _"abc"_ or _"abcxx"_.

Because there are __a lot__ of possible PSSs of one substring, the PSSM only looks at PSS for which _L_ or _R_ are fixed, meaning that there is only one number which is in the interval. To simplify even more, there are two variants of the PSSM: A left to right (LTR) variant which only handles PSSs with fixed _L_ and a right to left (RTL) variant which only handles PSSs with fixed _R_.

The LTR PSSM is implemented as a list of substring tries where the list index (zero-based) of a substring trie is this position of the substring. The value of all nodes of each substring trie is the selection of words which have the string encoded by the path of the nodes as a substring at the position given by list index.<br>
Given a PSS with fixed _L_, the PSSM will choose the substring trie for which the index is in _L_. This trie is then used to get the most specific selection given the PSS.

_Note: A RTL PSSM is implemented as an LTR PSSM which is constructed on the list of words for which the characters of every word are in reversed order. The substrings of the PSSs given the RTL PSSM also have to be reversed._

__Example:__ All words which match the query `awe*some` must start with `awe` and end with `some`. More complex: All words which match the query `?e??*e?` must have an `e` at index 1 and an `e` as the second last letter.


## CLI

This project also comes with a small command line interface to debug and test the library. Build and run the `RegexRetriever.CLI` project to open the CLI.

Every input is interpreted as a command or query. Enter an empty input to exit the program.

For debugging purposes, the program will automatically load (`$LOAD`) a word list (a text file where each line is one word) and open the input for the retriever (`$CREATE`).


### Commands

All commands start with a dollar sign `$` followed by the command name (case sensitive) and a list of arguments separated by spaces.

#### `$LOAD <path>`

This command loads the word list with the given path.

Because a new retriever has to be constructed after loading a new word list, the `$CREATE` command will be automatically executed after the word list is loaded.

__Example:__ `$LOAD C:\path\to\file.txt`

#### `$CREATE [ <retriever> ]`

This command is used to create a retriever.

If no argument is provided, another input will be opened prompting the user to input the retriever to create.

Retrievers may support additional creation options in the form of arguments. This is documented when executing this command with arguments.

__Example:__ `$CREATE array`

#### `$GC`

This command takes no arguments.

Upon execution, it will call .Net's garbage collection to compact the heap as much as possible. It will print the consumed memory when finished.

__Example:__ `$GC`

#### `$TEST [ <query 1> [ <query 2> [ <query 3> [ ... ] ] ] ]`

This command takes any number of queries and executes them, outputting the results as a markdown table along with execution time and other properties of the queries.

If no arguments are provided, the standard test cases will be executed. These queries are defined in `TestCases.cs`.

__Example:__ `$TEST abc* *??ly colo[u]r`
