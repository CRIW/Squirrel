import sqlite3 = require('sqlite3');
import {AudioMetaData} from "./AudioMetaData";
export class Database{
    private db: sqlite3.Database;
    constructor(dbpath: string){
        this.db = new sqlite3.Database(dbpath);
        this.db.serialize(() => {
            this.db.run("CREATE TABLE IF NOT EXISTS artists (name TEXT, songid INTEGER)");
            this.db.run("CREATE TABLE IF NOT EXISTS tracks (songid INTEGER PRIMARY KEY AUTOINCREMENT, path TEXT, album TEXT, title TEXT)");
        });
    }

    public close(){
        this.db.close();
        this.db = null;
    }

    public search(term: string):Promise<string[]>{
        return new Promise((resolve, reject)=>{
            var stmt = this 
        });
    }

    public getAlbumsForArtist(artist: string):Promise<string[]>{
        return new Promise((resolve, reject)=>{
            var stmt = this.db.prepare("SELECT DISTINCT t.album FROM tracks as t JOIN artists as a on t.songid == a.songid where a.name == ?",artist);
            stmt.all((err,rows)=>{
                if(err){
                    reject(err);
                }else{
                    resolve(rows);
                }
            });
        });
    }

    public getArtists():Promise<string[]>{
        return new Promise((resolve, reject)=>{
            var stmt = this.db.prepare("SELECT DISTINCT name FROM artists");
            stmt.all((err,rows)=>{
                if(err){
                    reject(err);
                }else{
                    resolve(rows);
                }
            });
        });
    }

    public getAllPaths():Promise<string[]>{
        return new Promise((resolve, reject)=>{
            var stmt = this.db.prepare("SELECT path from tracks");
            stmt.all((err,rows)=>{
                if(err){
                    reject(err);
                }else{
                    resolve(rows);
                }
            });
        });
    }

    public containsFile(filename: string):Promise<boolean>{
        return new Promise((resolve, reject)=>{
            var stmt = this.db.prepare("SELECT songid FROM tracks WHERE path = ?",filename);
            stmt.get((err,row)=>{
                if(err){
                    reject(err);
                }else{
                    if(row != undefined){
                        resolve(true);
                    }else{
                        resolve(false);
                    }
                }
            });
        });
    }

    public getSongIdForPath(filename: string):Promise<number>{
        return new Promise((resolve, reject)=>{
            var stmt = this.db.prepare("SELECT songid FROM tracks WHERE path = '?'",filename);
            stmt.get((err,row)=>{
                if(err){
                    reject(err);
                }else{
                    resolve(row);
                }
            });
        });
    }

    public addSongSync(amd: AudioMetaData){

    }

    public addSong(amd: AudioMetaData):Promise<void>{
        return new Promise<void>((resolve, reject) =>{
            var stmt = this.db.prepare("INSERT INTO tracks(path, album, title) values (?,?,?)",amd.path,amd.album,amd.title);
            stmt.run(err => {
                if(err){
                    reject(err);
                }else{
                    this.getSongIdForPath(amd.path).then(id =>{
                        var pms = amd.artists.map(artist => {
                            return new Promise((resolve, reject) => {
                                var stmt = this.db.prepare("INSERT INTO artists(name, songid) values (?,?)", artist,id);
                                stmt.run((err)=>{
                                    if(err){
                                        reject(err);
                                    }else{
                                        resolve();
                                    }
                                });
                            });
                        });
                        Promise.all(pms).then(()=>resolve).catch(err => reject(err)); 
                    }).catch(err => {
                        reject(err);
                    });
                }
            });
        });
    }

}