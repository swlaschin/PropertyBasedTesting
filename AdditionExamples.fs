module AdditionExamples

open NUnit.Framework
open FsCheck
open FsCheck.NUnit


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

    static member ``2 + 2 = 4``() =  
        add 2 2 = 4


// ===================================================
// Nunit tests
// ===================================================

[<Property(QuietOnSuccess = true)>]
let ``Commutative`` x y = 
    commutativeProperty x y

[<Property(Verbose= true)>]
let ``Associative`` x y z = 
    associativeProperty x y z 
    
[<Property(EndSize=300)>]
let ``Left Identity`` x = 
    leftIdentityProperty x 
    




