(*
From the article "Property-based testing"
http://fsharpforfunandprofit.com/posts/property-based-testing/
*)


(* ======================================================
Use F# to play with FsCheck interactively


====================================================== *)

// === Setup ===
// 1. Install Chocolately from http://chocolatey.org/
// 2. Install NuGet command line
//    cinst nuget.commandline
// 3. Install FsCheck.Nunit in same directory as script
//    nuget install FsCheck.Nunit -o Packages 

// sets the current directory to be same as the script directory
System.IO.Directory.SetCurrentDirectory (__SOURCE_DIRECTORY__)

// assumes nuget install FsCheck.Nunit has been run 
// so that assemblies are available under the current directory
#I @"Packages\FsCheck.1.0.3\lib\net45"
//#I @"Packages\FsCheck.0.9.2.0\lib\net40-Client"  // use older version for VS2012
#I @"Packages\NUnit.2.6.3\lib"

#r @"FsCheck.dll"
#r @"nunit.framework.dll"

// See troubleshooting FsCheck at foot of the file if you have problems

open System
open FsCheck
open NUnit.Framework


//=====================================
// Part 1 - the EDFH
//=====================================

module EDFH_V1 = 

    let add x y =
        if x=1 && y=2 then 
            3
        else
            0    

    [<Test>]
    let ``When I add 1 + 2, I expect 3``()=
        let result = add 1 2
        Assert.AreEqual(3,result)


module EDFH_V2 = 

    let add x y =
        if x=1 && y=2 then 
            3
        else if x=2 && y=2 then 
            4
        else
            0    

    [<Test>]
    let ``When I add 1 + 2, I expect 3``()=
        let result = add 1 2
        Assert.AreEqual(3,result)

    [<Test>]
    let ``When I add 2 + 2, I expect 4``()=
        let result = add 2 2
        Assert.AreEqual(4,result)
        
    
module EDFH_V3 = 

    let add x y =
        if x=1 && y=2 then 
            3
        else if x=2 && y=2 then 
            4
        else
            0    

    let rand = System.Random()
    let randInt() = rand.Next()

    [<Test>]
    let ``When I add two random numbers, I expect their sum``()=
        let x = randInt()
        let y = randInt()
        let expected = x + y
        let actual = add x y
        Assert.AreEqual(expected,actual)


    [<Test>]
    let ``When I add two random numbers (100 times), I expect their sum``()=
        for _ in [1..100] do
            let x = randInt()
            let y = randInt()
            let expected = x + y
            let actual = add x y
            Assert.AreEqual(expected,actual)

//=====================================
// Part 2 - Property-based tests (homebrew)
//=====================================

module HomeBrewPropertyTests_V1 = 

    let rand = System.Random()
    let randInt() = rand.Next()

    let add x y = x * y  // malicious implementation

    [<Test>]
    let ``When I add two numbers, the result should not depend on parameter order``()=
        for _ in [1..100] do
            let x = randInt()
            let y = randInt()
            let result1 = add x y
            let result2 = add y x // reversed params
            Assert.AreEqual(result1,result2)


module HomeBrewPropertyTests_V2 = 

    let rand = System.Random()
    let randInt() = rand.Next()

    let add x y = y - x  // malicious implementation

    [<Test>]
    let ``Adding 1 twice is the same as adding 2``()=
        for _ in [1..100] do
            let x = randInt()
            let y = randInt()
            let result1 = x |> add 1 |> add 1
            let result2 = x |> add 2 
            Assert.AreEqual(result1,result2)


module HomeBrewPropertyTests_V3 = 

    let rand = System.Random()
    let randInt() = rand.Next()

    let add x y = 0  // malicious implementation

    [<Test>]
    let ``When I add two numbers, the result should not depend on parameter order``()=
        for _ in [1..100] do
            let x = randInt()
            let y = randInt()
            let result1 = add x y
            let result2 = add y x // reversed params
            Assert.AreEqual(result1,result2)

    [<Test>]
    let ``Adding 1 twice is the same as adding 2``()=
        for _ in [1..100] do
            let x = randInt()
            let y = randInt()
            let result1 = x |> add 1 |> add 1
            let result2 = x |> add 2 
            Assert.AreEqual(result1,result2)


