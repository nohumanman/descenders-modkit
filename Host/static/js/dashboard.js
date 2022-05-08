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
        controlled_player : {},
        loading : false,
        search_value: "",
        self : null,
        command : "",
        valee: "",
    },
    methods: {
        SubmitEval(id, eval_command){
            $.get("/eval/" + id + "?order=" + eval_command);
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
            return hours+':'+minutes+':'+seconds; // Return is HH : MM : SS
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
        changeRouteAll(timeScale){
            if (timeScale == null){
                timeScale = 1;
            }
            this.ids.forEach(function(id){
                app.SubmitEval(id.id, "MODIFY_SPEED|" + timeScale.toString());
            });
        },
        setSelf(steam_id){
            this.self = steam_id
            window.localStorage.setItem('self', steam_id);
        },
        Spectate(id){
            app.SubmitEval(app.self, "SPECTATE|" + id.name);
        }
    }
});

app.self = window.localStorage.getItem('self');

$.getJSON("/get", function(data){app.ids = data["ids"]})
