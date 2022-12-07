
function toMonthName(monthNumber) {
    const date = new Date();
    date.setMonth(monthNumber - 1);
  
    return date.toLocaleString('en-US', {
      month: 'long',
    });
  }

var app = new Vue({
    el: '#app',
    delimiters: ['[[', ']]'],
    vuetify : new Vuetify({
        theme: { dark: true },
    }),
    data : {
        players : [],
        bike_types : [{"name" : "Enduro", "val" : 0},  {"name" : "Hardtail", "val" : 2}, {"name" : "Downhill", "val" : 1}],
        commands: [
            {"eval" : "RESPAWN_AT_START", "name": "Respawn at Start", "risk": "high"},
            {"eval": "RESPAWN_ON_TRACK", "name": "Respawn on Track", "risk": "high"},
            {"eval": "CUT_BRAKES", "name": "Cut Brakes", "risk": "medium"},
            {"eval": "BANNED|CRASH", "name": "Crash Game", "risk": "extreme"},
            {"eval": "BANNED|CLOSE", "name": "Close Game", "risk": "extreme"},
            {"eval": "TOGGLE_SPECTATOR", "name": "Make 'em dissapear", "risk": "high"},
            {"eval": "CLEAR_SESSION_MARKER", "name": "Clear Session Marker", "risk": "low"},
            {"eval": "RESET_PLAYER",  "name": "Reset Player", "risk": "high"},
            {"eval": "TOGGLE_COLLISION", "name": "Toggle Collision", "risk": "low"},
            {"eval": "SET_VEL|500", "name": "Into Orbit", "risk": "high"},
            {"eval": "ENABLE_STATS", "name": "Enable Stats", "risk": "low"},
            {"eval": "TOGGLE_GOD", "name": "Activate Anticheat", "risk": "extreme"},
            {"eval": "BAIL", "name": "Kill.", "risk": "high"},
            {"eval": "SET_BIKE_SIZE|0.5", "name": "half player&bike size", "risk": "high"},
            {"eval": "SET_BIKE_SIZE|2", "name": "double player&bike size", "risk": "high"},
            {"eval": "RIDERSGATE|0.5", "name": "Trigger BMX Gate", "risk": "low"},
            {"eval": "RIDERSGATE|0.5", "name": "Trigger BMX Gate", "risk": "low"},
            {"eval": "SET_FAR_CLIP|0.5", "name": "bind 'em", "risk": "low"},
            {"eval": "SET_FAR_CLIP|500", "name": "carrot 'em", "risk": "low"},
            {"eval": "INVALIDATE_TIME", "name": "invalidate time", "risk": "low"},
            {"eval": "SET_VEL|0", "name": "stop 'em", "risk": "low"},
        ],
        controlled_player : null,
        validated: "UNAUTHORISED",
        times: [],
        trails: [],
        self: {}, // the object of the player currently logged in
        tab: 0, // the current tab selected
        search: null, // the current search being used to filter players
        cached_output_log: "", // the current output log being viewed
    },
    methods: {
        RunCommand(player, command){
            if (command.risk == "extreme"){
                confirmed = confirm("the command '"+command.name+"' has a risk rating of extreme - are you sure you want to run this?");
                if (!confirmed)
                    return;
            }
            if (player == "ALL"){
                this.players.forEach(id => {
                    if (id.id != "ALL")
                        this.RunCommand(player.id, command.eval);
                });
            }
            else
                $.ajax({
                    url: "/eval/" + player.id + "?order=" + command.eval,
                    type: "GET",
                    success: function(data){},
                    error: function(data){
                        alert("RunCommand(); failed - are you sure you're logged in?");
                    }
                });
        },
        GetPlayerOutputLog(player){
            $.get("/get-output-log/" + player.id, function(data){
                app.cached_output_log = data;
            });
        },
        GetTrails(){
            $.get("/get-trails", function(data){
                app.trails = data["trails"];
            })
        },
        UpdateTimeIgnore(time){
            $.get("/ignore-time/" + time.time_id + "/" + time.ignore, function(text){
                // if failed, revert to previous time ignore.
                if (text != "success")
                    time.ignore = app.InverseStringBool(time.ignore);
            });
        },
        InverseStringBool(value){
            if (value == "True")
                return "False";
            else
                return "True";
        },
        GetWorlds(){
            $.get("/get-worlds", function(data){
                app.worlds = data["worlds"];
            })
        },
        CheckStatus(){
            $.get("/permission", function(data){
                app.validated = data;
            })
        },
        toHours(value){
            const sec = parseInt(value, 10); // convert value to number if it's string
            let hours   = Math.floor(sec / 3600); // get hours
            let minutes = Math.floor((sec - (hours * 3600)) / 60); // get minutes
            let seconds = sec - (hours * 3600) - (minutes * 60); //  get seconds
            // add 0 if value < 10; Example: 2 => 02
            if (hours   < 10) {hours   = "0"+hours;}
            if (minutes < 10) {minutes = "0"+minutes;}
            if (seconds < 10) {seconds = "0"+seconds;}
            return hours+' hrs, '+minutes+' mins, '+seconds+' secs'; // Return is HH : MM : SS
        },
        timeFromTimestamp(stamp){
            return (Date.now()/1000) - stamp
        },
        SearchMatch(search, item){
            if (search == null || search == "")
                return true;
            if (item.name.toLowerCase().includes(search.toLowerCase()))
                return true;
        },
        Spectate(id){
            $.get("/spectate", data={
                "steam_id" : app.self,
                "target_id": id.id,
                "player_name" : id.name
            });

            app.SubmitEval(app.self, "SPECTATE|" + id.name);
        },
        secs_to_str(secs){
            secs = parseFloat(secs);
            d_mins = Math.floor(secs / 60);
            d_secs = Math.floor(secs % 60)
            fraction = secs * 1000;
            fraction = Math.round(fraction % 1000);
            d_mins = d_mins.toString();
            d_secs = d_secs.toString();
            fraction = fraction.toString();
            if (d_mins.length == 1)
                d_mins = "0" + d_mins.toString()
            if (d_secs.length == 1)
                d_secs = "0" + d_secs
            while (fraction.length < 3)
                fraction = "0" + fraction
            return d_mins + ":" + d_secs + "." + fraction
        },
        redirect(url){
            window.location.href = url;
        },
        toggle_ignore_time(time_id, val){
            $.get("/toggle-ignore-time/" + time_id, data={"val": val});
        },
        getLeaderboard(){
            app.times = [];
            $.get("/get-all-times", data={"lim": app.lim}, function(data){
                app.times = data["times"];
            })
        },
        getColourOfBikeSelect(ourBike, setBike){
            if (ourBike == setBike){
                return "green"
            }
            return "black"
        },
        getSteamId(){
            $.get("/me", function(data){
                if (data.connections.code != 0){
                    data.connections.forEach(function(val){
                        if (val["type"] == "steam"){
                            app.self = val["id"];
                        }
                    })
                }
            })
        },
        addCommaToNum(num){
            if (num != null){
                return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
            }
            else{
                return ""
            }
        }
    }
});

