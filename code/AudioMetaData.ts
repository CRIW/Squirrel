export class AudioMetaData{
    public artists: string[];
    public album_artist: string;
    public album: string;
    public title: string;
    public track: string;
    public path: string;
    public genre: string;
    public date: string;

    public validate(){
        if(this.album_artist = null){
            this.album_artist = this.artists[0];
        }
    }

    public save(){
        
    }
}