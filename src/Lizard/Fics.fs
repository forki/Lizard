namespace LizardChess

open System
open System.Net.Sockets
open System.Threading
open System.Windows.Forms
open System.Text

module Fics = 
    let opts = Opts.load()
    
    //exit
    let exit (socket : Socket) = 
        if socket <> null then 
            socket.Shutdown(SocketShutdown.Both)
            socket.Close()
    
    //get players
    let players (str : string) = 
        let lines = str.Split([| "\n\r" |], StringSplitOptions.None)
        let line = lines |> Array.filter (fun l -> l.StartsWith("Creating:"))
        let bits = line.[0].Split([| ' ' |])
        { blankhdr with White = bits.[1] + bits.[2]
                        Black = bits.[3] + bits.[4]
                        Date = DateTime.Now.ToString("yyyy.MM.dd") }
    
    //get move
    let tmMove (str : string) = 
        let lines = str.Split([| "\n\r" |], StringSplitOptions.None)
        let line = lines |> Array.filter (fun l -> l.StartsWith("<12>"))
        let bits = line.[0].Split([| ' ' |])
        let mv = bits.[29]
        int (bits.[24]), int (bits.[25]), mv, bits.[9] = "W"
//"<12> rnbqkb-r pppppppp -----n-- -------- ----P--- -------- PPPPKPPP RNBQ-BNR
// B -1 0 0 1 1 0 7 Newton Einstein 1 2 12 39 39 119 122 2 K/e1-e2 (0:06) Ke2 0"
//
//This string always begins on a new line, and there are always exactly 31 non-
//empty fields separated by blanks. The fields are:
//
//* the string "<12>" to identify this line.
//* eight fields representing the board position.  The first one is White's
//  8th rank (also Black's 1st rank), then White's 7th rank (also Black's 2nd),
//  etc, regardless of who's move it is.
//* color whose turn it is to move ("B" or "W")
//* -1 if the previous move was NOT a double pawn push, otherwise the chess 
//  board file  (numbered 0--7 for a--h) in which the double push was made
//* can White still castle short? (0=no, 1=yes)
//* can White still castle long?
//* can Black still castle short?
//* can Black still castle long?
//* the number of moves made since the last irreversible move.  (0 if last move
//  was irreversible.  If the value is >= 100, the game can be declared a draw
//  due to the 50 move rule.)
//* The game number
//* White's name
//* Black's name
//* my relation to this game:
//    -3 isolated position, such as for "ref 3" or the "sposition" command
//    -2 I am observing game being examined
//     2 I am the examiner of this game
//    -1 I am playing, it is my opponent's move
//     1 I am playing and it is my move
//     0 I am observing a game being played
//* initial time (in seconds) of the match
//* increment In seconds) of the match
//* White material strength
//* Black material strength
//* White's remaining time
//* Black's remaining time
//* the number of the move about to be made (standard chess numbering -- White's
//  and Black's first moves are both 1, etc.)
//* verbose coordinate notation for the previous move ("none" if there were
//  none) [note this used to be broken for examined games]
//* time taken to make previous move "(min:sec)".
//* pretty notation for the previous move ("none" if there is none)
//* flip field for board orientation: 1 = Black at bottom, 0 = White at bottom.