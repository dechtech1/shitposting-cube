﻿open System
open System.Threading
open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful
open Suave.Utils.Collections

module Option =

    let ofChoice = function
        | Choice1Of2 x -> Some x
        | _ -> None

let cubeShitpost (s:string) =

    let str = s.ToUpper()

    let len = str.Length

    let space n = [for _ in 1..n do yield " "]

    let explode s = [for c in s -> c.ToString()]

    let rev (s:string) = new string(s.ToCharArray() |> Array.rev)

    sprintf "%s" <| "    " + (String.concat "\n    " ([String.concat " " (explode str)] @
                                [for i in 1..len-2 do yield String.concat " " <| [str.[i].ToString()] @ space(len-2) @ [(rev str).[i].ToString()] @ space(i-1) @ [(rev str).[i].ToString()]] @
                                [String.concat " " <| (str |> rev |> explode) @ space(len-2) @ [str.[0].ToString()]] @
                                [for i in 1..len-2 do yield String.concat " " <| space(i) @ [(rev str).[i].ToString()] @ space(len-2) @ [str.[i].ToString()] @ space(len-2-i) @ [str.[i].ToString()]] @
                                [String.concat " " <| space(len-1) @ explode str]))

let html = """<!doctype html>

<html lang="en">
    <head>
        <meta charset="utf-8">
        <title>The Shitcubeposter</title>
        <meta name="description" content="The Shitcubeposter">
        <meta name="author" content="dechtech1">
    </head>
    <body>
        <div style="margin: auto; width: 70%; padding: 50px; box-sizing: border-box; float: center;">
            <form style="font-family: Helvetica; font-size: 20px; text-align: center;">
                Text to cube:<br>
                <input type="text" id="strToTransform" style="width: 30%; margin: 20px;"><br>
                <input type="button" name="submit" value="Go" onclick="retrieveTransform()" style="width: 50px;"><br>
            </form>
            <div class="codebox" style="border:1px solid black; background-color:#EEEEFF; width:auto; height:auto; overflow:auto; padding:10px; margin:100px auto;">
                <pre>
                    <code id="codeboxCode" style="font-size:0.9em;">
                    </code>
                </pre>
            </div>
            <p style="font-family: Helvetica; font-size: 15px; text-align: center;"><a href="https://github.com/dechtech1/shitposting-cube">Find the backend on GitHub</a></p>
        </div>
        <script>
        function retrieveTransform() {
            var xhttp = new XMLHttpRequest();
            xhttp.onreadystatechange = function() {
                if (this.readyState == 4 && this.status == 200) {
                    document.getElementById("codeboxCode").innerHTML = '\n' + this.responseText;
                }
            };
            xhttp.open("GET", "/api/cubepost?cube=" + document.getElementById("strToTransform").value, true);
            xhttp.send();
        }
        </script>
    </body>
</html>"""

[<EntryPoint>]
let main _ =
    let cts = new CancellationTokenSource()
    let conf = { defaultConfig with cancellationToken = cts.Token }

    let shitpost q =
        defaultArg (Option.ofChoice (q ^^ "cube")) "LOL NO CUBE PARAM" |> cubeShitpost

    let sample : WebPart =
        choose
            [path "/" >=> choose [
                GET >=> request (fun _ -> Writers.setMimeType "text/html" >=> OK html)
                request (fun _ -> Writers.setMimeType "text/plain" >=> RequestErrors.NOT_FOUND (cubeShitpost "LOL HAVE A 404")) ]
             path "/api/cubepost" >=> choose [
                GET  >=> request (fun r -> Writers.setMimeType "text/plain" >=> OK (shitpost r.query))
                request (fun _ -> Writers.setMimeType "text/plain" >=> RequestErrors.NOT_FOUND (cubeShitpost "LOL HAVE A 404")) ]
             request (fun _ -> Writers.setMimeType "text/plain" >=> RequestErrors.NOT_FOUND (cubeShitpost "LOL HAVE A 404"))]

    let _, server = startWebServerAsync conf sample

    Async.Start(server, cts.Token)
    printfn "Make requests now"
    Console.ReadKey true |> ignore

    cts.Cancel()

    0