open System
open System.IO
open System.Windows.Forms
open System.Drawing

type Entry = { Label : string; Values : int list }

let distance (values1 : int list, values2 : int list) =
    values1
    |> List.zip values2
    |> List.map (fun x -> Math.Pow(float (fst x) - float(snd x), 2.0))
    |> List.sum

let loadValues (filename : string) =
           File.ReadAllLines(filename)
                |> Seq.ofArray
                |> Seq.skip 1
                |> Seq.map (fun line ->
                     {  
                        Label =  line.Substring(0,line.IndexOf(','));
                        Values = line.Split(',')
                                    |> Seq.ofArray
                                    |> Seq.skip (1)
                                    |> Seq.map( fun n -> Convert.ToInt32(n))
                                    |> Seq.toList
                     })
                |> Seq.toList

let kNN (entries : Entry list, newEntry : string * int[] , k : int) =
     entries  |> List.map(fun x -> ( x.Label, distance(x.Values, snd (newEntry) |>Array.toList )))
              |> List.sortBy (fun x -> snd x)
              |> Seq.ofList
              |> Seq.take k
              |> Seq.countBy (fun x -> fst x)
              |> Seq.toList


let drawDigit (pixels:float[], label:string) = 
        let tile = 20 
        let form = new Form(TopMost = true, Visible = true,  Width = 29 * tile, Height = 29 * tile)
        let panel = new Panel(Dock = DockStyle.Fill)
        panel.BackColor <- Color.Black
        form.Controls.Add(panel)
        let graphics = panel.CreateGraphics()
        pixels |> Array.iteri (fun i p ->
            let col = i % 28
            let row = i / 28
            let color = Color.FromArgb(int p, int p, int p)
            let brush = new SolidBrush(color)
            graphics.FillRectangle(brush, col * tile, row * tile, tile, tile))
        let point = new PointF((float32)5, (float32)5)
        let font = new Font(family = FontFamily.GenericSansSerif, emSize = (float32)30)
        graphics.DrawString(label, font, new SolidBrush(Color.YellowGreen), point)
        form.Show()

let root = __SOURCE_DIRECTORY__
let training = root + @"\data\digits\trainingsample.csv"
let validation = root + @"\data\digits\validationsample.csv"

let loaded = loadValues training
let test = loadValues validation

for i = 400 to 405 do
    let pixels = test.[i].Values |> List.map float |> List.toArray
    let k = 5
    let labels = kNN(loaded, ("X", test.[i].Values |> List.toArray), k)
    let guess = fst(labels |> List.item 0)
    drawDigit (pixels, (sprintf "guess: %s, real: %s" guess test.[i].Label))