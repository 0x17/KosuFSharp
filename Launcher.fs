module KosuFSharp.Launcher

open Kosu

[<EntryPoint>]
let main argv = 
    let config = GameConfiguration("HelloKosuFSharp", 640, 480, typeof<KosuFSharp.MyGame.MyState>)
    Launcher.Launch(argv, config)
    0
    