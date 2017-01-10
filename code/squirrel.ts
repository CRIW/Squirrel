import express = require("express");
import {Scanner} from "./Scanner";

class Squirrel{
    private app: express.Application;
    constructor(){
        this.app = express();
        this.app.use(express.static("static"));

       this.app.get("/",(req,res)=>{
            res.sendFile(__dirname + "/static/index.html");
        });
    }

    public run(){
        this.app.listen(3000,() => {
            console.log("Running on port 3000");
        });
    }

    public test(){
        var s:Scanner = new Scanner(null);
        console.log(JSON.stringify(s.scanDirectory("/home/apexys/Music/Rena/Music")));
    }
}

var sq = new Squirrel();
sq.test();