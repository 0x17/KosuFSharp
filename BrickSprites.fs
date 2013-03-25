namespace KosuFSharp
open System
open Kosu
open Kosu.Math
open Kosu.Rendering.Sprites
open Kosu.Rendering

[<Struct>]
type Brick(p : Vector2, d : Vector2) = 
    member this.pos = p
    member this.dir = d

module BrickSprites =
    open Helpers
    let brickW, brickH = 100, 100
    let spriteTcr = cellToTcr 3 3
    let setupSprite (brick : Brick) =
        let x = int(brick.pos.X)
        let y = int(brick.pos.Y)
        let rect = new Drawing.Rectangle(x, y, brickW, brickH)
        new TranslatedSprite(spriteTcr, rect)
    let initBrickDirs numBricks =
        [for i in 1 .. numBricks do yield randDir()]
    let initBrickPositions numBricks =
        [for i in 1 .. numBricks do yield randPos (Globals.ScrW-brickW) (Globals.ScrH-brickH)]
    let initBricks numBricks =
        let brickPositions = initBrickPositions(numBricks)
        let brickDirections = initBrickDirs(numBricks)
        List.init numBricks (fun i -> new Brick(brickPositions.[i], brickDirections.[i]))
        
