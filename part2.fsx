(*
From the article "Property-based testing, Part 2"
http://fsharpforfunandprofit.com/posts/property-based-testing-2/
*)


(* ======================================================
Standard Imports
====================================================== *)

System.IO.Directory.SetCurrentDirectory (__SOURCE_DIRECTORY__)
#I @"Packages\FsCheck.1.0.3\lib\net45"
//#I @"Packages\FsCheck.0.9.2.0\lib\net40-Client"  // use older version for VS2012
#r @"FsCheck.dll"

// See troubleshooting FsCheck at foot of "part1.fsx" if you have problems

open System
open FsCheck


(* ======================================================
Different paths, same destination
====================================================== *)

module CommutativeExamples =


    // ----------------------------
    // List.Sort Property - 1st attempt
    // ----------------------------

    let ``+1 then sort should be same as sort then +1`` sortFn aList = 
        let add1 x = x + 1
        
        let result1 = aList |> sortFn |> List.map add1
        let result2 = aList |> List.map add1 |> sortFn 
        result1 = result2

    let goodSort = List.sort
    let badSort aList = aList

    // test
    Check.Quick (``+1 then sort should be same as sort then +1`` goodSort)
    // Ok, passed 100 tests.

    // bad implementation passes too!
    Check.Quick (``+1 then sort should be same as sort then +1`` badSort)
    // Ok, passed 100 tests.


    // ----------------------------
    // List.Sort Property - 2nd attempt
    // ----------------------------

    let ``append minValue then sort should be same as sort then prepend minValue`` sortFn aList = 
        let minValue = Int32.MinValue
       
        let appendThenSort = (aList @ [minValue]) |> sortFn 
        let sortThenPrepend = minValue :: (aList |> sortFn)
        appendThenSort = sortThenPrepend 

    // test
    Check.Quick (``append minValue then sort should be same as sort then prepend minValue`` goodSort)
    // Ok, passed 100 tests.

    // bad implementation fails now!
    Check.Quick (``append minValue then sort should be same as sort then prepend minValue`` badSort)
    // Falsifiable, after 1 test (2 shrinks) 
    // [0]


    // The Enterprise Developer From Hell strikes again
    let badSort2 aList = 
        match aList with
        | [] -> []
        | _ -> 
            let last::reversedTail = List.rev aList 
            if (last = Int32.MinValue) then
                // if min is last, move to front
                let unreversedTail = List.rev reversedTail
                last :: unreversedTail 
            else
                aList // leave alone


    // Oh dear, the bad implementation passes!
    Check.Quick (``append minValue then sort should be same as sort then prepend minValue`` badSort2)
    // Ok, passed 100 tests.

    // ----------------------------
    // List.Sort Property - 3nd attempt
    // ----------------------------

    let ``negate then sort should be same as sort then negate then reverse`` sortFn aList = 
        let negate x = x * -1

        let negateThenSort = aList |> List.map negate |> sortFn 
        let sortThenNegateAndReverse = aList |> sortFn |> List.map negate |> List.rev
        negateThenSort = sortThenNegateAndReverse 

    // test
    Check.Quick ( ``negate then sort should be same as sort then negate then reverse`` goodSort)
    // Ok, passed 100 tests.

    // test
    Check.Quick ( ``negate then sort should be same as sort then negate then reverse``  badSort)
    // Falsifiable, after 1 test (1 shrinks) 
    // [1; 0]

    // test
    Check.Quick ( ``negate then sort should be same as sort then negate then reverse``  badSort2)
    // Falsifiable, after 5 tests (3 shrinks) 
    // [1; 0]

    // ----------------------------
    // List.rev Property - 1st attempt
    // ----------------------------

    let ``append any value then reverse should be same as reverse then prepend same value`` revFn anyValue aList = 
      
        let appendThenReverse = (aList @ [anyValue]) |> revFn 
        let reverseThenPrepend = anyValue :: (aList |> revFn)
        appendThenReverse = reverseThenPrepend 

    // test
    let goodReverse = List.rev
    Check.Quick (``append any value then reverse should be same as reverse then prepend same value`` goodReverse)
    // Ok, passed 100 tests.

    // bad implementation fails
    let badReverse aList = []
    Check.Quick (``append any value then reverse should be same as reverse then prepend same value`` badReverse)
    // Falsifiable, after 1 test (2 shrinks) 
    // true, []

    // bad implementation fails
    let badReverse2 aList = aList 
    Check.Quick (``append any value then reverse should be same as reverse then prepend same value`` badReverse2)
    // Falsifiable, after 1 test (1 shrinks) 
    // true, [false]