module HomeBrewPropertyTests_V4 = 

    let rand = System.Random()
    let randInt() = rand.Next()

    let add x y = x + y  // correct implementation

    [<Test>]
    let ``When I add two numbers, the result should not depend on parameter order``()=
        for _ in [1..100] do
            let x = randInt()
            let y = randInt()
            let result1 = add x y
            let result2 = add y x // reversed params
            Assert.AreEqual(result1,result2)

    [<Test>]
    let ``Adding 1 twice is the same as adding 2``()=
        for _ in [1..100] do
            let x = randInt()
            let y = randInt()
            let result1 = x |> add 1 |> add 1
            let result2 = x |> add 2 
            Assert.AreEqual(result1,result2)

    [<Test>]
    let ``Adding zero is the same as doing nothing``()=
        for _ in [1..100] do
            let x = randInt()
            let result1 = x |> add 0
            let result2 = x  
            Assert.AreEqual(result1,result2)

module HomeBrewPropertyTests_V5 = 

    let rand = System.Random()
    let randInt() = rand.Next()

    let add x y = x + y  // correct implementation

    let propertyCheck property = 
        // property has type: int -> int -> bool
        for _ in [1..100] do
            let x = randInt()
            let y = randInt()
            let result = property x y
            Assert.IsTrue(result)

    let commutativeProperty x y = 
        let result1 = add x y
        let result2 = add y x // reversed params
        result1 = result2

    [<Test>]
    let ``When I add two numbers, the result should not depend on parameter order``()=
        propertyCheck commutativeProperty 

    let adding1TwiceIsAdding2OnceProperty x _ = 
        let result1 = x |> add 1 |> add 1
        let result2 = x |> add 2 
        result1 = result2

    [<Test>]
    let ``Adding 1 twice is the same as adding 2``()=
        propertyCheck adding1TwiceIsAdding2OnceProperty 

    let identityProperty x _ = 
        let result1 = x |> add 0
        result1 = x

    [<Test>]
    let ``Adding zero is the same as doing nothing``()=
        propertyCheck identityProperty 


//=====================================
// Part 3 - Property-based tests (using FsCheck)
//=====================================

module AdditionProperties_Using_FsCheck = 

    let add x y = x + y  // correct implementation

    let commutativeProperty (x,y) = 
        let result1 = add x y
        let result2 = add y x // reversed params
        result1 = result2

    // check the property interactively            
    Check.Quick commutativeProperty 

    let adding1TwiceIsAdding2OnceProperty x = 
        let result1 = x |> add 1 |> add 1
        let result2 = x |> add 2 
        result1 = result2

    // check the property interactively            
    Check.Quick adding1TwiceIsAdding2OnceProperty 

    let identityProperty x = 
        let result1 = x |> add 0
        result1 = x

    // check the property interactively            
    Check.Quick identityProperty 


module AdditionProperties_Using_FsCheck_BadImplementation_1 = 

    let add x y =
        x * y // malicious implementation

    let commutativeProperty (x,y) = 
        let result1 = add x y
        let result2 = add y x // reversed params
        result1 = result2

    // check the property interactively            
    Check.Quick commutativeProperty   // satisfies OK!

    let adding1TwiceIsAdding2OnceProperty x = 
        let result1 = x |> add 1 |> add 1
        let result2 = x |> add 2 
        result1 = result2

    // check the property interactively            
    Check.Quick adding1TwiceIsAdding2OnceProperty  // fails for input "1"

    let identityProperty x = 
        let result1 = x |> add 0
        result1 = x

    // check the property interactively            
    Check.Quick identityProperty   // fails for input "1"


module AdditionProperties_Using_FsCheck_BadImplementation_2 = 

    let add x y = 
        if (x < 10) || (y < 10) then
            x + y  // correct for low values
        else
            x * y  // incorrect for high values

    let commutativeProperty (x,y) = 
        let result1 = add x y
        let result2 = add y x // reversed params
        result1 = result2

    // check the property interactively            
    Check.Quick commutativeProperty 

    let adding1TwiceIsAdding2OnceProperty x = 
        let result1 = x |> add 1 |> add 1
        let result2 = x |> add 2 
        result1 = result2

    // check the property interactively            
    Check.Quick adding1TwiceIsAdding2OnceProperty 
    Check.Verbose adding1TwiceIsAdding2OnceProperty 

    let identityProperty x = 
        let result1 = x |> add 0
        result1 = x

    // check the property interactively            
    Check.Quick identityProperty 
    Check.Verbose identityProperty 

    let associativeProperty x y z = 
        let result1 = add x (add y z)    // x + (y + z)
        let result2 = add (add x y) z    // (x + y) + z
        result1 = result2

    // check the property interactively            
    Check.Quick associativeProperty 
    // associativeProperty 8 2 9  // true
    // associativeProperty 8 2 10  // false

