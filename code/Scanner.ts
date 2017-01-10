import {AudioMetaData} from "./AudioMetaData";
import fs = require('fs');
import {Global} from "./Global";
import {Database} from "./Database";
import child_process = require('child_process');

export class Scanner{
    db: Database;

    scanCallback:  (AudioMetaData)=>void;
    constructor(db: Database){
        this.db = db;
    }

    public scan(){
        
    }

    public rescan(){
        
    }

    scanDirectory(dirname: string):AudioMetaData[]{
        var jobid = Global.JobControl.createJob("Scanner","Scan Directory", "Scanning " + dirname);
        var dirs = fs.readdirSync(dirname);
        var metaData = [];
        dirs.forEach(dir => {
            if(!dir.startsWith(".")){
                var stat = fs.statSync(dirname + "/" + dir);
                if(stat.isDirectory()){
                    var datas = this.scanDirectory(dirname + "/" + dir);
                    metaData = metaData.concat(datas);
                }else if(stat.isFile() && dir.endsWith(".mp3")){
                    var data = this.scanFile(dirname + "/" + dir);
                    metaData.push(data);
                }
            }
        });
        Global.JobControl.finishJob(jobid);
        return metaData;
    }

    public scanFile(filename: string):AudioMetaData{
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
        //console.log(stdout + "");
        //Global.JobControl.finishJob(jobid);
        return amd;
    }




}