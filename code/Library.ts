import {Scanner} from "./Scanner";
import {Database} from "./Database";
import {AudioMetaData} from "./AudioMetaData";

export class Library{
    public baseDir: string;
    database: Database;
    scanner: Scanner;
    constructor(baseDir: string, dbFile: string){
        this.database = new Database(dbFile);
        //this.scanner = new Scanner(baseDir, database);
    }



}