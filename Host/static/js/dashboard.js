var app = new Vue({
    el: '#app',
    delimiters: ['[[', ']]'],
    vuetify : new Vuetify({
        theme: {
            dark : {
                primary: '#1976D2',
                secondary: '#424242',
                accent: '#82B1FF',
                error: '#FF5252',
                info: '#2196F3',
                success: '#4CAF50',
                warning: '#FFC107',
            }
    },
    }),
    data : {
        ids : [{"name":"nohumanman", "id" : "123123", "command" : "", "steam_avatar_src" : "https://images.unsplash.com/photo-1566275529824-cca6d008f3da?ixlib=rb-1.2.1&ixid=MnwxMjA3fDB8MHxzZWFyY2h8NHx8cGhvdG98ZW58MHx8MHx8&w=1000&q=80"}],
        suggested : ["SPECTATE|nohumanman", "SET_BIKE|1", "FREEZE_PLAYER", "UNFREEZE_PLAYER", "TOGGLE_CONTROL|true", "CLEAR_SESSION_MARKER", "RESET_PLAYER", "ADD_MODIFIER|FAKIEBALANCE", "ADD_MODIFIER|PUMPSTRENGTH", "RESPAWN_ON_TRACK", "RESPAWN_AT_START"],
        controlling : false,
        bike_types : [
            {"name" : "Enduro", "val" : 0},
            {"name" : "Hardtail", "val" : 2},
            {"name" : "Downhill", "val" : 1}
        ],
        commands: [
            {
                "eval" : "RESPAWN_AT_START",
                "name": "Respawn at Start"
            },
            {
                "eval": "RESPAWN_ON_TRACK",
                "name": "Respawn on Track"
            },
            {
                "eval": "CUT_BRAKES",
                "name": "Cut Brakes"
            },
            {
                "eval": "BANNED|CRASH",
                "name": "Crash Game"
            },
            {
                "eval": "BANNED|CLOSE",
                "name": "Close Game"
            },
            {
                "eval": "TOGGLE_SPECTATOR",
                "name": "Make 'em dissapear"
            },
            {
                "eval": "CLEAR_SESSION_MARKER",
                "name": "Clear Session Marker"
            },
            {
                "eval": "RESET_PLAYER",
                "name": "Reset Player"
            },
            {
                "eval": "TOGGLE_COLLISION",
                "name": "Toggle Collision"
            },
            {
                "eval": "SET_VEL|500",
                "name": "Into Orbit"
            },
            {
                "eval": "ENABLE_STATS",
                "name": "Enable Stats"
            },
            {
                "eval": "TOGGLE_GOD",
                "name": "Activate Anticheat"
            },
            {
                "eval": "BAIL",
                "name": "Kill."
            }
        ],
        controlled_player : {},
        loading : false,
        search_value: "",
        self : null,
        command : "",
        valee: "",
        timeScale: 1,
        search: null,
        validated: "UNAUTHORISED",
    },
    methods: {
        SubmitEval(id, eval_command){
            if (id == "ALL"){
                this.ids.forEach(id => {
                    if (id.id != "ALL"){
                        this.SubmitEval(id.id, eval_command);
                    }
                });
            }
            else{
                $.get("/eval/" + id + "?order=" + eval_command);
            }
        },
        CheckStatus(){
            $.get("/permission", function(data){
                app.validated = data;
            })
        },
        stringToColour(str) {
            var hash = 0;
            for (var i = 0; i < str.length; i++) {
              hash = str.charCodeAt(i) + ((hash << 5) - hash);
            }
            var colour = '#';
            for (var i = 0; i < 3; i++) {
              var value = (hash >> (i * 8)) & 0xFF;
              colour += ('00' + value.toString(16)).substr(-2);
            }
            return colour;
        },
        Randomise(){
            $.get("/randomise");
        },
        RidersGate(){
            let num = Math.floor(Math.random() * (5 - 0 + 1) + 0)
            app.ids.forEach(function(id){
                $.get("/eval/" + id.id + "?order=RIDERSGATE|" + num.toString());
            });
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
        killPlayer(){
            let x = confirm('Do you want to kill this player?')
            if (x){
                this.SubmitEval(this.controlled_player.id, "FREEZE_PLAYER")
                this.SubmitEval(this.controlled_player, "UNFREEZE_PLAYER");
            }
        },
        ToggleControl(id){
            try{
                let x = id.paused
            }
            catch(err){
                id["paused"] = false;
            }
            if (id.paused == null || !id.paused) {
                app.SubmitEval(id.id, 'TOGGLE_CONTROL|false');
                id["paused"] = true;
            }
            else{
                app.SubmitEval(id.id, 'TOGGLE_CONTROL|true');
                id["paused"] = false;
            }
        },
        changeRoute(timeScale){
            if (timeScale == null){
                timeScale = 1;
            }
            this.SubmitEval(this.controlled_player.id, "MODIFY_SPEED|" + timeScale.toString());
        },
        setSelf(steam_id){
            this.self = steam_id
            $.get("/submit-steam-id", data={"steam_id": app.self})
            // window.localStorage.setItem('self', steam_id);
        },
        Spectate(id){
            $.get("/spectating", data={"steam_id" : app.self, "player_name" : id.name})

            app.SubmitEval(app.self, "SPECTATE|" + id.name);
        }
    }
});

$.get("/get-steam-id", function(data){
    app.self = data["steam_id"];
    console.log(app.self);
})

// app.self = window.localStorage.getItem('self');


function updatePlayers() {
    $.getJSON("/get", function(data){
        
        function compare_lname( a, b )
        {
        if ( a.name.toLowerCase() < b.name.toLowerCase()){
            return -1;
        }
        if ( a.name.toLowerCase() > b.name.toLowerCase()){
            return 1;
        }
        return 0;
        }
    
        data["ids"].sort(compare_lname);
        app.ids = data["ids"]
        let startTime = new Date().getTime();
        if (app.ids[0] == null || app.ids[0].id != "ALL"){
            app.ids.unshift(
                {
                    "name":"All Players",
                    "id" : "ALL",
                    "command" : "",
                    "steam_avatar_src" : "https://dinahjean.files.wordpress.com/2020/04/all.jpg",
                    "total_time": new Date().getTime()-startTime,
                    "world_name": "this place",
                    "reputation":  420420,
                    "version": "N/A",
                }
            )
        }
    })
}

updatePlayers();
app.CheckStatus();
setInterval(updatePlayers, 1000);
setInterval(app.CheckStatus, 1000);
