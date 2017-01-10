import {Logger as _logger} from "./Logger";
import {JobControl as _jobcontrol} from "./JobControl";

export namespace Global{
    export var Logger = new _logger();
    export var JobControl = new _jobcontrol();
    export var escapeName = (name:string) =>'"' + name.replace(/"/g, '\"') + '"';
}