module AdditionProperties_Using_FsCheck_BadImplementation_3 = 

    let add x y = 
        if (x < 25) || (y < 25) then
            x + y  // correct for low values
        else
            x * y  // incorrect for high values

    let associativeProperty x y z = 
        let result1 = add x (add y z)    // x + (y + z)
        let result2 = add (add x y) z    // (x + y) + z
        result1 = result2

    // check the property interactively            
    Check.Quick associativeProperty 

    // with tracing/logging
    Check.Verbose associativeProperty 


//=====================================
// Part 4 - Understanding FsCheck - Generators and Shrinking
//=====================================

module GenerationExamples = 

    // get the generator for ints
    let intGenerator = Arb.generate<int>
    // generate three ints with a maximum size of 1
    Gen.sample 1 3 intGenerator    // e.g. [0; 0; -1]
    // generate three ints with a maximum size of 10
    Gen.sample 10 3 intGenerator   // e.g. [-4; 8; 5]
    // generate three ints with a maximum size of 100
    Gen.sample 100 3 intGenerator  // e.g. [-37; 24; -62] 

    // see how the value are clustered around the center point
    intGenerator 
    |> Gen.sample 10 1000 
    |> Seq.groupBy id 
    |> Seq.map (fun (k,v) -> (k,Seq.length v))
    |> Seq.sortBy (fun (k,v) -> k)
    |> Seq.toList 

    // see how the value are clustered around the center point
    intGenerator 
    |> Gen.sample 30 10000 
    |> Seq.groupBy id 
    |> Seq.map (fun (k,v) -> (k,Seq.length v))
    |> Seq.sortBy (fun (k,v) -> k)
    |> Seq.toList 

    // ------------------------------------
    // tuple generator
    // ------------------------------------
    let tupleGenerator = Arb.generate<int*int*int>

    // generate 3 tuples with a maximum size of 1
    Gen.sample 1 3 tupleGenerator 
    // result: [(0, 0, 0); (0, 0, 0); (0, 1, -1)]

    // generate 3 tuples with a maximum size of 10
    Gen.sample 10 3 tupleGenerator 
    // result: [(-6, -4, 1); (2, -2, 8); (1, -4, 5)]

    // generate 3 tuples with a maximum size of 100
    Gen.sample 100 3 tupleGenerator 
    // result: [(-2, -36, -51); (-5, 33, 29); (13, 22, -16)]

    // ------------------------------------
    // int option generator
    // ------------------------------------
    let intOptionGenerator = Arb.generate<int option>
    // generate 10 int options with a maximum size of 5
    Gen.sample 5 10 intOptionGenerator 
    // result:  [Some 0; Some -1; Some 2; Some 0; Some 0; 
    //           Some -4; null; Some 2; Some -2; Some 0]

    // ------------------------------------
    // int list generator
    // ------------------------------------
    let intListGenerator = Arb.generate<int list>
    // generate 10 int lists with a maximum size of 5
    Gen.sample 5 10 intListGenerator 
    // result:  [ []; []; [-4]; [0; 3; -1; 2]; [1]; 
    //            [1]; []; [0; 1; -2]; []; [-1; -2]]

    // ------------------------------------
    // string generator
    // ------------------------------------
    let stringGenerator = Arb.generate<string>

    // generate 3 strings with a maximum size of 1
    Gen.sample 1 3 stringGenerator 
    // result: [""; "!"; "I"]

    // generate 3 strings with a maximum size of 10
    Gen.sample 10 3 stringGenerator 
    // result: [""; "eiX$<M"; "U%0Ika&r"]

    // ------------------------------------
    // the generator will work with user defined types too!
    // ------------------------------------
    type Color = Red | Green of int | Blue of bool
    let colorGenerator = Arb.generate<Color>
    // generate 10 colors with a maximum size of 50
    Gen.sample 50 10 colorGenerator 
    // result:  [Green -47; Red; Red; Red; Blue true; 
    //           Green 2; Blue false; Red; Blue true; Green -12]


    // ------------------------------------
    type Point = {x:int; y:int; color: Color}
    let pointGenerator = Arb.generate<Point>
    // generate 10 points with a maximum size of 50
    Gen.sample 50 10 pointGenerator 

    (* result
    [{x = -8; y = 12; color = Green -4;}; 
     {x = 28; y = -31; color = Green -6;}; 
     {x = 11; y = 27; color = Red;}; 
     {x = -2; y = -13; color = Red;};
     {x = 6; y = 12; color = Red;};
     // etc
    *)

module ShrinkExamples = 


    Arb.shrink 100 |> Seq.toList 
    //  [0; 50; 75; 88; 94; 97; 99]

    Arb.shrink (1,2,3) |> Seq.toList 
    //  [(0, 2, 3); (1, 0, 3); (1, 1, 3); (1, 2, 0); (1, 2, 2)]

    Arb.shrink "abcd" |> Seq.toList 
    //  ["bcd"; "acd"; "abd"; "abc"; "abca"; "abcb"; "abcc"; "abad"; "abbd"; "aacd"]
    
    Arb.shrink [1;2;3] |> Seq.toList 
    //  [[2; 3]; [1; 3]; [1; 2]; [1; 2; 0]; [1; 2; 2]; [1; 0; 3]; [1; 1; 3]; [0; 2; 3]]

    // silly property to test
    let isSmallerThan80 x = x < 80

    isSmallerThan80 100 // false, so start shrinking

    isSmallerThan80 0 // true
    isSmallerThan80 50 // true
    isSmallerThan80 75 // true
    isSmallerThan80 88 // false, so shrink again

    Arb.shrink 88 |> Seq.toList 
    //  [0; 44; 66; 77; 83; 86; 87]
    isSmallerThan80 0 // true
    isSmallerThan80 44 // true
    isSmallerThan80 66 // true
    isSmallerThan80 77 // true
    isSmallerThan80 83 // false, so shrink again

    Arb.shrink 83 |> Seq.toList 
    //  [0; 42; 63; 73; 78; 81; 82]
    // smallest failure is 81, so shrink again

    Arb.shrink 81 |> Seq.toList 
    //  [0; 41; 61; 71; 76; 79; 80]
    // smallest failure is 80
    // no smaller value found


//=====================================
// Part 5 - Configurating FsCheck 
//=====================================

module ControllingTheNumberOfTests = 

    // silly property to test
    let isSmallerThan80 x = x < 80

    Check.Quick isSmallerThan80 
    // result: Ok, passed 1000 tests.

    let config = {
        Config.Quick with 
            MaxTest = 1000
        }
    Check.One(config,isSmallerThan80 )
    // result: Ok, passed 1000 tests.

    let config = {
        Config.Quick with 
            MaxTest = 10000
        }
    Check.One(config,isSmallerThan80 )
    // result: Falsifiable, after 8660 tests (1 shrink) (StdGen (539845487,295941658)):
    //         80

    let config = {
        Config.Quick with 
            EndSize = 1000
        }
    Check.One(config,isSmallerThan80 )
    // result: Falsifiable, after 21 tests (4 shrinks) (StdGen (1033193705,295941658)):
    //         80

module LoggingTheTests = 

    let add x y = 
        if (x < 25) || (y < 25) then
            x + y  // correct for low values
        else
            x * y  // incorrect for high values

    let associativeProperty x y z = 
        let result1 = add x (add y z)    // x + (y + z)
        let result2 = add (add x y) z    // (x + y) + z
        result1 = result2

    // --------------------------------------
    // customizing FsCheck output 
    // --------------------------------------

    // create a function for displaying a test
    let printTest testNum [x;y;z] = 
        sprintf "#%-3i %3O %3O %3O\n" testNum x y z

    // create a function for displaying a shrink
    let printShrink [x;y;z] = 
        sprintf "shrink %3O %3O %3O\n" x y z

    // create a new FsCheck configuration
    let config = {
        Config.Quick with 
            Replay = Random.StdGen (995282583,295941602) |> Some 
            Every = printTest 
            EveryShrink = printShrink
        }

    // check the given property with the new configuration
    Check.One(config,associativeProperty)

(*
#0    -1  -1   0
#1     0   0   0
#2    -2   0  -3
#3     1   2   0
#4    -4   2  -3
#5     3   0  -3
#6    -1  -1  -1
#7    -7  -5  -9
#8     3  -4  -6
#9     1  -1  -3
#10    1   0   1
#11    0  -6   4
#12   -1   2  -1
#13   -6   4   2
#14   -5  -3  -3
#15    1  -1   1
#16    9  -9   0
#17    5  -2  -2
#18  -13   3 -14
#19   -9  13 -10
#20   -5  -8  13
#21   -2   4  -1
#22    1   2  -1
#23   -1   1  -6
#24    4  -2  19
#25    0   0   0
#26    2   2  -1
#27    4   1   8
#28   12 -20  19
#29   15  24  -2
#30   26 -14   6
#31    4  -8  -8
#32   -7 -11   0
#33   -6   0 -12
#34  -16  -4 -11
#35   28  -6   3
#36   10 -23 -14
#37   10   3   9
#38  -24 -26   1
#39    3  -5  -7
#40   -7  10   3
#41    5  -1   7
#42  -23   8 -20
#43   -4   8   8
#44    4   5  -8
#45  -15   5  -8
#46  -21 -25  29
#47  -10  -7 -13
#48   -4 -19  23
#49   46  -4  50
shrink  35  -4  50
shrink  27  -4  50
shrink  26  -4  50
shrink  25  -4  50
shrink  25   4  50
shrink  25   2  50
shrink  25   1  50
shrink  25   1  38
shrink  25   1  29
shrink  25   1  26
Falsifiable, after 50 tests (10 shrinks) (StdGen (995282583,295941602)):
25
1
26

*)


    // Let's look at how shrinking was done.
    // The last set of inputs (46,-4,50) was false, so shrinking started
    associativeProperty 46 -4 50  // false, so shrink

    // list of possible shrinks starting at 46
    Arb.shrink 46 |> Seq.toList 
    // result [0; 23; 35; 41; 44; 45]

    // find the next test that fails when shrinking the x parameter 
    let x,y,z = (46,-4,50) 
    Arb.shrink x
    |> Seq.tryPick (fun x -> if associativeProperty x y z then None else Some (x,y,z) )
    // answer (35, -4, 50)

    // find the next test that fails when shrinking the x parameter 
    let x,y,z = (35,-4,50) 
    Arb.shrink x
    |> Seq.tryPick (fun x -> if associativeProperty x y z then None else Some (x,y,z) )
    // answer (27, -4, 50)

    // find the next test that fails when shrinking the x parameter 
    let x,y,z = (27,-4,50) 
    Arb.shrink x
    |> Seq.tryPick (fun x -> if associativeProperty x y z then None else Some (x,y,z) )
    // answer (26, -4, 50)

    // find the next test that fails when shrinking the x parameter 
    let x,y,z = (26,-4,50) 
    Arb.shrink x
    |> Seq.tryPick (fun x -> if associativeProperty x y z then None else Some (x,y,z) )
    // answer (25, -4, 50)

    // find the next test that fails when shrinking the x parameter 
    let x,y,z = (25,-4,50) 
    Arb.shrink x
    |> Seq.tryPick (fun x -> if associativeProperty x y z then None else Some (x,y,z) )
    // answer None

    // finished with the x parameter!

    // find the next test that fails when shrinking the y parameter 
    let x,y,z = (25,-4,50) 
    Arb.shrink y
    |> Seq.tryPick (fun y -> if associativeProperty x y z then None else Some (x,y,z) )
    // answer (25, 4, 50)

    // find the next test that fails when shrinking the y parameter 
    let x,y,z = (25,4,50) 
    Arb.shrink y
    |> Seq.tryPick (fun y -> if associativeProperty x y z then None else Some (x,y,z) )
    // answer (25, 2, 50)

    // find the next test that fails when shrinking the y parameter 
    let x,y,z = (25,2,50) 
    Arb.shrink y
    |> Seq.tryPick (fun y -> if associativeProperty x y z then None else Some (x,y,z) )
    // answer (25, 1, 50)

    // find the next test that fails when shrinking the y parameter 
    let x,y,z = (25,1,50) 
    Arb.shrink y
    |> Seq.tryPick (fun y -> if associativeProperty x y z then None else Some (x,y,z) )
    // answer None

    // finished with the y parameter!

    // find the next test that fails when shrinking the z parameter 
    let x,y,z = (25,1,50) 
    Arb.shrink z
    |> Seq.tryPick (fun z -> if associativeProperty x y z then None else Some (x,y,z) )
    // answer (25, 1, 38)

    // find the next test that fails when shrinking the z parameter 
    let x,y,z = (25,1,38) 
    Arb.shrink z
    |> Seq.tryPick (fun z -> if associativeProperty x y z then None else Some (x,y,z) )
    // answer (25, 1, 29)

    // find the next test that fails when shrinking the z parameter 
    let x,y,z = (25,1,29) 
    Arb.shrink z
    |> Seq.tryPick (fun z -> if associativeProperty x y z then None else Some (x,y,z) )
    // answer (25, 1, 26)

    // find the next test that fails when shrinking the z parameter 
    let x,y,z = (25,1,26) 
    Arb.shrink z
    |> Seq.tryPick (fun z -> if associativeProperty x y z then None else Some (x,y,z) )
    // answer None

    // finished with all parameters!
    // final counterexample is (25,1,26) 


module AddingConditions_V1 = 

    let additionIsNotMultiplication x y = 
        x + y <> x * y

    Check.Quick additionIsNotMultiplication 
    // Falsifiable, after 3 tests (0 shrinks) (StdGen (2037191079,295941699)):
    // 0
    // 0

module AddingConditions_V2 = 

    let additionIsNotMultiplication x y = 
        x + y <> x * y

    let preCondition x y = 
        (x,y) <> (0,0)

    let additionIsNotMultiplication_withPreCondition x y = 
        preCondition x y ==> additionIsNotMultiplication x y 

    Check.Quick additionIsNotMultiplication_withPreCondition
    // Falsifiable, after 38 tests (0 shrinks) (StdGen (1870180794,295941700)):
    // 2
    // 2


module AddingConditions_V3 = 

    let additionIsNotMultiplication x y = 
        x + y <> x * y

    let preCondition x y = 
        (x,y) <> (0,0)
        && (x,y) <> (2,2)

    let additionIsNotMultiplication_withPreCondition x y = 
        preCondition x y ==> additionIsNotMultiplication x y 

    Check.Quick additionIsNotMultiplication_withPreCondition


//=====================================
// Part 6 - Combining multiple properties 
//=====================================

module CombiningProperties = 

    let add x y = x + y // good implementation

    let commutativeProperty x y = 
        add x y = add y x    

    let associativeProperty x y z = 
        add x (add y z) = add (add x y) z    

    let leftIdentityProperty x = 
        add x 0 = x

    let rightIdentityProperty x = 
        add 0 x = x

    type AdditionSpecification =
        static member ``Commutative`` x y = commutativeProperty x y
        static member ``Associative`` x y z = associativeProperty x y z 
        static member ``Left Identity`` x = leftIdentityProperty x 
        static member ``Right Identity`` x = rightIdentityProperty x 

    Check.QuickAll<AdditionSpecification>()


module CombiningPropertiesAndExamples = 

    let add x y = x + y // good implementation

    let commutativeProperty x y = 
        add x y = add y x    

    let associativeProperty x y z = 
        add x (add y z) = add (add x y) z    

    let leftIdentityProperty x = 
        add x 0 = x

    let rightIdentityProperty x = 
        add 0 x = x


    type AdditionSpecification =
        static member ``Commutative`` x y = commutativeProperty x y
        static member ``Associative`` x y z = associativeProperty x y z 
        static member ``Left Identity`` x = leftIdentityProperty x 
        static member ``Right Identity`` x = rightIdentityProperty x 

        // some examples as well
        static member ``1 + 2 = 3``() =  
            add 1 2 = 3

        static member ``1 + 2 = 2 + 1``() =  
            add 1 2 = add 2 1 

        static member ``42 + 0 = 0 + 42``() =  
            add 42 0 = add 0 42 


    Check.QuickAll<AdditionSpecification>()



//=====================================
// Installing and Troubleshooting FsCheck 
//=====================================

// run the following two lines to check that FsCheck is working
let revRevIsOrig (xs:list<int>) = List.rev(List.rev xs) = xs
Check.Quick revRevIsOrig 

(*
If you get no errors, then everything is good.

If you *do* get errors, it's probably because you are on an older version of Visual Studio. Upgrade to VS2013 or failing that, do the following:

* First make sure you have the latest F# core installed ([currently 3.1](https://stackoverflow.com/questions/20332046/correct-version-of-fsharp-core)).
* Make sure your that your `app.config` has the [appropriate binding redirects](http://blog.ploeh.dk/2014/01/30/how-to-use-fsharpcore-430-when-all-you-have-is-431/).
* Make sure that your NUnit assemblies are being referenced locally rather than from the GAC.

These steps should ensure that compiled code works. 

With F# interactive, it can be trickier. If you are not using VS2013, you might run into errors 
such as `System.InvalidCastException: Unable to cast object of type 'Arrow'`.

The best cure for this is to upgrade to VS2013!  
Failing that, you can use an older version of FsCheck, such as 0.9.2 (which I have tested successfully with VS2012) -- see the top of the file.
*)

