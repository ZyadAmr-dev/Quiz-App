open System
let mutable score = 0

type Question =
    | MCQ of string * string list * int // 1-index not 0
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

let askQuestion (question: Question) =
    match question with
    | MCQ(text, options, _) ->
        printfn "\n%s" text
        options
        |> List.iteri (fun idx opt -> printfn "%d. %s" (idx + 1) opt)
        printf "Your choice (number): "
        let input = Console.ReadLine()
        match Int32.TryParse(input) with
        | true, choice -> Some(choice.ToString())
        | _ -> None
    | Written(text, _) ->
        printfn "\n%s" text
        printf "Your answer: "
        Some(Console.ReadLine())


let calculateScore (questions: Map<int, Question>) (answers: Map<int, string>) =
    questions
    |> Map.fold (fun score key question ->
        match question, answers.TryFind(key) with
        | MCQ(_, _, correctIdx), Some userAnswer when userAnswer = string correctIdx -> score + 1
        | Written(_, correctAnswer), Some userAnswer when userAnswer.Trim().ToLower() = correctAnswer.Trim().ToLower() -> score + 1
        | _ -> score
    ) 0


let runQuiz () =
    printfn "Welcome to the Quiz Application by F#!"
    let userAnswers =
        quizQuestions
        |> Map.fold (fun (answers: Map<int,string>) key question ->
            let userAnswer = askQuestion question
            match userAnswer with
            | Some answer -> answers.Add(key, string answer)
            | None -> answers
        ) Map.empty

    let score = calculateScore quizQuestions userAnswers
    printfn "\nQuiz Ended!"
    printfn "Your score is: %d out of %d" score quizQuestions.Count

runQuiz()