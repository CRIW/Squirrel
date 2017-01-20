

import express = require("express");

export class Server{
    private app: express.Application;
    constructor(){
        this.app = express();
        this.app.use(express.static("static"));

       this.app.get("/",(req,res)=>{
            res.sendFile(__dirname + "/static/index.html");
        });

        this.app.get("/search/:term", (req, res)=>{
            
        });

        this.app.get("/mp3/:id", (req,res)=>{

        });
    }

    public run(){
        this.app.listen(3000,() => {
                console.log("Running on port 3000");
        });
    }


}