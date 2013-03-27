namespace KosuFSharp

open System
open OpenTK.Input
open Kosu
open Kosu.Utilities
open Kosu.Math
open Kosu.Rendering
open Kosu.Rendering.Sprites
open Kosu.Cameras

module MyGame =
    let mutable geomCache : GeometryCache = null
    let mutable sb : SpriteCache = null
    
    let mutable position = Vector2(320.0f, 240.0f)
    
    let numBricks = 128
    let mutable bricks = BrickSprites.initBricks numBricks

    let updateCircle () =
        geomCache.Clear()
        geomCache.AddCircle(position, 32.0f, Color4.AliceBlue)

    let updateBrickSprites () =
        sb.Clear()
        bricks
        |> List.map BrickSprites.setupSprite
        |> List.iter(fun spr -> sb.AddSpr(spr))

    let create () =
        Utils.SetupGlState()

        let orthoCam = OrthoCamera()
        orthoCam.Apply()
        
        sb <- new SpriteCache("texmap.png", false, true)

        updateBrickSprites ()

        geomCache <- new GeometryCache()
        updateCircle()

    module ScreenDeflector =
        let flipHoriz (vec : Vector2) = new Vector2(vec.X * -1.0f, vec.Y)
        let flipVert (vec : Vector2) = new Vector2(vec.X, vec.Y * -1.0f)

        let keepInScrHoriz (brick : Brick) =
            if brick.pos.X + float32(BrickSprites.brickW) > float32(Globals.ScrW) || brick.pos.X < 0.0f then
                new Brick(brick.pos, flipHoriz(brick.dir))
            else brick

        let keepInScrVert (brick : Brick) =
            if brick.pos.Y + float32(BrickSprites.brickH) > float32(Globals.ScrH) || brick.pos.Y < 0.0f then
                new Brick(brick.pos, flipVert(brick.dir))
            else brick

        let keepInScr brick = brick |> keepInScrHoriz |> keepInScrVert
    
    let moveBrick (brick : Brick) = new Brick(brick.pos+brick.dir, brick.dir)

    let draw delta =
        bricks <- List.map(fun brick -> brick |> moveBrick |> ScreenDeflector.keepInScr) bricks
        updateBrickSprites ()
        
        Utils.ClearScr()
        
        sb.Render()
        geomCache.Render()
    
    let move dx dy =
        let movSpeed = 10.0f
        position.X <- position.X + dx * movSpeed
        position.Y <- position.Y + dy * movSpeed
        updateCircle ()
    
    let keyInput (keyDevice : KeyboardDevice) (delta : int64) =
        if keyDevice.[Key.Left] then
            move -1.0f 0.0f
        if keyDevice.[Key.Right] then
            move 1.0f 0.0f
        if keyDevice.[Key.Up] then
            move 0.0f -1.0f
        if keyDevice.[Key.Down] then
            move 0.0f 1.0f

    let mouseInput (tx : int) (ty : int) (dx : int) (dy : int) (mbtns : MouseBtns) (delta : int64) =
        if mbtns.Lmb then
            position.X <- float32(tx)
            position.Y <- float32(ty)
            updateCircle ()
    
    let dispose () =
        geomCache.Dispose()
        sb.Dispose()
            
    let mutable cbacks = StateCallbacks()
    cbacks.Create <- Action(create)
    cbacks.Draw <- Action<_>(draw)
    cbacks.KeyInput <- Action<_,_>(keyInput)
    cbacks.MouseInput <- Action<_,_,_,_,_,_>(mouseInput)
    cbacks.Dispose <- Action(dispose)
    
    type MyState(stateMgr : IStateManager) =
        inherit FuncState(stateMgr, cbacks)


