export class Logger{
    constructor(){

    }

    public Log(source:string, message:string){
        var d = new Date();
        var to2d = (s:number) => (s + "").length < 2 ? "0" + s : "" + s;
        var message = to2d(d.getHours()) + ":" + to2d(d.getMinutes()) + ":" + to2d(d.getSeconds()) + " [" + source + "] " + message;
        console.log(message);
    }
}