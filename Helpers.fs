namespace KosuFSharp

open System
open Kosu.Math
open Kosu.Utilities
open Kosu.Rendering

module Helpers =
    let rgen = new Random()
    let randFloat c = float32(rgen.NextDouble()) * c
    let randInt n = rgen.Next(n)
    let randDir () = 
        let alpha = randFloat(2.0f*MathUtils.Pi)
        new Vector2(MathUtils.Cosf(alpha), MathUtils.Sinf(alpha))
    let randPos maxX maxY =
        new Vector2(float32(rgen.Next(maxX)), float32(rgen.Next(maxY)))
    let cellToTcr uix vix =
        let uvDim = 1.0f / 16.0f
        new TexCoordRect(float32(uix) * uvDim, (float32(uix)+1.0f) * uvDim, float32(vix) * uvDim, (float32(vix)+1.0f) * uvDim)
