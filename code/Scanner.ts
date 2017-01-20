import {AudioMetaData} from "./AudioMetaData";
import fs = require('fs');
import {Global} from "./Global";
import {Database} from "./Database";
import child_process = require('child_process');

export class Scanner{
    db: Database;
    baseDir: string;
    scanlist: string[] = [];

    constructor(db: Database, baseDir: string){
        this.db = db;
        this.baseDir = baseDir;
    }

    public scan(){
        this.scanDirectory(this.baseDir);
    }

    public rescan(){
        
    }

    public validate(){

    }

    scanDirectory(dirname: string){
        var jobid = Global.JobControl.createJob("Scanner","Scan Directory", "Scanning " + dirname);
        var dirs = fs.readdirSync(dirname);
        var jobs = [];
        dirs.forEach(dir => {
            if(!dir.startsWith(".")){
                var stat = fs.statSync(dirname + "/" + dir);
                if(stat.isDirectory()){
                    this.scanDirectory(dirname + "/" + dir);
                }else if(stat.isFile() && dir.endsWith(".mp3")){
                    jobs.push(this.scanFile(dirname + "/" + dir));
                }
            }
        });
        Promise.all(jobs).then(() => Global.JobControl.finishJob(jobid));
    }

    handleScan():Promise<void>{
        return new Promise<void>((resolve, reject) => {
            if(this.scanlist.length > 0){
                var toScan = this.scanlist.pop();
                var stat = fs.statSync(toScan);
                if(stat.isDirectory()){
                    this.scanDirectory(toScan);
                }
            }
        });
        
    }

    public scanFile(filename: string):Promise<void>{
        return new Promise<void>((resolve, reject) => {
            this.db.containsFile(filename).then(contains => {
                if(!contains){
                    //var jobid = Global.JobControl.createJob("Scanner","Scan File", "Scanning " + filename);
                    var options = {
                        "stdio" : [null, "pipe", null]
                    };
                    var stdout = child_process.execSync("ffmpeg -i " + Global.escapeName(filename) + " -f ffmetadata -", options);
                    var lines = (stdout + "").split("\n");
                    var amd = new AudioMetaData();
                    lines.forEach(line => {
                        if(line.startsWith("title")){
                            amd.title = line.substring("title=".length);
                        }else if(line.startsWith("artist")){
                            amd.artists = line.substring("artist=".length).split(', ');
                        }else if(line.startsWith("album_artist")){
                            amd.album_artist = line.substring("album_artist=".length);
                        }else if(line.startsWith("track")){
                            amd.track = line.substring("track=".length);
                        }else if(line.startsWith("genre")){
                            amd.genre = line.substring("genre=".length);
                        }else if(line.startsWith("date")){
                            amd.date = line.substring("date=".length);
                        }else if(line.startsWith("album")){
                            amd.album = line.substring("album=".length);
                        }
                    });
                    amd.path = filename;
                    amd.validate();
                    this.db.addSong(amd).then(() => resolve());
                }else{
                    resolve();
                }
        });
        });
    }




}