open System
open OpenTK.Input
open Kosu
open Kosu.Utilities
open Kosu.Math
open Kosu.Rendering
open Kosu.Rendering.Sprites
open Kosu.Cameras

[<Struct>]
type Brick(p : Vector2, d : Vector2) = 
    member this.pos = p
    member this.dir = d

module Helpers =
    let rgen = new Random()
    let randFloat(c) = float32(rgen.NextDouble()) * c
    let randInt(n) = rgen.Next(n)
    let randDir() = 
        let alpha = randFloat(2.0f*MathUtils.Pi)
        new Vector2(MathUtils.Cosf(alpha), MathUtils.Sinf(alpha))
    let randPos(maxX, maxY) =
        new Vector2(float32(rgen.Next(maxX)), float32(rgen.Next(maxY)))

module BrickSprites =
    open Helpers
    let brickW, brickH = 100, 100
    let setupSprite(uix, vix)(brick : Brick) =
        let uvDim = 1.0f / 16.0f
        let tcr = new TexCoordRect(float32(uix) * uvDim, (float32(uix)+1.0f) * uvDim, float32(vix) * uvDim, (float32(vix)+1.0f) * uvDim)
        let x = int(brick.pos.X)
        let y = int(brick.pos.Y)
        let rect = new Drawing.Rectangle(x, y, brickW, brickH)
        new TranslatedSprite(tcr, rect)
    let initBrickDirs(numBricks) =
        [for i in 1 .. numBricks do yield randDir()]
    let initBrickPositions(numBricks) =
        [for i in 1 .. numBricks do yield randPos(Globals.ScrW-brickW, Globals.ScrH-brickH)]

module Program =    
    let mutable geomCache : GeometryCache = null
    let mutable sb : SpriteCache = null
    let mutable position = Vector2(320.0f, 240.0f)
    let mutable bricks : Brick list = []    

    let updateCircle() =
        geomCache.Clear()
        geomCache.AddCircle(position, 32.0f, Color4.AliceBlue)

    let updateBrickPositions() =
        sb.Clear()
        bricks
        |> List.map(BrickSprites.setupSprite(3, 3))
        |> List.iter(fun spr -> sb.AddSpr(spr))

    let create() =
        Utils.SetupGlState()

        let orthoCam = OrthoCamera()
        orthoCam.Apply()          
        
        sb <- new SpriteCache("texmap.png", false, true)
        let numBricks = 128
        let brickPositions = BrickSprites.initBrickPositions(numBricks)
        let brickDirections = BrickSprites.initBrickDirs(numBricks)
        for i in 0 .. numBricks-1 do
            bricks <- new Brick(brickPositions.[i], brickDirections.[i]) :: bricks

        updateBrickPositions()

        geomCache <- new GeometryCache()
        updateCircle()
        ()

    let flipHoriz(vec : Vector2) = new Vector2(vec.X * -1.0f, vec.Y)
    let flipVert(vec : Vector2) = new Vector2(vec.X, vec.Y * -1.0f)

    let keepInScrHoriz(brick : Brick) =
        if brick.pos.X + float32(BrickSprites.brickW) > float32(Globals.ScrW) || brick.pos.X < 0.0f then
            new Brick(brick.pos, flipHoriz(brick.dir))
        else brick

    let keepInScrVert(brick : Brick) =
        if brick.pos.Y + float32(BrickSprites.brickH) > float32(Globals.ScrH) || brick.pos.Y < 0.0f then
            new Brick(brick.pos, flipVert(brick.dir))
        else brick

    let keepInScr(brick) = brick |> keepInScrHoriz |> keepInScrVert

    let draw(delta) =
        bricks <- List.map(fun (brick : Brick) -> keepInScr(new Brick(brick.pos+brick.dir, brick.dir))) bricks
        updateBrickPositions()
        Utils.ClearScr()
        geomCache.Render()
        sb.Render()
    
    let move dx dy =
        let movSpeed = 10.0f
        position.X <- position.X + dx * movSpeed
        position.Y <- position.Y + dy * movSpeed
        updateCircle()
        ()
    
    let keyInput(keyDevice : KeyboardDevice)(delta : int64) =
        if keyDevice.[Key.Left] then
            move -1.0f 0.0f
        if keyDevice.[Key.Right] then
            move 1.0f 0.0f
        if keyDevice.[Key.Up] then
            move 0.0f -1.0f
        if keyDevice.[Key.Down] then
            move 0.0f 1.0f
    
    let dispose() =
        geomCache.Dispose()
        sb.Dispose()
            
    let mutable cbacks = StateCallbacks()
    cbacks.Create <- Action(create)
    cbacks.Draw <- Action<_>(draw)
    cbacks.KeyInput <- Action<_,_>(keyInput)
    cbacks.Dispose <- Action(dispose)
    
    type MyState(stateMgr : IStateManager) =
        inherit FuncState(stateMgr, cbacks)

[<EntryPoint>]
let main argv = 
    let config = GameConfiguration("HelloKosuFSharp", 640, 480, typeof<Program.MyState>)
    Launcher.Launch(argv, config)
    0
