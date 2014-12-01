module PropertyBasedTesting.FsCheckExamples


open System
open FsCheck
open NUnit.Framework


[<Test>]
let ``test that FsCheck is installed correctly``()=
    let revRevIsOrig (xs:list<int>) = List.rev(List.rev xs) = xs
    Check.Quick revRevIsOrig 


