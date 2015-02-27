##Append to List
###Description
Appends two list

###Inputs
  * **listA** *(object)* - First list
  * **listB** *(object)* - Second list

###Output
  * None


##Cartesian Product
###Description
Applies a combinator to each pair in the cartesian product of two sequences

###Inputs
  * **comb** *(object)* - Combinator
  * **list1** *(object)* - First list
  * **list2** *(object)* - Second list

###Output
  * None


##Combine
###Description
Applies a combinator to each element in two sequences

###Inputs
  * **comb** *(object)* - Combinator
  * **list1** *(object)* - First list
  * **list2** *(object)* - Second list

###Output
  * None


##Drop From List
###Description
Drops elements from a list

###Inputs
  * **amt** *(object)* - Amount of elements to drop
  * **list** *(object)* - The list to drop elements from

###Output
  * None


##Empty List
###Description
An empty list



###Output
  * None


##Filter
###Description
Filters a sequence by a given predicate

###Inputs
  * **p(x)** *(object)* - Predicate
  * **seq** *(object)* - Sequence to filter

###Output
  * None


##First in List
###Description
Gets the first element of a list

###Inputs
  * **list** *(object)* - A list

###Output
  * None


##Get From List
###Description
Gets an element from a list at a specified index.

###Inputs
  * **index** *(object)* - Index of the element to extract
  * **list** *(object)* - The list to extract elements from

###Output
  * None


##Is Empty List?
###Description
Checks to see if the given list is empty.

###Inputs
  * **list** *(object)* - A list

###Output
  * None


##List
###Description
Makes a new list out of the given inputs



###Output
  * None


##List Length
###Description
Gets the length of a list

###Inputs
  * **list** *(object)* - A list

###Output
  * None


##List Rest
###Description
Gets the list with the first element removed.

###Inputs
  * **list** *(object)* - A list

###Output
  * None


##Make Pair
###Description
Constructs a list pair.

###Inputs
  * **first** *(object)* - The new Head of the list
  * **rest** *(object)* - The new Tail of the list

###Output
  * None


##Map
###Description
Maps a sequence

###Inputs
  * **f(x)** *(object)* - The procedure used to map elements

###Output
  * None


##Number Sequence
###Description
Creates a sequence of numbers

###Inputs
  * **start** *(double)* - Number to start the sequence at
  * **end** *(double)* - Number to end the sequence at
  * **step** *(double)* - Space between numbers

###Output
  * None


##Reduce
###Description
Reduces a sequence.

###Inputs
  * **f(x, a)** *(object)* - Reductor Funtion
  * **a** *(object)* - Seed
  * **seq** *(object)* - Sequence

###Output
  * None


##Reverse
###Description
Reverses a list

###Inputs
  * **list** *(object)* - List to sort

###Output
  * None


##Sort
###Description
Returns a sorted list of numbers or strings.

###Inputs
  * **list** *(object)* - List of numbers or strings to sort

###Output
  * None


##Sort-By
###Description
Returns a sorted list, using the given key mapper.

###Inputs
  * **list** *(object)* - List to sort
  * **c(x)** *(object)* - Key Mapper

###Output
  * None


##Sort-With
###Description
Returns a sorted list, using the given comparitor.

###Inputs
  * **list** *(object)* - List to sort
  * **c(x, y)** *(object)* - Comparitor

###Output
  * None


##Split Pair
###Description
Deconstructs a list pair.

###Inputs
  * **list** *(object)* - 

###Output
  * None


##Take From List
###Description
Takes elements from a list

###Inputs
  * **amt** *(object)* - Amount of elements to extract
  * **list** *(object)* - The list to extract elements from

###Output
  * None
