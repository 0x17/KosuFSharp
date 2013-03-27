namespace KosuFSharp

open System
open NUnit.Framework
open FsUnit

open BrickSprites

[<TestFixture>] 
type BrickSpritesTest () =

    let numBricks = 10

    [<Test>]
    member x.testInitBrickDirs () =
        let brickDirs = initBrickDirs numBricks
        brickDirs |> should haveLength numBricks
       
    [<Test>]
    member x.testInitBrickPositions () =
        let brickPositions = initBrickPositions numBricks
        brickPositions |> should haveLength numBricks