import express = require("express");
import {Scanner} from "./Scanner";
import {Database} from "./Database";

class Squirrel{
    
    public run(){
        
    }

    public test(){
        var db:Database = new Database("squirrel.db");
        var s:Scanner = new Scanner(db, "/home/apexys/Music/Rena/Music");
        s.scan();
        //db.close();
    }
}

var sq = new Squirrel();
sq.test();