let startTime = new Date().getTime();

function updatePlayers() {
    $.getJSON("/get", function(data){
        
        function compare_lname( a, b )
        {
            if (a.name != null && b.name != null){
                if ( a.name.toLowerCase() < b.name.toLowerCase()){
                    return -1;
                }
                if ( a.name.toLowerCase() > b.name.toLowerCase()){
                    return 1;
                }
            }
            return 0;
        }
        //data["players"].sort(compare_lname);
        app.players = data["players"];
        if (app.players[0] == null || app.players[0].id != "ALL"){
            app.players.unshift(
                {
                    "name":"All Players",
                    "id" : "ALL",
                    "command" : "",
                    "address": ["0.0.0.0", 6969],
                    "steam_avatar_src" : "https://dinahjean.files.wordpress.com/2020/04/all.jpg",
                    "time_on_world": 0,
                    "total_time": 0,
                    "time_loaded": startTime/1000,
                    "world_name": "this dashboard",
                    "reputation":  0,
                    "version": "0.0.0",
                    "bike_type": ""
                }
            )
        }
    })
}

//app.setSelf('UNKNOWN');
updatePlayers();
app.CheckStatus();
setInterval(updatePlayers, 1000);
setInterval(app.CheckStatus, 1000);
app.getSteamId();
setInterval(app.getSteamId, 500);
//app.GetConcurrency();
//app.GetTrails();
//app.GetWorlds();
//app.getLeaderboard();