(* ======================================================
"There and back again"
====================================================== *)

module InverseExamples =

    // ----------------------------
    // List.rev Property - 1st attempt
    // ----------------------------

    let ``reverse then reverse should be same as original`` revFn aList = 
        let reverseThenReverse = aList |> revFn |> revFn
        reverseThenReverse = aList

    // test
    let goodReverse = List.rev
    Check.Quick (``reverse then reverse should be same as original`` goodReverse)
    // Ok, passed 100 tests.

    // bad implementation passes too!
    let badReverse aList = aList 
    Check.Quick (``reverse then reverse should be same as original`` badReverse)
    // Ok, passed 100 tests.

(* ======================================================
"Hard to prove, easy to verify"
====================================================== *)


module EasyToVerifyExamples = 

    // ----------------------------
    // string split Property 
    // ----------------------------

    let ``concatting the elements of a string split by commas recreates the original string`` aListOfStrings = 
        // helper to make a string
        let concatWithComma s t = s + "," + t
        let originalString = aListOfStrings |> List.fold concatWithComma ""
        
        // now for the property
        let tokens = originalString.Split [| ',' |] 
        let recombinedString = 
            // can use reduce safely because there is always at least one token
            tokens |> Array.reduce concatWithComma 

        // compare the result with the original
        originalString = recombinedString 

    Check.Quick ``concatting the elements of a string split by commas recreates the original string`` 
    Check.Verbose ``concatting the elements of a string split by commas recreates the original string`` 
        
    let ``concatting the elements of a string split by spaces recreates the original string`` aListOfStrings = 
        // helper to make a string
        let concatWithSpace s t = s + " " + t
        let originalString = aListOfStrings |> List.fold concatWithSpace ""
        
        // now for the property
        let tokens = originalString.Split [| ' ' |] 
        let recombinedString = 
            // can use reduce safely because there is always at least one token
            tokens |> Array.reduce concatWithSpace 

        // compare the result with the original
        originalString = recombinedString 

    Check.Quick ``concatting the elements of a string split by spaces recreates the original string`` 
    
    // ----------------------------
    // List.sort Property 
    // ----------------------------

    (*
    let ``adjacent pairs from a list should be ordered`` sortFn aList = 
        let pairs = aList |> sortFn |> Seq.pairwise
        pairs |> Seq.forall (fun (x,y) -> x <= y )
    // type not handled System.IComparable
    *)

    let ``adjacent pairs from a list should be ordered`` sortFn (aList:int list) = 
        let pairs = aList |> sortFn |> Seq.pairwise
        pairs |> Seq.forall (fun (x,y) -> x <= y )
        
    // test
    let goodSort = List.sort
    Check.Quick (``adjacent pairs from a list should be ordered`` goodSort)
    // Ok, passed 100 tests.

    // bad implementation passes
    let badSort aList = []
    Check.Quick (``adjacent pairs from a list should be ordered`` badSort)
    // Ok, passed 100 tests.

    // bad implementation passes and has same length!
    let badSort2 aList = 
        match aList with 
        | [] -> []
        | head::_ -> List.replicate (List.length aList) head 

    // badSort2 [1;2;3]  => [1;1;1]
    Check.Quick (``adjacent pairs from a list should be ordered`` badSort2)


    // Note that the constraint to "int list" was arbitrary. We could use "string list" instead.

    let ``adjacent pairs from a string list should be ordered`` sortFn (aList:string list) = 
        let pairs = aList |> sortFn |> Seq.pairwise
        pairs |> Seq.forall (fun (x,y) -> x <= y )

    Check.Quick (``adjacent pairs from a string list should be ordered`` goodSort)

(* ======================================================
"Some things never change"
====================================================== *)

