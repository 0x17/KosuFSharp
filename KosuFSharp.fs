open System
open OpenTK.Input
open Kosu
open Kosu.Utilities
open Kosu.Math
open Kosu.Rendering
open Kosu.Rendering.Sprites
open Kosu.Cameras

type Brick = { pos : Vector2; dir : Vector2 }

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
    let setupSprite(brick : Brick) =
        let uvDim = 1.0f / 16.0f
        let uix, vix = Helpers.randInt(8), Helpers.randInt(8)
        let tcr = new TexCoordRect(float32(uix) * uvDim, (float32(uix)+1.0f) * uvDim, float32(vix) * uvDim, (float32(vix)+1.0f) * uvDim)
        let x = int(brick.pos.X)
        let y = int(brick.pos.Y)
        let rect = new Drawing.Rectangle(x, y, 100, 100)
        new TranslatedSprite(tcr, rect)
    let initBrickDirs(numBricks) =
        [for i in 1 .. numBricks do yield randDir()]
    let initBrickPositions(numBricks) =
        [for i in 1 .. numBricks do yield randPos(Globals.ScrW, Globals.ScrH)]

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
        |> List.map BrickSprites.setupSprite
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
            bricks <- { pos=brickPositions.[i]; dir=brickDirections.[i] } :: bricks

        updateBrickPositions()

        geomCache <- new GeometryCache()
        updateCircle()
        ()

    let draw(delta) =
        bricks <- List.map(fun brick -> { pos=brick.pos+brick.dir; dir=brick.dir }) bricks
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
