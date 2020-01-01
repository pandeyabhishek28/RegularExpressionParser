# RegularExpressionParser (dotnet Core and Angular2+)
Regular Expression Parser

This is a simple implementation. More features can be added to this demo code if you want.
If you add more features to this demo code presented here, I would very much like to have a copy of that code if it is OK with you. 
I don't think it would be too difficult to port this demo code to C++ if you want.

I wished to grasp how a regular expression program works. thus I did some Googling and located some cool articles that describe the method of how regular expressions find a match. I listed the articles within the reference section of this text. I have implemented this program supported my analysis. I'll not go too much into describing the method and also the theory behind the regular expression, since the articles within the reference section cowl this okay (the topic of regular expressions is large and can need a book to elucidate thoroughly).

In this article, I'll merely show an implementation of a straightforward Regular Expression program (or mini Regular Expression Parser). I'll continue using the terms Automata, NFA, DFA, Minimum DFA, state, transitions, and alphabetic character transition. If you are doing not perceive these terms, I extremely suggest you scan au courant a number of the articles within the reference section before continuing.

So you ask, "why this one?" This implementation is completed stepwise, thus it makes it simple for somebody desirous to find out how regular expressions work. alternative features:

Has a user interface that helps you perceive the states and transitions

Use of ^ and $ tokens to specify match at the start and end of the pattern severally

A C# (Dotnet Core, angular2+, typescript) implementation, quite object-oriented

Has a feature permitting you to regulate the greediness of the program - permitting you to expertise the various behavior of greediness.

Not restricted to code characters (0-255)

This implementation is additional complete than most parsers I stumbled on.

Points of Interest:-

The NFA models for quantifiers *, +, and ? can be found in the articles I mentioned. When I was implementing the parser, I had a lot of trouble with a couple of transitions:

_ (underscore) -any single character

[^A] -Complement of the character set

I did not find information regarding these transitions during my Googling.

After much trial and error, I came up with the NFA models that work fine. Using these models, you do not have to modify the original algorithm at all.

This "AnyChar" transition is handled in the RegEx.FindMatch method as a special case. If the current state does NOT have a transition over the current input symbol, it checks to see if the current state has a transition over the "AnyChar" symbol. If so, it uses the transition.

The complement of the character set uses an "AnyChar" transition and a "Dummy" transition. If the current state uses a transition that is forbidden (i.e., A in [^A] ), it ends up in a state that has only one transition going away from it — that is the "Dummy" transition and that state is not acceptable. A "Dummy" transition is NEVER used in the actual process and thus the parser reaches a dead-end state, effectively resulting in a mismatch. If the current state does not have any transition over the current input symbol, it uses the "AnyChar" transition and ends up accepting the state effectively matching the correct sub/string.

References:-

Mr. Mike Clark's notes, 
Writing own regular expression parser By Amer Gerzic. 
Regular Expression Matching Can Be Simple And Fast by Russ Cox.
Regular Expression by Mizan Rahman.

Thanks for reading! I hope you liked it.
