import {Global} from "./Global";
export class JobControl{
    private jobs: Map<number,Job>;
    private jobId: number;
    constructor(){
        this.jobs = new Map<number, Job>();
        this.jobId = 0;
    }

    public createJob(category: string, name: string, description: string):number{
        return this.addJob(new Job(category,name,description));
    }

    public addCancellingMethod(id:number, cancel:()=>void){
        if(this.jobs.has(id)){
            var j = this.jobs.get(id);
            j.cancel = cancel;
            this.jobs.set(j.id,j);
        }else{
            Global.Logger.Log("JobControl.addCancellingMethod", "Job " + id + " does not exist");
        }
    }
    
    public addJob(job: Job):number{
        job.id = this.jobId;
        this.jobId++;
        this.jobs.set(job.id,job);
        Global.Logger.Log("JobControl.addJob", "Added job " + job.id);
        return job.id;
    }

    public finishJob(id: number){
         if(this.jobs.has(id)){
            var j = this.jobs.get(id);
            this.jobs.delete(j.id);
        }else{
            Global.Logger.Log("JobControl.finishJob", "Job " + id + " does not exist");
        }
    }

    public deleteJob(id: number){
        if(this.jobs.has(id)){
            var j = this.jobs.get(id);
            j.cancel();
            this.jobs.delete(j.id);
        }else{
            Global.Logger.Log("JobControl.deleteJob", "Job " + id + " does not exist");
        }
    }

    public getAllJobs():IterableIterator<Job>{
        return this.jobs.values();
    }


}

class Job{
    public id:number;
    public Name:string;
    public Category: string;
    public Description: string;
    public cancel: ()=>void;
    constructor(category: string, name: string, description: string){
        this.Category = category;
        this.Name = name;
        this.Description =description;
        this.id = null;
    }

    public isCancellable():boolean{
        return this.cancel != null;
    }
}