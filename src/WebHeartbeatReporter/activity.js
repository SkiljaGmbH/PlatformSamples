// -- config --------------------------------------------------------------------------
BASE_AUTH_URL = 'https://your-auth-service/legacy';
BASE_RT_URL = 'https://localhost/api/processservice/api/v2.1/processService';

//--Please make sure to use correct client ID and secret!--
const CLIENT_ID = "Web_Samples_Legacy";
const CLIENT_SECRET = "EAAAACVLNseQPFMYexu8gKbOvdWN2ak3u2FzOUHGwy1MBhlGQ1FJgRHvZq7fzXF5LZDFog==";
const MACHINE_NAME = "SampleVanillaJSHBReporter";
const HB_INTERVAL = 3000;
const HB_COUNT = 5;

// -- util --------------------------------------------------------------------------
STOP = false;
TOKEN = "";
TIMER = null;
TIMER_LOCK = null;
LOOP = false;
ACTTYPE_ID = 0; //9;
PROC_ID = 0; // 32;
_wi = null;
USER_TRACK = false;

function rest(method, url, ct, body, okstatus, cb, cberr) {
    var xhttp = new XMLHttpRequest();
    xhttp.onreadystatechange = function () {
        if (xhttp.readyState === XMLHttpRequest.DONE) {
            if (xhttp.status === okstatus) {
                cb(this);
            } else {
                cberr(this);
            }
        }
    };
    xhttp.open(method, url, true);
    xhttp.setRequestHeader("Content-Type", ct);
    if (TOKEN !== null) {
        xhttp.setRequestHeader("authorization", "Bearer " + TOKEN.access_token);
    }
    xhttp.send(body, true);    
}

function log(msg) {
    console.log(msg);
    document.getElementById("_console").innerHTML =
        document.getElementById("_console").innerHTML + "<br>" + msg
    ; 
}


function cbError(xhttp) {
    log(xhttp.statusText + ": " + xhttp.responseText);
    // log error
}

function encodeData(data) {
    var result = [];
    for (var p in data)
        result.push(encodeURIComponent(p) + "=" + encodeURIComponent(data[p]));
    return result.join("&");
}

// -- login ---------------------------------------------------------------------------
function cbLogin(xhttp) {
    var result = JSON.parse(xhttp.responseText);
    TOKEN = result;
    log(xhttp.statusText + ": " + "Login successful.");
    lockNextWorkItem();
}

function login(username, password) {
    var data = {
        grant_type: "password",
        username: username,
        password: password,
        client_id: CLIENT_ID,
        client_secret: CLIENT_SECRET,
        machine_name: MACHINE_NAME
    };
    log("Login as [" + data.username + "] in progress ...");
    rest(
        "POST",
        BASE_AUTH_URL + "/token",
        "application/x-www-form-urlencoded",
        encodeData(data), 200,
        cbLogin, cbError
    );
}

// -- WorkItems -------------------------------------------------------------------------------
function cbLockNextWorkItem(xhttp) {    
    TIMER_LOCK = null;
    var wi = JSON.parse(xhttp.responseText);
    if (wi == null) {
        log(xhttp.statusText + ": " + "WorkItem is not available. Retrying in 5 sec ...");    
        TIMER_LOCK = window.setTimeout(function(){lockNextWorkItem() }, 5000);
    } else {
        log(xhttp.statusText + ": " + "Locking of work item [ID=" + wi.WorkItemID + "] successful.");    
        startReporting(wi);
    }    
}

function lockNextWorkItem() {
    log("Lock work item in progress ...");
	let dtoWorkItemLockFilter = {
        ActivityTypeIDs: [parseInt(ACTTYPE_ID)],
        ProcessIDs: [parseInt(PROC_ID)],
        ProcessTypes: null,
        ExcludedInstanceIDs: null,
	};
    rest(
        "POST",
        BASE_RT_URL + "/WorkItems/lock" + (USER_TRACK === true ? "?userTracking=true" : ""), 
        "application/json",
        JSON.stringify(dtoWorkItemLockFilter), 200, cbLockNextWorkItem, cbError
    );
}

function cbReleaseWorkItem(xhttp) {
    log(xhttp.statusText + ": " + "Releasing successful.");        
    _wi = null;
    if (!STOP) lockNextWorkItem();
}

function releaseWorkItem() {
    if (_wi == null) {
        return;
    }
    rest(
        "POST",
        BASE_RT_URL + "/WorkItems/Move" + (USER_TRACK === true ? "?userTracking=true" : ""),
        "application/json",
        JSON.stringify(_wi),
        200, cbReleaseWorkItem, cbError
    );
}

// -- Heartbeat ------------------------------------------------------------
function cbSendHeartbeat(xhttp) {
    log(xhttp.statusText + ": " + "Heartbeat sent.");        
    //var result = JSON.parse(xhttp.responseText);    
}

function sendHeartbeat(wi) {
    log("Sending Heartbeat in progress ...");
    var hbit = JSON.stringify({
        WorkItemID: wi.WorkItemID,
        TimeStamp: wi.TimeStamp,
        ReportedAt: new Date(),
        ActivityMessage: "Demo web activity reporting heartbeat."
    });
    log("Heartbeat: " + hbit);
    rest(
        "POST",
        BASE_RT_URL + "/Heartbeats",
        "application/json",
        hbit, 200, cbSendHeartbeat, cbError
    );
}

// -- Heartbeat loop -------------------------------------------------------
function Run(uname, pass, pid, aid, userTrack) {
    STOP = false;
    ACTTYPE_ID = aid;
    PROC_ID = pid;
	USER_TRACK = userTrack;
    login(uname, pass);
}

function startReporting(wi) {    
    _wi = wi;    
    _wi._HbCount = 0;
    TIMER = window.setInterval(function () {        
        _wi._HbCount++;
        sendHeartbeat(_wi);
    }, HB_INTERVAL);    
}

function endReporting() {
    log("Stop processing of work item.");
    if (TIMER_LOCK !== null) {
        window.clearInterval(TIMER_LOCK);
        TIMER_LOCK = null;
    }

    if (TIMER !== null) {
        window.clearInterval(TIMER);
        TIMER = null;
    }

    if (_wi !== null) {
        releaseWorkItem(_wi);
    } else {
        if (!STOP) lockNextWorkItem();
    }
}

function Next() {
    endReporting();
}

function Quit() {
    STOP = true;
    endReporting();
    log("Quit.");
}