module InvariantExamples =

    // ----------------------------
    // List.sort Property - 1st attempt using length
    // ----------------------------

    (*
    let ``sort should have same length as original`` sortFn aList = 
        let sorted = aList |> sortFn 
        List.length sorted = List.length aList
    // type not handled System.IComparable
    *)

    let ``sort should have same length as original`` sortFn (aList:int list) = 
        let sorted = aList |> sortFn 
        List.length sorted = List.length aList

    // test
    let goodSort = List.sort
    Check.Quick (``sort should have same length as original`` goodSort )
    // Ok, passed 100 tests.

    // one bad implementation fails
    let badSort aList = []
    Check.Quick (``sort should have same length as original`` badSort )
    // Falsifiable, after 1 test (1 shrink) 
    // [0]

    // bad implementation passes and has same length!
    let badSort2 aList = 
        match aList with 
        | [] -> []
        | head::_ -> List.replicate (List.length aList) head 

    // badSort2 [1;2;3]  => [1;1;1]
    Check.Quick (``sort should have same length as original`` badSort2)

    // also satisfies pairwise property too!
    Check.Quick (EasyToVerifyExamples.``adjacent pairs from a list should be ordered`` badSort2)


    // ----------------------------
    // List.sort Property - 2nd attempt using permutation
    // ----------------------------

    module Permutation = 
        /// given aList and anElement to insert,
        /// generate all possible lists with anElement 
        /// inserted into aList 
        let rec insertElement anElement aList =
            // From http://stackoverflow.com/a/4610704/1136133
            seq { 
                match aList with
                // empty returns a singleton
                | [] -> yield [anElement] 
                // not empty? 
                | first::rest ->
                    // return anElement prepended to the list
                    yield anElement::aList
                    // also return first prepended to all the sublists
                    for sublist in insertElement anElement rest do
                        yield first::sublist
                }

        /// Given a list, return all permutations of it
        let rec permutations aList =
            seq { 
                match aList with
                | [] -> yield []
                | first::rest ->
                    // for each sub-permutation, 
                    // return the first inserted into it somewhere
                    for sublist in permutations rest do
                        yield! insertElement first sublist
                }

        // test
        (*
        permutations ['a';'b';'c'] |> Seq.toList
        //  [['a'; 'b'; 'c']; ['b'; 'a'; 'c']; ['b'; 'c'; 'a']; ['a'; 'c'; 'b'];
        //  ['c'; 'a'; 'b']; ['c'; 'b'; 'a']]

        permutations ['a';'b';'c';'d'] |> Seq.toList
        //  [['a'; 'b'; 'c'; 'd']; ['b'; 'a'; 'c'; 'd']; ['b'; 'c'; 'a'; 'd'];
        //   ['b'; 'c'; 'd'; 'a']; ['a'; 'c'; 'b'; 'd']; ['c'; 'a'; 'b'; 'd'];
        //   ['c'; 'b'; 'a'; 'd']; ['c'; 'b'; 'd'; 'a']; ['a'; 'c'; 'd'; 'b'];
        //   ['c'; 'a'; 'd'; 'b']; ['c'; 'd'; 'a'; 'b']; ['c'; 'd'; 'b'; 'a'];
        //   ['a'; 'b'; 'd'; 'c']; ['b'; 'a'; 'd'; 'c']; ['b'; 'd'; 'a'; 'c'];
        //   ['b'; 'd'; 'c'; 'a']; ['a'; 'd'; 'b'; 'c']; ['d'; 'a'; 'b'; 'c'];
        //   ['d'; 'b'; 'a'; 'c']; ['d'; 'b'; 'c'; 'a']; ['a'; 'd'; 'c'; 'b'];
        //   ['d'; 'a'; 'c'; 'b']; ['d'; 'c'; 'a'; 'b']; ['d'; 'c'; 'b'; 'a']]


        permutations [3;3] |> Seq.toList
        //  [[3; 3]; [3; 3]]

        permutations [1;2;3] |> Seq.toList
        *)

    let ``a sorted list is always a permutation of the original list`` sortFn (aList:int list) = 
        let sorted = aList |> sortFn 
        let permutationsOfOriginalList = Permutation.permutations aList 

        // the sorted list must be in the seq of permutations
        permutationsOfOriginalList 
        |> Seq.exists (fun permutation -> permutation = sorted) 

    /// really really slow!
    // Check.Quick (``a sorted list is always a permutation of the original list`` goodSort)


    // ----------------------------
    // List.sort Property - 3rd attempt using "HasSameContents"
    // ----------------------------

    module ListContents = 

        /// Given an element and a list, and other elements previously skipped,
        /// return a new list without the specified element.
        /// If not found, return None
        let rec withoutElementRec anElement aList skipped = 
            match aList with
            | [] -> None
            | head::tail when anElement = head -> 
                // matched, so create a new list from the skipped and the remaining
                // and return it
                let skipped' = List.rev skipped
                Some (skipped' @ tail)
            | head::tail  -> 
                // no match, so prepend head to the skipped and recurse 
                let skipped' = head :: skipped
                withoutElementRec anElement tail skipped' 

        /// Given an element and a list
        /// return a new list without the specified element.
        /// If not found, return None
        let withoutElement x aList = 
            withoutElementRec x aList [] 
    
        (*
        withoutElement 1 [1;2;3]
        withoutElement 2 [1;2;3]
        withoutElement 3 [1;2;3]
        withoutElement 4 [1;2;3]
        *)

        /// Given two lists, return true if they have the same contents
        /// regardless of order
        let rec isPermutationOf list1 list2 = 
            match list1 with
            | [] -> List.isEmpty list2 // if both empty, true
            | h1::t1 -> 
                match withoutElement h1 list2 with
                | None -> false
                | Some t2 -> 
                    isPermutationOf t1 t2
    
        (*        
        isPermutationOf  [1;2;3]  [3;1;2]
        isPermutationOf  [1;2;3]  
        isPermutationOf  [3;2] [1;2;3] 
        isPermutationOf  [1;2;3]  [4;1;2]
        isPermutationOf  [3;3] [3;3] 
        isPermutationOf  [3;3] [3;3;3] 
        isPermutationOf  [3;3;3] [3;3] 
        *)


    let ``a sorted list has same contents as the original list`` sortFn (aList:int list) = 
        let sorted = aList |> sortFn 
        ListContents.isPermutationOf  aList sorted

    Check.Quick (``a sorted list has same contents as the original list``  goodSort)
    // Ok, passed 100 tests.

    Check.Quick (``a sorted list has same contents as the original list``  badSort2)
    // Falsifiable, after 2 tests (5 shrinks) 
    // [1; 0]


    // ----------------------------
    // List.rev Property - 1st attempt
    // ----------------------------

    let ``reverse should have same length as original`` revFn aList = 
        let reversed = aList |> revFn 
        List.length reversed = List.length aList

    // test
    let goodReverse = List.rev
    Check.Quick (``reverse should have same length as original`` goodReverse)
    // Ok, passed 100 tests.

    // one bad implementation fails
    let badReverse aList = []
    Check.Quick (``reverse should have same length as original`` badReverse)
    // Falsifiable, after 1 test (1 shrink) 
    // [true]

    // another bad implementation passes
    let badReverse2 aList = aList 
    Check.Quick (``reverse should have same length as original`` badReverse2)
    // Ok, passed 100 tests.


(* ======================================================
Property Combinations
====================================================== *)

module PropertyCombinations =

    // what if you to combine properties?

    let ``list is sorted``sortFn (aList:int list) = 
        let prop1 = EasyToVerifyExamples.``adjacent pairs from a list should be ordered`` sortFn aList 
        let prop2 = InvariantExamples.``a sorted list has same contents as the original list`` sortFn aList 
        prop1 .&. prop2 

    // test
    let goodSort = List.sort
    Check.Quick (``list is sorted`` goodSort )
    // Ok, passed 100 tests.

    // test
    let badSort aList = []
    Check.Quick (``list is sorted`` badSort )
    // Falsifiable, after 1 test (0 shrinks) 
    // [0]

    // -------------------------
    // not clear which property failed above
    // ...so add labels
    // -------------------------

    let ``list is sorted (labelled)``sortFn (aList:int list) = 
        let prop1 = EasyToVerifyExamples.``adjacent pairs from a list should be ordered`` sortFn aList 
                    |@ "adjacent pairs from a list should be ordered"
        let prop2 = InvariantExamples.``a sorted list has same contents as the original list`` sortFn aList 
                    |@ "a sorted list has same contents as the original list"
        prop1 .&. prop2 

    Check.Quick (``list is sorted (labelled)`` badSort )
    //  Falsifiable, after 1 test (2 shrinks)
    //  Label of failing property: a sorted list has same contents as the original list
    //  [0]

    
(* ======================================================
"Solving a smaller problem"
====================================================== *)

module StructuralInduction =

    // ----------------------------
    // List.sort Property - 1st attempt
    // ----------------------------

    let rec ``First element is <= than second, and tail is also sorted`` sortFn (aList:int list) = 
        let sortedList = aList |> sortFn 
        match sortedList with
        | [] -> true
        | [first] -> true
        | [first;second] -> 
            first <= second
        | first::second::tail -> 
            first <= second &&
            let subList = second::tail 
            ``First element is <= than second, and tail is also sorted`` sortFn subList  

    // test
    let goodSort = List.sort
    Check.Quick (``First element is <= than second, and tail is also sorted`` goodSort )
    // Ok, passed 100 tests.

    // bad implementation passes
    let badSort aList = []
    Check.Quick (``First element is <= than second, and tail is also sorted`` badSort )
    // Ok, passed 100 tests.

    // bad implementation passes and has same length!
    let badSort2 aList = 
        match aList with 
        | [] -> []
        | head::_ -> List.replicate (List.length aList) head 

    // badSort2 [1;2;3]  => [1;1;1]
    Check.Quick (``First element is <= than second, and tail is also sorted`` badSort2)
    // Ok, passed 100 tests.

(* ======================================================
"The more things change, the more they stay the same" 
====================================================== *)

module IdempotentExamples = 

    // ----------------------------
    // List.sort Property 
    // ----------------------------
    
    let ``sorting twice gives the same result as sorting once`` sortFn (aList:int list) =
        let sortedOnce = aList |> sortFn 
        let sortedTwice = aList |> sortFn |> sortFn 
        sortedOnce = sortedTwice

    // test
    let goodSort = List.sort
    Check.Quick (``sorting twice gives the same result as sorting once`` goodSort )
    // Ok, passed 100 tests.

    // ----------------------------
    // Non-idempotent service
    // ----------------------------

    type NonIdempotentService() =
        let mutable data = 0
        member this.Get() = 
            data
        member this.Set value = 
            data <- value

    let ``querying NonIdempotentService after update gives the same result`` value1 value2 =
        let service = NonIdempotentService()
        service.Set value1

        // first GET 
        let get1 = service.Get()

        // another task updates the data store
        service.Set value2

        // second GET called just like first time
        let get2 = service.Get() 
        get1 = get2 

    Check.Quick ``querying NonIdempotentService after update gives the same result``
    // Falsifiable, after 2 tests 

    // ----------------------------
    // Idempotent service
    // ----------------------------

    type IdempotentService() =
        let mutable data = Map.empty
        member this.GetAsOf (dt:DateTime) = 
            data |> Map.find dt
        member this.SetAsOf (dt:DateTime) value = 
            data <- data |> Map.add dt value

    let ``querying IdempotentService after update gives the same result`` value1 value2 =
        let service = IdempotentService()
        let dt1 = DateTime.Now.AddMinutes(-1.0)
        service.SetAsOf dt1 value1

        // first GET 
        let get1 = service.GetAsOf dt1 

        // another task updates the data store
        let dt2 = DateTime.Now
        service.SetAsOf dt2 value2

        // second GET called just like first time
        let get2 = service.GetAsOf dt1 
        get1 = get2 
    
    Check.Quick ``querying IdempotentService after update gives the same result``
    // Ok, passed 100 tests.

(* ======================================================
"Test Oracle"
====================================================== *)

module TestOracleExamples = 


    /// An implementation of sort using insertion sort
    module InsertionSort = 
        
        // Insert a new element into a list by looping over the list.
        // As soon as you find a larger element, insert in front of it
        let rec insert newElem list = 
            match list with 
            | head::tail when newElem > head -> 
                head :: insert newElem tail
            | other -> // including empty list
                newElem :: other 
 
        // Sorts a list by inserting the head into the rest of the list 
        // after the rest have been sorted
        let rec sort list = 
            match list with
            | []   -> []
            | head::tail -> 
                insert head (sort tail)

        // test
        // insertionSort  [5;3;2;1;1]

    let ``sort should give same result as insertion sort`` sortFn (aList:int list) = 
        let sorted1 = aList |> sortFn 
        let sorted2 = aList |> InsertionSort.sort
        sorted1 = sorted2 

    // test
    let goodSort = List.sort
    Check.Quick (``sort should give same result as insertion sort`` goodSort)
    // Ok, passed 100 tests.

    // test
    let badSort aList = aList 
    Check.Quick (``sort should give same result as insertion sort`` badSort)
    // Falsifiable, after 4 tests (6 shrinks) 
    // [1; 0]

(* ======================================================
Roman Numerals
====================================================== *)


// From fsharpforfunandprofit.com/posts/roman-numeral-kata/
module CrossCheck_RomanNumerals = 

    let arabicToRomanUsingTallying arabic = 
       (String.replicate arabic "I")
        .Replace("IIIII","V")
        .Replace("VV","X")
        .Replace("XXXXX","L")
        .Replace("LL","C")
        .Replace("CCCCC","D")
        .Replace("DD","M")
        // optional substitutions
        .Replace("IIII","IV")
        .Replace("VIV","IX")
        .Replace("XXXX","XL")
        .Replace("LXL","XC")
        .Replace("CCCC","CD")
        .Replace("DCD","CM")


    let biQuinaryDigits place (unit,five,ten) arabic =
      let digit =  arabic % (10*place) / place
      match digit with
      | 0 -> ""
      | 1 -> unit
      | 2 -> unit + unit
      | 3 -> unit + unit + unit
      | 4 -> unit + five // changed to be one less than five 
      | 5 -> five
      | 6 -> five + unit
      | 7 -> five + unit + unit
      | 8 -> five + unit + unit + unit
      | 9 -> unit + ten  // changed to be one less than ten
      | _ -> failwith "Expected 0-9 only"

    let arabicToRomanUsingBiQuinary arabic = 
      let units = biQuinaryDigits 1 ("I","V","X") arabic
      let tens = biQuinaryDigits 10 ("X","L","C") arabic
      let hundreds = biQuinaryDigits 100 ("C","D","M") arabic
      let thousands = biQuinaryDigits 1000 ("M","?","?") arabic
      thousands + hundreds + tens + units

    let ``biquinary should give same result as tallying`` arabic = 
        let tallyResult = arabicToRomanUsingTallying arabic 
        let biquinaryResult = arabicToRomanUsingBiQuinary arabic 
        tallyResult = biquinaryResult 

    Check.Quick ``biquinary should give same result as tallying``
    // ArgumentException: The input must be non-negative.

    let arabicNumber = Arb.Default.Int32() |> Arb.filter (fun i -> i > 0 && i <= 4000) 
    let ``for all values of arabicNumber biquinary should give same result as tallying`` = 
        Prop.forAll arabicNumber ``biquinary should give same result as tallying`` 
    Check.Quick ``for all values of arabicNumber biquinary should give same result as tallying``
    // Ok, passed 100 tests.



(* ======================================================
Dollar properties
====================================================== *)

module Dollar_V1 =

    // OO style class with members
    type Dollar(amount:int) =
        member val Amount  = amount with get, set
        member this.Add add = 
            this.Amount <- this.Amount + add
        member this.Times multiplier  = 
            this.Amount <- this.Amount * multiplier  
        static member Create amount  = 
            Dollar amount  
        
    // interactive test
    let d = Dollar.Create 2
    d.Amount  // 2
    d.Times 3 
    d.Amount  // 6
    d.Add 1
    d.Amount  // 7

    // ----------------------------
    // Inverse: setter/getter
    // ----------------------------

    let ``set then get should give same result`` value = 
        let obj = Dollar.Create 0
        obj.Amount <- value
        let newValue = obj.Amount
        value = newValue 

    Check.Quick ``set then get should give same result`` 
    // Ok, passed 100 tests.

    // ----------------------------
    // Idempotent: setter
    // ----------------------------

    let ``set amount is idempotent`` value = 
        let obj = Dollar.Create 0
        obj.Amount <- value
        let afterFirstSet = obj.Amount
        obj.Amount <- value
        let afterSecondSet = obj.Amount
        afterFirstSet = afterSecondSet 

    Check.Quick ``set amount is idempotent`` 
    // Ok, passed 100 tests.

module Dollar_V2 =

    // Immutable OO style class with members
    type Dollar(amount:int) =
        member val Amount  = amount 
        member this.Add add = 
            Dollar (amount + add)
        member this.Times multiplier  = 
            Dollar (amount * multiplier)
        static member Create amount  = 
            Dollar amount  
        
    // interactive test
    let d1 = Dollar.Create 2
    d1.Amount  // 2
    let d2 = d1.Times 3 
    d2.Amount  // 6
    let d3 = d2.Add 1
    d3.Amount  // 7

    // ----------------------------
    // Different paths: times inside and outside
    // ----------------------------

    let ``create then times should be same as times then create`` start multiplier = 
        let d0 = Dollar.Create start
        let d1 = d0.Times(multiplier)
        let d2 = Dollar.Create (start * multiplier)     
        d1 = d2

    Check.Quick ``create then times should be same as times then create``
    // Falsifiable, after 1 test 

    let ``dollars with same amount must be equal`` amount = 
        let d1 = Dollar.Create amount 
        let d2 = Dollar.Create amount 
        d1 = d2

    Check.Quick ``dollars with same amount must be equal`` 
    // Falsifiable, after 1 test 

module Dollar_V3 =

    // Switch to immutable record type with members
    type Dollar = {amount:int } 
        with 
        member this.Add add = 
            {amount = this.amount + add }
        member this.Times multiplier  = 
            {amount = this.amount * multiplier }
        static member Create amount  = 
            {amount=amount}

    let ``dollars with same amount must be equal`` amount = 
        let d1 = Dollar.Create amount 
        let d2 = Dollar.Create amount 
        d1 = d2

    Check.Quick ``dollars with same amount must be equal`` 
    // Ok, passed 100 tests.

    // going up and across the top is the same as going across the bottom and up
    let ``create then times should be same as times then create`` start multiplier = 
        let d0 = Dollar.Create start
        let d1 = d0.Times(multiplier)
        let d2 = Dollar.Create (start * multiplier)     
        d1 = d2

    Check.Quick ``create then times should be same as times then create``
    // Ok, passed 100 tests.

    // going up and across the top and down is the same as just going across the bottom
    let ``create then times then get should be same as times`` start multiplier = 
        let d0 = Dollar.Create start
        let d1 = d0.Times(multiplier)
        let a1 = d1.amount
        let a2 = start * multiplier     
        a1 = a2

    Check.Quick ``create then times then get should be same as times``
    // Ok, passed 100 tests.


    let ``create then times then add should be same as times then add then create`` start multiplier adder = 
        let d0 = Dollar.Create start
        let d1 = d0.Times(multiplier)
        let d2 = d1.Add(adder)
        let directAmount = (start * multiplier) + adder
        let d3 = Dollar.Create directAmount 
        d2 = d3

    Check.Quick ``create then times then add should be same as times then add then create`` 
    // Ok, passed 100 tests.

module Dollar_V4 =

    type Dollar = {amount:int } 
        with 
        member this.Map f = 
            {amount = f this.amount}
        member this.Times multiplier = 
            this.Map (fun a -> a * multiplier)
        member this.Add adder = 
            this.Map (fun a -> a + adder)
        static member Create amount  = 
            {amount=amount}

    let ``create then map should be same as map then create`` start f = 
        let d0 = Dollar.Create start
        let d1 = d0.Map f  
        let d2 = Dollar.Create (f start)     
        d1 = d2

    Check.Quick ``create then map should be same as map then create`` 
    // Ok, passed 100 tests.

    Check.Verbose ``create then map should be same as map then create`` 

    (*
0:
18
<fun:Invoke@3000>
1:
7
<fun:Invoke@3000>
-- etc
98:
47
<fun:Invoke@3000>
99:
36
<fun:Invoke@3000>
Ok, passed 100 tests.
    *)

    let ``create then map should be same as map then create2`` start (F (_,f)) = 
        let d0 = Dollar.Create start
        let d1 = d0.Map f  
        let d2 = Dollar.Create (f start)     
        d1 = d2

    Check.Verbose ``create then map should be same as map then create2`` 

    (*
0:
0
{ 0->1 }
1:
0
{ 0->0 }
2:
2
{ 2->-2 }
-- etc
98:
-5
{ -5->-52 }
99:
10
{ 10->28 }
Ok, passed 100 tests.
    *)






