open System
open System.Windows.Forms

type Question =
    | MCQ of string * string list * int 
    | Written of string * string  

let quizQuestions =
    Map.ofList [
        1, MCQ("What is the capital of Japan?", ["Tokyo"; "Kyoto"; "Osaka"], 1)
        2, MCQ("What is the capital of Australia?", ["Canberra"; "Sydney"; "Melbourne"], 1)
        3, MCQ("What is the capital of Canada?", ["Toronto"; "Ottawa"; "Vancouver"], 2)
        4, MCQ("What is the capital of Egypt?", ["Cairo"; "Alexandria"; "Giza"], 1)

        5, MCQ("Which country won the FIFA World Cup in 2018?", ["France"; "Croatia"; "Germany"], 1)
        6, MCQ("Which player is known as 'El Pibe de Oro'?", ["Lionel Messi"; "Diego Maradona"; "Cristiano Ronaldo"], 2)
        7, MCQ("Which club has won the most UEFA Champions League titles?", ["Real Madrid"; "Barcelona"; "AC Milan"], 1)

        8, Written("What is the capital of Italy?", "Rome")
        9, Written("What is the capital of India?", "New Delhi")

        10, Written("Who won the Ballon d'Or in 2023?", "Lionel Messi")
    ]

let mutable currentQuestion = 1
let mutable userAnswers = Map.empty<int, string>

[<EntryPoint>]
let main argv =
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

    let displayQuestion questionId =
        match quizQuestions.TryFind(questionId) with
        | Some (MCQ(text, options, _)) ->
            questionLabel.Text <- text
            answerBox.Visible <- false
            mcqButtons |> List.iteri (fun idx btn ->
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
        | None -> // End of quiz
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

    mcqButtons |> List.iteri (fun idx btn ->
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
