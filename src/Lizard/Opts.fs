namespace LizardChess

open Nessos.FsPickler.Json
open System.IO

module Opts = 
    //STORAGE elements
    let optfile = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Options.json")
    let json = FsPickler.CreateJsonSerializer()
    
    ///load - get stored options 
    let load() = 
        if File.Exists optfile then 
            let str = File.ReadAllText(optfile)
            json.UnPickleOfString<Options>(str)
        else 
            { Opnfol = "ToDo"
              Tstfol = "ToDo"
              Engfol = "ToDo"
              Gmfol = "ToDo"
              Rnum = 1
              Rskip = 1
              Lnum = 1
              Lskip = 1
              Eng = "ToDo"
              Emindepth = 1
              Emaxdepth = 20
              Elog = false
              Geng = "ToDo"
              Gsecpm = 1
              Guseopn = false
              Funame = "ToDo"
              Fpass = "ToDo"
              Ftime = 1
              Fuopn = false }
    
    ///save - save options to file
    let save (opts) = 
        let str = json.PickleToString<Options>(opts)
        File.WriteAllText(optfile, str)