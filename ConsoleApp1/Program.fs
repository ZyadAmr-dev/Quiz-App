open System
open System.IO
open System.Windows.Forms
open System.Text.Json

// Define the Question type
type Question = 
    | MCQ of text:string * options:string list * correctIndex:int
    | Written of text:string * correctAnswer:string

// JSON Serialization helpers
type QuestionJson = {
    id: int
    ``type``: string
    text: string
    options: string[] option
    correctAnswer: string option
    correctIndex: int option
}

let loadQuestionsFromJson (filePath: string) =
    let jsonText = File.ReadAllText(filePath)
    let jsonDoc = JsonDocument.Parse(jsonText)
    let root = jsonDoc.RootElement.GetProperty("questions")

    root.EnumerateArray()
    |> Seq.map (fun questionJson ->
        let id = questionJson.GetProperty("id").GetInt32()
        let questionType = questionJson.GetProperty("type").GetString()
        let text = questionJson.GetProperty("text").GetString()

        match questionType with 
        | "MCQ" ->
            let options = 
                questionJson.GetProperty("options").EnumerateArray() 
                |> Seq.map (fun opt -> opt.GetString()) 
                |> Seq.toList
            let correctIndex = questionJson.GetProperty("correctIndex").GetInt32()
            id, MCQ(text, options, correctIndex)
        | "Written" ->
            let correctAnswer = questionJson.GetProperty("correctAnswer").GetString()
            id, Written(text, correctAnswer)
        | _ -> failwith "Unknown question type"
    )
    |> Map.ofSeq

[<EntryPoint>]
let main argv =
    let quizQuestions = loadQuestionsFromJson "questions.json"
    let form = new Form(Text = "F# Quiz", Width = 700, Height = 400)
    let questionLabel = new Label(AutoSize = true, Top = 50, Left = 50)
    form.Controls.Add(questionLabel)

    let answerBox = new TextBox(Top = 150, Left = 50, Width = 200)
    form.Controls.Add(answerBox)

    let mcqButtons = [1..3] |> List.map (fun i -> 
        let btn = new Button(Top = 100 + (i * 30), Left = 50, Width = 200, Text = "")
        btn.Visible <- false
        form.Controls.Add(btn)
        btn
    )

    let submitButton = new Button(Text = "Submit", Top = 250, Left = 50)
    form.Controls.Add(submitButton)

    let resultLabel = new Label(AutoSize = true, Top = 300, Left = 50, Text = "")
    form.Controls.Add(resultLabel)

    let mutable currentQuestion = 1
    let mutable userAnswers = Map.empty<int, string>

    let displayQuestion questionId =
        match quizQuestions.TryFind(questionId) with
        | Some (MCQ(text, options, _)) -> 
            questionLabel.Text <- text
            answerBox.Visible <- false
            mcqButtons 
            |> List.iteri (fun idx btn -> 
                if idx < options.Length then 
                    btn.Visible <- true 
                    btn.Text <- options.[idx]
                else 
                    btn.Visible <- false
            )
        | Some (Written(text, _)) -> 
            questionLabel.Text <- text
            answerBox.Visible <- true
            mcqButtons |> List.iter (fun btn -> btn.Visible <- false)
        | None -> 
            // End of quiz
            questionLabel.Text <- "Quiz Completed!"
            answerBox.Visible <- false
            mcqButtons |> List.iter (fun btn -> btn.Visible <- false)
            submitButton.Visible <- false

            let score = 
                quizQuestions 
                |> Map.fold (fun acc key question -> 
                    match question, userAnswers.TryFind(key) with
                    | MCQ(_, _, correctIdx), Some answer when answer = string correctIdx -> acc + 1
                    | Written(_, correctAnswer), Some answer when answer.Trim().ToLower() = correctAnswer.Trim().ToLower() -> acc + 1
                    | _ -> acc
                ) 0
            
            resultLabel.Text <- sprintf "Your score: %d out of %d" score quizQuestions.Count

    mcqButtons 
    |> List.iteri (fun idx btn -> 
        btn.Click.Add (fun _ -> 
            userAnswers <- userAnswers.Add(currentQuestion, string (idx + 1))
            currentQuestion <- currentQuestion + 1
            displayQuestion currentQuestion
        )
    )

    submitButton.Click.Add (fun _ -> 
        if answerBox.Visible then 
            userAnswers <- userAnswers.Add(currentQuestion, answerBox.Text)
        currentQuestion <- currentQuestion + 1
        displayQuestion currentQuestion
    )

    displayQuestion currentQuestion
    Application.Run(form)
    0